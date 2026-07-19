using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SPCharacterController.Editor
{
    /// <summary>
    /// 状态转换表导入窗口 - 将 XLSX 状态矩阵写入角色状态配置资产。
    /// </summary>
    public sealed class TransitionTableImporterWindow : EditorWindow
    {
        private string _xlsxPath = string.Empty;
        private CharacterStateConfigSO _targetConfig;
        private TransitionTableData _previewData;
        private Vector2 _scrollPosition;
        private string _message = string.Empty;
        private MessageType _messageType = MessageType.Info;

        /// <summary>
        /// 打开状态转换表导入窗口。
        /// </summary>
        [MenuItem("Tools/SP Character Controller/Transition Table Importer")]
        public static void Open()
        {
            GetWindow<TransitionTableImporterWindow>("Transition Table Importer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("XLSX 状态转换表", EditorStyles.boldLabel);
            DrawFileField();

            EditorGUI.BeginChangeCheck();
            _targetConfig = (CharacterStateConfigSO)EditorGUILayout.ObjectField(
                new GUIContent("目标配置", "按现有 Nodes 的资产名称映射索引，仅写入 Rules。"),
                _targetConfig,
                typeof(CharacterStateConfigSO),
                false);
            if (EditorGUI.EndChangeCheck())
                ClearPreview();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("解析预览"))
                    Preview();
                using (new EditorGUI.DisabledScope(_previewData == null))
                {
                    if (GUILayout.Button("导入配置"))
                        Import();
                }
            }

            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);
            DrawPreview();
        }

        private void DrawFileField()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent("XLSX 文件", "自动选择首个能与目标配置完整映射的状态转换表。"));
                EditorGUILayout.SelectableLabel(_xlsxPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("选择", GUILayout.Width(60f)))
                {
                    string selectedPath = EditorUtility.OpenFilePanel("选择状态转换表", GetInitialDirectory(), "xlsx");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        _xlsxPath = selectedPath;
                        ClearPreview();
                    }
                }
            }
        }

        private void Preview()
        {
            try
            {
                _previewData = BuildImportData();
                _message = $"解析成功：工作表 {_previewData.WorksheetName}，{_previewData.StateNames.Length} 个状态，{_previewData.Rules.Length} 条可达规则。";
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _previewData = null;
                _message = exception.Message;
                _messageType = MessageType.Error;
            }
        }

        private void Import()
        {
            try
            {
                TransitionTableData data = BuildImportData();
                Undo.RecordObject(_targetConfig, "Import State Transition Table");
                _targetConfig.Rules = data.Rules;
                EditorUtility.SetDirty(_targetConfig);
                AssetDatabase.SaveAssets();

                _previewData = data;
                _message = $"导入完成：已写入 {_targetConfig.name}，共 {data.Rules.Length} 条规则。";
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _message = exception.Message;
                _messageType = MessageType.Error;
            }
        }

        private TransitionTableData BuildImportData()
        {
            if (_targetConfig == null)
                throw new InvalidOperationException("请选择目标 CharacterStateConfigSO。");

            IReadOnlyList<TransitionWorksheetData> worksheets = TransitionTableXlsxReader.ReadWorksheets(_xlsxPath);
            return BuildFirstCompatibleTable(worksheets, _targetConfig.Nodes);
        }

        private static TransitionTableData BuildFirstCompatibleTable(
            IReadOnlyList<TransitionWorksheetData> worksheets,
            IReadOnlyList<StateNodeSO> nodes)
        {
            var errors = new List<string>(worksheets.Count);
            foreach (TransitionWorksheetData worksheet in worksheets)
            {
                try
                {
                    TransitionTableData data = TransitionTableParser.Parse(worksheet.Name, worksheet.Cells);
                    return MapRuleIndexes(data, nodes);
                }
                catch (InvalidDataException exception)
                {
                    errors.Add($"{worksheet.Name}: {exception.Message}");
                }
            }

            throw new InvalidDataException(
                $"工作簿中没有能与目标配置完整映射的状态转换表。{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        private static TransitionTableData MapRuleIndexes(TransitionTableData data, IReadOnlyList<StateNodeSO> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                throw new InvalidDataException("目标配置的 Nodes 为空，请先配置状态节点。");

            var nodeIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    throw new InvalidDataException($"目标配置的 Nodes[{i}] 为空。");
                if (nodeIndexes.ContainsKey(nodes[i].name))
                    throw new InvalidDataException($"目标配置中存在重名状态节点：{nodes[i].name}。");
                nodeIndexes.Add(nodes[i].name, i);
            }

            var tableToConfigIndexes = new int[data.StateNames.Length];
            for (int i = 0; i < data.StateNames.Length; i++)
            {
                tableToConfigIndexes[i] = FindNodeIndex(data.StateNames[i], nodes, nodeIndexes);
            }

            var mappedRules = new StateTransitionRule[data.Rules.Length];
            for (int i = 0; i < data.Rules.Length; i++)
            {
                StateTransitionRule rule = data.Rules[i];
                rule.FromIndex = tableToConfigIndexes[rule.FromIndex];
                rule.ToIndex = tableToConfigIndexes[rule.ToIndex];
                mappedRules[i] = rule;
            }

            var configStateNames = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                configStateNames[i] = nodes[i].name;
            return new TransitionTableData(data.WorksheetName, configStateNames, mappedRules);
        }

        private static int FindNodeIndex(
            string tableStateName,
            IReadOnlyList<StateNodeSO> nodes,
            IReadOnlyDictionary<string, int> nodeIndexes)
        {
            if (nodeIndexes.TryGetValue(tableStateName, out int exactIndex))
                return exactIndex;

            string suffix = "_" + tableStateName;
            var matchingIndexes = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].name.EndsWith(suffix, StringComparison.Ordinal))
                    matchingIndexes.Add(i);
            }

            if (matchingIndexes.Count == 1)
                return matchingIndexes[0];

            if (matchingIndexes.Count > 1)
            {
                var matchingNames = new string[matchingIndexes.Count];
                for (int i = 0; i < matchingIndexes.Count; i++)
                    matchingNames[i] = nodes[matchingIndexes[i]].name;
                throw new InvalidDataException(
                    $"状态 {tableStateName} 匹配到多个节点：{string.Join(", ", matchingNames)}。请在表格中使用完整资产名称。");
            }

            var availableNames = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                availableNames[i] = nodes[i].name;
            throw new InvalidDataException(
                $"目标配置中找不到状态 {tableStateName} 对应的 StateNodeSO。可用节点：{string.Join(", ", availableNames)}。");
        }

        private void DrawPreview()
        {
            if (_previewData == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"规则预览 - {_previewData.WorksheetName}", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (StateTransitionRule rule in _previewData.Rules)
            {
                string fromState = _previewData.StateNames[rule.FromIndex];
                string toState = _previewData.StateNames[rule.ToIndex];
                EditorGUILayout.LabelField($"{fromState} -> {toState}: {rule.Condition}");
            }
            EditorGUILayout.EndScrollView();
        }

        private string GetInitialDirectory()
        {
            if (!string.IsNullOrEmpty(_xlsxPath))
                return Path.GetDirectoryName(_xlsxPath);
            return Application.dataPath;
        }

        private void ClearPreview()
        {
            _previewData = null;
            _message = string.Empty;
        }
    }
}
