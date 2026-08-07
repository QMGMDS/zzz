using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SPCharacter.Core.Editor
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
        [MenuItem("Tools/SPCharacter/Transition Table Importer")]
        public static void Open()
        {
            GetWindow<TransitionTableImporterWindow>("Transition Table Importer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("XLSX 状态转换与打断点表", EditorStyles.boldLabel);
            DrawFileField();

            EditorGUI.BeginChangeCheck();
            _targetConfig = (CharacterStateConfigSO)EditorGUILayout.ObjectField(
                new GUIContent("目标配置", "按现有 Nodes 的资产名称映射索引，将条件与打断点写入 Rules。"),
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
                EditorGUILayout.PrefixLabel(new GUIContent("XLSX 文件", "单元格格式：条件表达式@打断点，如 WantToAttack@0.4；无打断点时省略 @打断点。"));
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
                _message = BuildSuccessMessage("解析成功", _previewData);
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
                _message = $"{BuildSuccessMessage("导入完成", data)} 已写入 {_targetConfig.name}。";
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
                    return MapRuleIds(data, nodes);
                }
                catch (InvalidDataException exception)
                {
                    errors.Add($"{worksheet.Name}: {exception.Message}");
                }
            }

            throw new InvalidDataException(
                $"工作簿中没有能与目标配置完整映射的状态转换表。{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        private static TransitionTableData MapRuleIds(TransitionTableData data, IReadOnlyList<StateNodeSO> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                throw new InvalidDataException("目标配置的 Nodes 为空，请先配置状态节点。");

            var nodesByName = new Dictionary<string, StateNodeSO>(StringComparer.Ordinal);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    throw new InvalidDataException($"目标配置的 Nodes[{i}] 为空。");
                if (nodesByName.ContainsKey(nodes[i].name))
                    throw new InvalidDataException($"目标配置中存在重名状态节点：{nodes[i].name}。");
                nodesByName.Add(nodes[i].name, nodes[i]);
            }

            var tableToNodeId = new string[data.StateNames.Length];
            for (int i = 0; i < data.StateNames.Length; i++)
                tableToNodeId[i] = FindNode(data.StateNames[i], nodes, nodesByName).Id;

            var mappedRules = new StateTransitionRule[data.Rules.Length];
            for (int i = 0; i < data.Rules.Length; i++)
            {
                StateTransitionRule rule = data.Rules[i];
                mappedRules[i] = new StateTransitionRule(
                    tableToNodeId[IndexOfStateName(data.StateNames, rule.FromId)],
                    tableToNodeId[IndexOfStateName(data.StateNames, rule.ToId)],
                    rule.Condition,
                    rule.InterruptPoint);
            }

            var configStateNames = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                configStateNames[i] = nodes[i].Id;
            return new TransitionTableData(data.WorksheetName, configStateNames, mappedRules);
        }

        private static StateNodeSO FindNode(
            string tableStateName,
            IReadOnlyList<StateNodeSO> nodes,
            IReadOnlyDictionary<string, StateNodeSO> nodesByName)
        {
            if (nodesByName.TryGetValue(tableStateName, out StateNodeSO exactNode))
                return exactNode;

            string suffix = "_" + tableStateName;
            var matchingNodes = new List<StateNodeSO>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].name.EndsWith(suffix, StringComparison.Ordinal))
                    matchingNodes.Add(nodes[i]);
            }

            if (matchingNodes.Count == 1)
                return matchingNodes[0];

            if (matchingNodes.Count > 1)
            {
                var matchingNames = new string[matchingNodes.Count];
                for (int i = 0; i < matchingNodes.Count; i++)
                    matchingNames[i] = matchingNodes[i].name;
                throw new InvalidDataException(
                    $"状态 {tableStateName} 匹配到多个节点：{string.Join(", ", matchingNames)}。请在表格中使用完整资产名称。");
            }

            var availableNames = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                availableNames[i] = nodes[i].name;
            throw new InvalidDataException(
                $"目标配置中找不到状态 {tableStateName} 对应的 StateNodeSO。可用节点：{string.Join(", ", availableNames)}。");
        }

        private static int IndexOfStateName(string[] stateNames, string name)
        {
            for (int i = 0; i < stateNames.Length; i++)
                if (string.Equals(stateNames[i], name, StringComparison.Ordinal))
                    return i;
            throw new InvalidDataException($"表格状态名 {name} 不在表头中。");
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
                EditorGUILayout.LabelField(
                    $"{rule.FromId} -> {rule.ToId}: {TransitionTableParser.FormatCondition(rule.Condition)}；打断点 {rule.InterruptPoint:0.###}");
            }
            EditorGUILayout.EndScrollView();
        }

        private static string BuildSuccessMessage(string prefix, TransitionTableData data)
        {
            return $"{prefix}：工作表 {data.WorksheetName}，{data.StateNames.Length} 个状态，{data.Rules.Length} 条可达规则。";
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