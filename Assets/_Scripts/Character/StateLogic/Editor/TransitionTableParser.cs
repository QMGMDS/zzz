using System;
using System.Collections.Generic;
using System.IO;

namespace SPCharacterController.Editor
{
    /// <summary>
    /// 状态转换表解析结果 - 保存有序状态名称与转换规则。
    /// </summary>
    internal sealed class TransitionTableData
    {
        internal TransitionTableData(string worksheetName, string[] stateNames, StateTransitionRule[] rules)
        {
            WorksheetName = worksheetName;
            StateNames = stateNames;
            Rules = rules;
        }

        internal string WorksheetName { get; }
        internal string[] StateNames { get; }
        internal StateTransitionRule[] Rules { get; }
    }

    /// <summary>
    /// 状态转换表解析器 - 将二维字符串矩阵转换为运行时规则。
    /// </summary>
    internal static class TransitionTableParser
    {
        internal static TransitionTableData Parse(string worksheetName, string[,] cells)
        {
            if (string.IsNullOrWhiteSpace(worksheetName))
                throw new ArgumentException("工作表名称不能为空。", nameof(worksheetName));
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));
            if (cells.GetLength(0) < 2 || cells.GetLength(1) < 2)
                throw new InvalidDataException("状态表至少需要一个来源状态和一个目标状态。");

            string corner = Normalize(cells[0, 0]);
            if (string.IsNullOrEmpty(corner))
                throw new InvalidDataException("状态表左上角单元格 A1 不能为空。");

            int stateCount = cells.GetLength(1) - 1;
            if (cells.GetLength(0) - 1 != stateCount)
                throw new InvalidDataException("状态表必须是方阵，来源状态数量必须等于目标状态数量。");

            var stateNames = new string[stateCount];
            var uniqueNames = new HashSet<string>(StringComparer.Ordinal);
            for (int column = 0; column < stateCount; column++)
            {
                string stateName = Normalize(cells[0, column + 1]);
                if (string.IsNullOrEmpty(stateName))
                    throw new InvalidDataException($"目标状态名称不能为空：{GetCellName(0, column + 1)}。");
                if (!uniqueNames.Add(stateName))
                    throw new InvalidDataException($"状态名称重复：{stateName}。");
                stateNames[column] = stateName;
            }

            var rules = new List<StateTransitionRule>();
            for (int row = 0; row < stateCount; row++)
            {
                string fromState = Normalize(cells[row + 1, 0]);
                if (!string.Equals(fromState, stateNames[row], StringComparison.Ordinal))
                    throw new InvalidDataException(
                        $"来源状态 {GetCellName(row + 1, 0)} 必须与目标状态顺序一致，应为 {stateNames[row]}。");

                for (int column = 0; column < stateCount; column++)
                {
                    string expression = Normalize(cells[row + 1, column + 1]);
                    if (string.IsNullOrEmpty(expression))
                        throw new InvalidDataException($"规则单元格不能为空：{GetCellName(row + 1, column + 1)}。");
                    if (string.Equals(expression, "None", StringComparison.OrdinalIgnoreCase))
                        continue;

                    rules.Add(new StateTransitionRule
                    {
                        FromIndex = row,
                        ToIndex = column,
                        Condition = ParseCondition(expression, row + 1, column + 1),
                    });
                }
            }

            return new TransitionTableData(worksheetName, stateNames, rules.ToArray());
        }

        private static CharacterIntention ParseCondition(string expression, int row, int column)
        {
            CharacterIntention condition = CharacterIntention.None;
            var parsedNames = new HashSet<string>(StringComparer.Ordinal);
            string[] tokens = expression.Split('+');
            foreach (string rawToken in tokens)
            {
                string token = Normalize(rawToken);
                if (string.IsNullOrEmpty(token))
                    throw new InvalidDataException($"条件格式无效：{GetCellName(row, column)}。");

                if (string.Equals(token, nameof(CharacterIntention.None), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"None 不能与其他条件组合：{GetCellName(row, column)}。");
                if (!Enum.TryParse(token, false, out CharacterIntention intention) ||
                    !Enum.IsDefined(typeof(CharacterIntention), intention) || intention == CharacterIntention.None)
                    throw new InvalidDataException(
                        $"无法识别条件 {token}：{GetCellName(row, column)}。条件名称必须与 CharacterIntention 枚举名称一致。");
                if (!parsedNames.Add(token))
                    throw new InvalidDataException($"条件重复：{token}，位置 {GetCellName(row, column)}。");

                condition |= intention;
            }

            return condition;
        }

        private static string Normalize(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string GetCellName(int zeroBasedRow, int zeroBasedColumn)
        {
            int column = zeroBasedColumn + 1;
            string columnName = string.Empty;
            while (column > 0)
            {
                column--;
                columnName = (char)('A' + column % 26) + columnName;
                column /= 26;
            }

            return columnName + (zeroBasedRow + 1);
        }
    }
}
