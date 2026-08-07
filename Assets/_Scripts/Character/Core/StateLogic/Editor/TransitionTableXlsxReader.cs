using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace SPCharacter.Core.Editor
{
    /// <summary>
    /// XLSX 工作表数据 - 保存工作表名称与二维字符串表。
    /// </summary>
    internal sealed class TransitionWorksheetData
    {
        internal TransitionWorksheetData(string name, string[,] cells)
        {
            Name = name;
            Cells = cells;
        }

        internal string Name { get; }
        internal string[,] Cells { get; }
    }

    /// <summary>
    /// XLSX 状态表读取器 - 读取工作簿中的工作表并转换为二维字符串表。
    /// </summary>
    internal static class TransitionTableXlsxReader
    {
        private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

        internal static IReadOnlyList<TransitionWorksheetData> ReadWorksheets(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("未选择 XLSX 文件。", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到 XLSX 文件。", filePath);
            if (!string.Equals(Path.GetExtension(filePath), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("仅支持 .xlsx 文件，不支持旧版 .xls 文件。");

            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                List<string> sharedStrings = ReadSharedStrings(archive);
                IReadOnlyList<(string Name, string Path)> worksheetInfos = GetWorksheetInfos(archive);
                var worksheets = new List<TransitionWorksheetData>(worksheetInfos.Count);
                foreach ((string name, string path) in worksheetInfos)
                {
                    XmlDocument worksheet = LoadXml(archive, path);
                    worksheets.Add(new TransitionWorksheetData(name, ReadCells(worksheet, sharedStrings)));
                }

                return worksheets;
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            var result = new List<string>();
            if (entry == null)
                return result;

            XmlDocument document = LoadXml(entry);
            XmlNamespaceManager namespaces = CreateNamespaces(document.NameTable);
            XmlNodeList stringItems = document.SelectNodes("/x:sst/x:si", namespaces);
            foreach (XmlNode stringItem in stringItems)
            {
                XmlNodeList textNodes = stringItem.SelectNodes(".//x:t", namespaces);
                string value = string.Empty;
                foreach (XmlNode textNode in textNodes)
                    value += textNode.InnerText;
                result.Add(value);
            }

            return result;
        }

        private static IReadOnlyList<(string Name, string Path)> GetWorksheetInfos(ZipArchive archive)
        {
            XmlDocument workbook = LoadXml(archive, "xl/workbook.xml");
            XmlNamespaceManager workbookNamespaces = CreateNamespaces(workbook.NameTable);
            XmlNodeList sheetNodes = workbook.SelectNodes("/x:workbook/x:sheets/x:sheet", workbookNamespaces);
            if (sheetNodes.Count == 0)
                throw new InvalidDataException("XLSX 工作簿中没有工作表。");

            XmlDocument relationships = LoadXml(archive, "xl/_rels/workbook.xml.rels");
            var relationshipNamespaces = new XmlNamespaceManager(relationships.NameTable);
            relationshipNamespaces.AddNamespace("r", PackageRelationshipNamespace);
            XmlNodeList relationshipNodes = relationships.SelectNodes("/r:Relationships/r:Relationship", relationshipNamespaces);
            var relationshipTargets = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XmlNode relationshipNode in relationshipNodes)
            {
                string id = relationshipNode.Attributes?["Id"]?.Value;
                string target = relationshipNode.Attributes?["Target"]?.Value;
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target))
                    relationshipTargets[id] = target;
            }

            var result = new List<(string Name, string Path)>(sheetNodes.Count);
            foreach (XmlNode sheetNode in sheetNodes)
            {
                string sheetName = sheetNode.Attributes?["name"]?.Value;
                XmlAttribute relationshipAttribute = sheetNode.Attributes?["id", RelationshipNamespace];
                if (string.IsNullOrEmpty(sheetName) || relationshipAttribute == null)
                    throw new InvalidDataException("无法读取工作表名称或关系标识。");
                if (!relationshipTargets.TryGetValue(relationshipAttribute.Value, out string target))
                    throw new InvalidDataException($"无法定位工作表 {sheetName} 的文件。");

                string normalizedTarget = target.Replace('\\', '/');
                string path = normalizedTarget.StartsWith("/", StringComparison.Ordinal)
                    ? normalizedTarget.TrimStart('/')
                    : NormalizeArchivePath("xl/" + normalizedTarget);
                result.Add((sheetName, path));
            }

            return result;
        }

        private static string[,] ReadCells(XmlDocument worksheet, IReadOnlyList<string> sharedStrings)
        {
            XmlNamespaceManager namespaces = CreateNamespaces(worksheet.NameTable);
            XmlNodeList cellNodes = worksheet.SelectNodes("/x:worksheet/x:sheetData/x:row/x:c", namespaces);
            if (cellNodes.Count == 0)
                return new string[0, 0];

            int maxRow = 0;
            int maxColumn = 0;
            var values = new Dictionary<(int Row, int Column), string>();

            foreach (XmlNode cellNode in cellNodes)
            {
                string reference = cellNode.Attributes?["r"]?.Value;
                if (string.IsNullOrEmpty(reference))
                    continue;

                GetCellPosition(reference, out int row, out int column);
                string value = ReadCellValue(cellNode, namespaces, sharedStrings);
                if (string.IsNullOrEmpty(value))
                    continue;

                maxRow = Math.Max(maxRow, row);
                maxColumn = Math.Max(maxColumn, column);
                values[(row, column)] = value;
            }

            var result = new string[maxRow, maxColumn];
            foreach (KeyValuePair<(int Row, int Column), string> pair in values)
                result[pair.Key.Row - 1, pair.Key.Column - 1] = pair.Value;
            return result;
        }

        private static string ReadCellValue(
            XmlNode cellNode,
            XmlNamespaceManager namespaces,
            IReadOnlyList<string> sharedStrings)
        {
            string cellType = cellNode.Attributes?["t"]?.Value;
            if (cellType == "inlineStr")
            {
                XmlNodeList textNodes = cellNode.SelectNodes("x:is//x:t", namespaces);
                string inlineValue = string.Empty;
                foreach (XmlNode textNode in textNodes)
                    inlineValue += textNode.InnerText;
                return inlineValue;
            }

            string rawValue = cellNode.SelectSingleNode("x:v", namespaces)?.InnerText ?? string.Empty;
            if (cellType != "s")
                return rawValue;

            if (!int.TryParse(rawValue, NumberStyles.None, CultureInfo.InvariantCulture, out int sharedStringIndex) ||
                sharedStringIndex < 0 || sharedStringIndex >= sharedStrings.Count)
                throw new InvalidDataException($"共享字符串索引无效：{rawValue}。");

            return sharedStrings[sharedStringIndex];
        }

        private static void GetCellPosition(string reference, out int row, out int column)
        {
            int separator = 0;
            while (separator < reference.Length && char.IsLetter(reference[separator]))
                separator++;
            if (separator == 0 || separator == reference.Length ||
                !int.TryParse(reference.Substring(separator), out row))
                throw new InvalidDataException($"单元格坐标无效：{reference}。");

            column = 0;
            for (int i = 0; i < separator; i++)
                column = column * 26 + char.ToUpperInvariant(reference[i]) - 'A' + 1;
        }

        private static XmlDocument LoadXml(ZipArchive archive, string entryPath)
        {
            ZipArchiveEntry entry = archive.GetEntry(entryPath);
            if (entry == null)
                throw new InvalidDataException($"XLSX 内缺少文件：{entryPath}。");
            return LoadXml(entry);
        }

        private static XmlDocument LoadXml(ZipArchiveEntry entry)
        {
            var document = new XmlDocument { XmlResolver = null };
            using (Stream stream = entry.Open())
            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { XmlResolver = null }))
                document.Load(reader);
            return document;
        }

        private static XmlNamespaceManager CreateNamespaces(XmlNameTable nameTable)
        {
            var namespaces = new XmlNamespaceManager(nameTable);
            namespaces.AddNamespace("x", SpreadsheetNamespace);
            namespaces.AddNamespace("rel", RelationshipNamespace);
            return namespaces;
        }

        private static string NormalizeArchivePath(string path)
        {
            var parts = new List<string>();
            foreach (string part in path.Split('/'))
            {
                if (part == "..")
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                    continue;
                }

                if (part != "." && part.Length > 0)
                    parts.Add(part);
            }

            return string.Join("/", parts);
        }
    }
}
