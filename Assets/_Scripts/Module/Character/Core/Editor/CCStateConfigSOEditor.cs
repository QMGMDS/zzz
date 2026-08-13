#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

using UnityEditor;
using UnityEngine;

namespace SPCharacter.Core.Editor
{
    /// <summary>
    /// CCStateConfigSO Inspector 扩展 - 支持从 Excel 状态矩阵导入转移规则
    /// </summary>
    [CustomEditor(typeof(CCStateConfigSO))]
    internal sealed class CCStateConfigSOEditor : UnityEditor.Editor
    {
        private const string RulesPropertyName = "_rules";
        private const string FromIdPropertyName = "_fromId";
        private const string ToIdPropertyName = "_toId";
        private const string ConditionPropertyName = "_condition";
        private const string RequiredPropertyName = "_required";
        private const string ForbiddenPropertyName = "_forbidden";
        private const string InterruptPointPropertyName = "_interruptPoint";
        private const string PriorityPropertyName = "_priority";

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                if (GUILayout.Button("从 Excel 导入状态转移规则（.xlsx）"))
                    ImportRulesFromXlsx();
            }

            if (targets.Length != 1)
                EditorGUILayout.HelpBox("多选时不可导入 Excel，请只选择一个 CCStateConfigSO。", MessageType.Info);
        }

        private void ImportRulesFromXlsx()
        {
            CCStateConfigSO config = (CCStateConfigSO)target;
            string xlsxPath = EditorUtility.OpenFilePanel("导入状态转移矩阵", Application.dataPath, "xlsx");
            if (string.IsNullOrEmpty(xlsxPath))
                return;

            try
            {
                StateTransitionRule[] rules = CCStateConfigExcelImporter.ImportRules(xlsxPath, config);
                ApplyRules(config, rules);
                Debug.Log($"{config.name}: 已从 Excel 导入 {rules.Length} 条状态转移规则。", config);
                EditorUtility.DisplayDialog("导入完成", $"已导入 {rules.Length} 条状态转移规则。", "确定");
            }
            catch (Exception exception)
            {
                string message = $"导入状态转移矩阵失败：{exception.Message}";
                Debug.LogError(message, config);
                EditorUtility.DisplayDialog("导入失败", message, "确定");
            }
        }

        private void ApplyRules(CCStateConfigSO config, IReadOnlyList<StateTransitionRule> rules)
        {
            Undo.RecordObject(config, "Import CC State Transition Rules");

            serializedObject.Update();
            SerializedProperty rulesProperty = serializedObject.FindProperty(RulesPropertyName);
            rulesProperty.arraySize = rules.Count;

            for (int i = 0; i < rules.Count; i++)
            {
                StateTransitionRule rule = rules[i];
                SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
                ruleProperty.FindPropertyRelative(FromIdPropertyName).stringValue = rule.FromId;
                ruleProperty.FindPropertyRelative(ToIdPropertyName).stringValue = rule.ToId;
                ruleProperty.FindPropertyRelative(InterruptPointPropertyName).floatValue = rule.InterruptPoint;
                ruleProperty.FindPropertyRelative(PriorityPropertyName).intValue = rule.Priority;

                SerializedProperty conditionProperty = ruleProperty.FindPropertyRelative(ConditionPropertyName);
                conditionProperty.FindPropertyRelative(RequiredPropertyName).intValue = (int)rule.Condition.Required;
                conditionProperty.FindPropertyRelative(ForbiddenPropertyName).intValue = (int)rule.Condition.Forbidden;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// CC 状态配置 Excel 导入器 - 将 .xlsx 状态矩阵解析为状态转移规则
    /// </summary>
    internal static class CCStateConfigExcelImporter
    {
        private const string MainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string DocumentRelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private const string WorkbookPath = "xl/workbook.xml";
        private const string WorkbookRelsPath = "xl/_rels/workbook.xml.rels";
        private const string XlsxExtension = ".xlsx";
        private const string NoRuleToken = "None";
        private const char ConditionSeparator = '+';
        private const char ForbiddenPrefix = '!';
        private const char InterruptPointPrefix = '@';
        private const char PriorityPrefix = '#';

        private static readonly XNamespace SpreadsheetNs = MainNamespace;
        private static readonly XNamespace DocumentRelNs = DocumentRelationshipNamespace;
        private static readonly XNamespace PackageRelNs = PackageRelationshipNamespace;

        /// <summary>
        /// 从 .xlsx 状态矩阵导入状态转移规则
        /// </summary>
        /// <param name="xlsxPath">Excel 文件路径</param>
        /// <param name="config">目标状态配置</param>
        /// <returns>解析得到的状态转移规则数组</returns>
        public static StateTransitionRule[] ImportRules(string xlsxPath, CCStateConfigSO config)
        {
            if (string.IsNullOrWhiteSpace(xlsxPath))
                throw new ArgumentException("Excel 文件路径为空。", nameof(xlsxPath));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (!string.Equals(Path.GetExtension(xlsxPath), XlsxExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("仅支持 .xlsx 文件。");
            if (!File.Exists(xlsxPath))
                throw new FileNotFoundException("找不到 Excel 文件。", xlsxPath);

            HashSet<string> nodeIds = BuildNodeIdSet(config);
            XlsxWorksheetData worksheetData = ReadFirstWorksheet(xlsxPath);
            return ParseRules(worksheetData, nodeIds);
        }

        private static HashSet<string> BuildNodeIdSet(CCStateConfigSO config)
        {
            StateNodeSO[] nodes = config.Nodes;
            if (nodes == null || nodes.Length == 0)
                throw new InvalidDataException("目标 CCStateConfigSO 没有配置状态节点，无法校验 Excel 状态 Id。");

            HashSet<string> nodeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < nodes.Length; i++)
            {
                StateNodeSO node = nodes[i];
                if (node == null)
                    throw new InvalidDataException($"目标 CCStateConfigSO 的 Nodes[{i}] 为空。");
                if (string.IsNullOrEmpty(node.Id))
                    throw new InvalidDataException($"目标 CCStateConfigSO 的 Nodes[{i}].Id 为空。");
                if (!nodeIds.Add(node.Id))
                    throw new InvalidDataException($"目标 CCStateConfigSO 存在重复状态 Id：{node.Id}。");
            }

            return nodeIds;
        }

        private static StateTransitionRule[] ParseRules(XlsxWorksheetData worksheetData, HashSet<string> nodeIds)
        {
            Dictionary<int, string> toIdsByColumn = ParseToHeaders(worksheetData, nodeIds);
            ValidateBlankHeaderColumns(worksheetData, toIdsByColumn);

            List<StateTransitionRule> rules = new List<StateTransitionRule>();
            HashSet<string> fromIds = new HashSet<string>(StringComparer.Ordinal);
            for (int row = 2; row <= worksheetData.MaxRow; row++)
            {
                string fromId = NormalizeCellText(worksheetData.GetCell(row, 1));
                if (string.IsNullOrEmpty(fromId))
                {
                    if (HasAnyRuleCellValue(worksheetData, toIdsByColumn, row))
                        throw new InvalidDataException($"{FormatCell(row, 1)} 缺少 From 状态 Id。");

                    continue;
                }

                if (!fromIds.Add(fromId))
                    throw new InvalidDataException($"{FormatCell(row, 1)} 存在重复 From 状态 Id：{fromId}。");
                if (!nodeIds.Contains(fromId))
                    throw new InvalidDataException($"{FormatCell(row, 1)} 指向不存在的 From 状态 Id：{fromId}。");

                foreach (KeyValuePair<int, string> pair in toIdsByColumn)
                {
                    int column = pair.Key;
                    string toId = pair.Value;
                    string cellText = NormalizeCellText(worksheetData.GetCell(row, column));
                    if (IsNoRuleCell(cellText))
                        continue;

                    rules.Add(ParseRuleCell(fromId, toId, cellText, row, column));
                }
            }

            return rules.ToArray();
        }

        private static Dictionary<int, string> ParseToHeaders(XlsxWorksheetData worksheetData, HashSet<string> nodeIds)
        {
            Dictionary<int, string> toIdsByColumn = new Dictionary<int, string>();
            HashSet<string> toIds = new HashSet<string>(StringComparer.Ordinal);

            for (int column = 2; column <= worksheetData.MaxColumn; column++)
            {
                string toId = NormalizeCellText(worksheetData.GetCell(1, column));
                if (string.IsNullOrEmpty(toId))
                    continue;
                if (!toIds.Add(toId))
                    throw new InvalidDataException($"{FormatCell(1, column)} 存在重复 To 状态 Id：{toId}。");
                if (!nodeIds.Contains(toId))
                    throw new InvalidDataException($"{FormatCell(1, column)} 指向不存在的 To 状态 Id：{toId}。");

                toIdsByColumn.Add(column, toId);
            }

            if (toIdsByColumn.Count == 0)
                throw new InvalidDataException("Excel 第一行没有找到任何 To 状态 Id。请从 B1 开始填写目标状态 Id。");

            return toIdsByColumn;
        }

        private static void ValidateBlankHeaderColumns(XlsxWorksheetData worksheetData, IReadOnlyDictionary<int, string> toIdsByColumn)
        {
            for (int column = 2; column <= worksheetData.MaxColumn; column++)
            {
                if (toIdsByColumn.ContainsKey(column))
                    continue;

                for (int row = 2; row <= worksheetData.MaxRow; row++)
                {
                    string cellText = NormalizeCellText(worksheetData.GetCell(row, column));
                    if (string.IsNullOrEmpty(cellText))
                        continue;

                    throw new InvalidDataException($"{FormatCell(1, column)} 表头为空，但 {FormatCell(row, column)} 存在配置内容。");
                }
            }
        }

        private static bool HasAnyRuleCellValue(
            XlsxWorksheetData worksheetData,
            IReadOnlyDictionary<int, string> toIdsByColumn,
            int row)
        {
            foreach (KeyValuePair<int, string> pair in toIdsByColumn)
            {
                if (!string.IsNullOrEmpty(NormalizeCellText(worksheetData.GetCell(row, pair.Key))))
                    return true;
            }

            return false;
        }

        private static StateTransitionRule ParseRuleCell(string fromId, string toId, string cellText, int row, int column)
        {
            string remainingText = cellText;
            int priority = 0;
            float interruptPoint = 0f;

            int priorityIndex = remainingText.IndexOf(PriorityPrefix);
            if (priorityIndex >= 0)
            {
                if (remainingText.IndexOf(PriorityPrefix, priorityIndex + 1) >= 0)
                    throw new InvalidDataException($"{FormatCell(row, column)} 存在多个 # 优先级标记。");

                string priorityText = remainingText.Substring(priorityIndex + 1).Trim();
                if (!int.TryParse(priorityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out priority))
                    throw new InvalidDataException($"{FormatCell(row, column)} 的优先级不是有效整数：{priorityText}。");

                remainingText = remainingText.Substring(0, priorityIndex).Trim();
            }

            int interruptPointIndex = remainingText.IndexOf(InterruptPointPrefix);
            if (interruptPointIndex >= 0)
            {
                if (remainingText.IndexOf(InterruptPointPrefix, interruptPointIndex + 1) >= 0)
                    throw new InvalidDataException($"{FormatCell(row, column)} 存在多个 @ 打断点标记。");

                string interruptPointText = remainingText.Substring(interruptPointIndex + 1).Trim();
                if (!float.TryParse(interruptPointText, NumberStyles.Float, CultureInfo.InvariantCulture, out interruptPoint))
                    throw new InvalidDataException($"{FormatCell(row, column)} 的打断点不是有效数字：{interruptPointText}。");
                if (float.IsNaN(interruptPoint) || interruptPoint < 0f || interruptPoint > 1f)
                    throw new InvalidDataException($"{FormatCell(row, column)} 的打断点必须位于 0 到 1 之间：{interruptPointText}。");

                remainingText = remainingText.Substring(0, interruptPointIndex).Trim();
            }

            if (remainingText.IndexOf(PriorityPrefix) >= 0 || remainingText.IndexOf(InterruptPointPrefix) >= 0)
                throw new InvalidDataException($"{FormatCell(row, column)} 的 @ 或 # 标记只能位于单元格末尾。");

            StateTransitionCondition condition = ParseCondition(remainingText, row, column);
            return new StateTransitionRule(fromId, toId, condition, interruptPoint, priority);
        }

        private static StateTransitionCondition ParseCondition(string conditionText, int row, int column)
        {
            if (string.IsNullOrWhiteSpace(conditionText))
                return new StateTransitionCondition(CCIntention.None, CCIntention.None);

            CCIntention required = CCIntention.None;
            CCIntention forbidden = CCIntention.None;
            string[] conditionTokens = conditionText.Split(ConditionSeparator);
            for (int i = 0; i < conditionTokens.Length; i++)
            {
                string token = conditionTokens[i].Trim();
                if (string.IsNullOrEmpty(token))
                    throw new InvalidDataException($"{FormatCell(row, column)} 存在空条件片段。");

                bool isForbidden = token[0] == ForbiddenPrefix;
                if (isForbidden)
                    token = token.Substring(1).Trim();
                if (string.IsNullOrEmpty(token))
                    throw new InvalidDataException($"{FormatCell(row, column)} 的 ! 后缺少意图名称。");
                if (!Enum.TryParse(token, false, out CCIntention intention) || intention == CCIntention.None)
                    throw new InvalidDataException($"{FormatCell(row, column)} 包含未知意图名称：{token}。");

                if (isForbidden)
                    forbidden |= intention;
                else
                    required |= intention;
            }

            if ((required & forbidden) != CCIntention.None)
                throw new InvalidDataException($"{FormatCell(row, column)} 同一个意图不能同时要求和禁止。");

            return new StateTransitionCondition(required, forbidden);
        }

        private static bool IsNoRuleCell(string cellText)
        {
            return string.IsNullOrEmpty(cellText) ||
                   string.Equals(cellText, NoRuleToken, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCellText(string cellText)
        {
            return string.IsNullOrWhiteSpace(cellText) ? string.Empty : cellText.Trim();
        }

        private static XlsxWorksheetData ReadFirstWorksheet(string xlsxPath)
        {
            using (FileStream fileStream = File.OpenRead(xlsxPath))
            using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                List<string> sharedStrings = ReadSharedStrings(archive);
                string worksheetPath = ResolveFirstWorksheetPath(archive);
                ZipArchiveEntry worksheetEntry = GetRequiredEntry(archive, worksheetPath);

                using (Stream stream = worksheetEntry.Open())
                {
                    XDocument worksheetDocument = XDocument.Load(stream);
                    return ReadWorksheetData(worksheetDocument, sharedStrings);
                }
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
            List<string> sharedStrings = new List<string>();
            if (sharedStringsEntry == null)
                return sharedStrings;

            using (Stream stream = sharedStringsEntry.Open())
            {
                XDocument sharedStringsDocument = XDocument.Load(stream);
                XElement root = sharedStringsDocument.Root;
                if (root == null)
                    return sharedStrings;

                foreach (XElement item in root.Elements(SpreadsheetNs + "si"))
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (XElement textElement in item.Descendants(SpreadsheetNs + "t"))
                    {
                        builder.Append(textElement.Value);
                    }

                    sharedStrings.Add(builder.ToString());
                }
            }

            return sharedStrings;
        }

        private static string ResolveFirstWorksheetPath(ZipArchive archive)
        {
            ZipArchiveEntry workbookEntry = GetRequiredEntry(archive, WorkbookPath);
            ZipArchiveEntry workbookRelsEntry = GetRequiredEntry(archive, WorkbookRelsPath);
            string firstSheetRelationshipId = ReadFirstSheetRelationshipId(workbookEntry);
            string firstSheetTarget = ReadRelationshipTarget(workbookRelsEntry, firstSheetRelationshipId);
            return NormalizeZipPath(ResolveWorkbookTarget(firstSheetTarget));
        }

        private static string ReadFirstSheetRelationshipId(ZipArchiveEntry workbookEntry)
        {
            using (Stream stream = workbookEntry.Open())
            {
                XDocument workbookDocument = XDocument.Load(stream);
                XElement root = workbookDocument.Root;
                if (root == null)
                    throw new InvalidDataException("Excel 工作簿缺少 workbook 根节点。");

                XElement sheetsElement = root.Element(SpreadsheetNs + "sheets");
                if (sheetsElement == null)
                    throw new InvalidDataException("Excel 工作簿没有工作表。");

                foreach (XElement sheetElement in sheetsElement.Elements(SpreadsheetNs + "sheet"))
                {
                    string relationshipId = (string)sheetElement.Attribute(DocumentRelNs + "id");
                    if (!string.IsNullOrEmpty(relationshipId))
                        return relationshipId;
                }
            }

            throw new InvalidDataException("Excel 工作簿没有可读取的工作表关系 Id。");
        }

        private static string ReadRelationshipTarget(ZipArchiveEntry workbookRelsEntry, string relationshipId)
        {
            using (Stream stream = workbookRelsEntry.Open())
            {
                XDocument relsDocument = XDocument.Load(stream);
                XElement root = relsDocument.Root;
                if (root == null)
                    throw new InvalidDataException("Excel 工作簿关系文件缺少根节点。");

                foreach (XElement relationshipElement in root.Elements(PackageRelNs + "Relationship"))
                {
                    string id = (string)relationshipElement.Attribute("Id");
                    if (!string.Equals(id, relationshipId, StringComparison.Ordinal))
                        continue;

                    string target = (string)relationshipElement.Attribute("Target");
                    if (string.IsNullOrEmpty(target))
                        throw new InvalidDataException($"Excel 工作表关系 {relationshipId} 缺少 Target。");

                    return target;
                }
            }

            throw new InvalidDataException($"Excel 工作簿找不到工作表关系：{relationshipId}。");
        }

        private static string ResolveWorkbookTarget(string target)
        {
            string normalizedTarget = target.Replace('\\', '/');
            if (normalizedTarget.StartsWith("/", StringComparison.Ordinal))
                return normalizedTarget.TrimStart('/');

            return "xl/" + normalizedTarget;
        }

        private static XlsxWorksheetData ReadWorksheetData(XDocument worksheetDocument, IReadOnlyList<string> sharedStrings)
        {
            XlsxWorksheetData worksheetData = new XlsxWorksheetData();
            XElement root = worksheetDocument.Root;
            if (root == null)
                throw new InvalidDataException("Excel 工作表缺少根节点。");

            foreach (XElement cellElement in root.Descendants(SpreadsheetNs + "c"))
            {
                string reference = (string)cellElement.Attribute("r");
                if (!TryParseCellReference(reference, out int row, out int column))
                    continue;

                string cellText = ReadCellText(cellElement, sharedStrings);
                worksheetData.SetCell(row, column, cellText);
            }

            return worksheetData;
        }

        private static string ReadCellText(XElement cellElement, IReadOnlyList<string> sharedStrings)
        {
            string cellType = (string)cellElement.Attribute("t");
            if (string.Equals(cellType, "s", StringComparison.Ordinal))
                return ReadSharedStringCell(cellElement, sharedStrings);
            if (string.Equals(cellType, "inlineStr", StringComparison.Ordinal))
                return ReadInlineStringCell(cellElement);

            XElement valueElement = cellElement.Element(SpreadsheetNs + "v");
            return valueElement == null ? string.Empty : valueElement.Value;
        }

        private static string ReadSharedStringCell(XElement cellElement, IReadOnlyList<string> sharedStrings)
        {
            XElement valueElement = cellElement.Element(SpreadsheetNs + "v");
            if (valueElement == null ||
                !int.TryParse(valueElement.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) ||
                index < 0 ||
                index >= sharedStrings.Count)
            {
                throw new InvalidDataException("Excel 包含无效的共享字符串索引。");
            }

            return sharedStrings[index];
        }

        private static string ReadInlineStringCell(XElement cellElement)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XElement textElement in cellElement.Descendants(SpreadsheetNs + "t"))
            {
                builder.Append(textElement.Value);
            }

            return builder.ToString();
        }

        private static ZipArchiveEntry GetRequiredEntry(ZipArchive archive, string entryPath)
        {
            ZipArchiveEntry entry = archive.GetEntry(NormalizeZipPath(entryPath));
            if (entry == null)
                throw new InvalidDataException($"Excel 文件缺少必要条目：{entryPath}。");

            return entry;
        }

        private static string NormalizeZipPath(string path)
        {
            string[] parts = path.Replace('\\', '/').Split('/');
            List<string> normalizedParts = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part) || string.Equals(part, ".", StringComparison.Ordinal))
                    continue;
                if (string.Equals(part, "..", StringComparison.Ordinal))
                {
                    if (normalizedParts.Count > 0)
                        normalizedParts.RemoveAt(normalizedParts.Count - 1);
                    continue;
                }

                normalizedParts.Add(part);
            }

            return string.Join("/", normalizedParts);
        }

        private static bool TryParseCellReference(string reference, out int row, out int column)
        {
            row = 0;
            column = 0;
            if (string.IsNullOrEmpty(reference))
                return false;

            int index = 0;
            while (index < reference.Length && char.IsLetter(reference[index]))
            {
                column = (column * 26) + char.ToUpperInvariant(reference[index]) - 'A' + 1;
                index++;
            }

            while (index < reference.Length && char.IsDigit(reference[index]))
            {
                row = (row * 10) + reference[index] - '0';
                index++;
            }

            return row > 0 && column > 0;
        }

        private static string FormatCell(int row, int column)
        {
            return $"{FormatColumn(column)}{row}";
        }

        private static string FormatColumn(int column)
        {
            StringBuilder builder = new StringBuilder();
            while (column > 0)
            {
                column--;
                builder.Insert(0, (char)('A' + column % 26));
                column /= 26;
            }

            return builder.ToString();
        }

        /// <summary>
        /// 已解析的 Excel 工作表文本数据
        /// </summary>
        private sealed class XlsxWorksheetData
        {
            private readonly Dictionary<int, Dictionary<int, string>> _rows = new Dictionary<int, Dictionary<int, string>>();

            /// <summary>最大有效行号</summary>
            public int MaxRow { get; private set; }

            /// <summary>最大有效列号</summary>
            public int MaxColumn { get; private set; }

            /// <summary>
            /// 获取指定单元格文本
            /// </summary>
            /// <param name="row">一基行号</param>
            /// <param name="column">一基列号</param>
            /// <returns>单元格文本，空单元格返回空字符串</returns>
            public string GetCell(int row, int column)
            {
                if (!_rows.TryGetValue(row, out Dictionary<int, string> columns))
                    return string.Empty;

                return columns.TryGetValue(column, out string value) ? value : string.Empty;
            }

            /// <summary>
            /// 写入指定单元格文本
            /// </summary>
            /// <param name="row">一基行号</param>
            /// <param name="column">一基列号</param>
            /// <param name="value">单元格文本</param>
            public void SetCell(int row, int column, string value)
            {
                if (!_rows.TryGetValue(row, out Dictionary<int, string> columns))
                {
                    columns = new Dictionary<int, string>();
                    _rows.Add(row, columns);
                }

                columns[column] = value;
                MaxRow = Math.Max(MaxRow, row);
                MaxColumn = Math.Max(MaxColumn, column);
            }
        }
    }
}
#endif

