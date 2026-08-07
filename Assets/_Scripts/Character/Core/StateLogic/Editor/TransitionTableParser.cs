using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SPCharacter.Contract;

namespace SPCharacter.Core.Editor
{
    /// <summary>
    /// 状态转换表解析结果 - 保存有序状态名称与转换规则。
    /// </summary>
    internal sealed class TransitionTableData
    {
        internal TransitionTableData(
            string worksheetName,
            string[] stateNames,
            StateTransitionRule[] rules)
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
    /// 单元格格式：条件表达式@打断点，如 "WantToAttack@0.4"；省略 "@打断点" 时打断点为 0。
    /// </summary>
    internal static class TransitionTableParser
    {
        private const string CellSeparator = "@";

        internal static TransitionTableData Parse(string worksheetName, string[,] cells)
        {
            ValidateWorksheetNameAndCells(worksheetName, cells);
            string[] stateNames = ParseStateNames(cells);
            ValidateSourceStateNames(cells, stateNames);

            int stateCount = stateNames.Length;
            var rules = new List<StateTransitionRule>(stateCount * stateCount);
            for (int row = 0; row < stateCount; row++)
            {
                for (int column = 0; column < stateCount; column++)
                {
                    string expression = Normalize(cells[row + 1, column + 1]);
                    if (string.IsNullOrEmpty(expression))
                        throw new InvalidDataException($"规则单元格不能为空：{GetCellName(row + 1, column + 1)}。");
                    if (string.Equals(expression, "None", StringComparison.OrdinalIgnoreCase))
                        continue;

                    rules.Add(new StateTransitionRule(
                        stateNames[row],
                        stateNames[column],
                        ParseCondition(expression, row + 1, column + 1),
                        ParseInterruptPoint(expression, row + 1, column + 1)));
                }
            }

            return new TransitionTableData(worksheetName, stateNames, rules.ToArray());
        }

        private static void ValidateWorksheetNameAndCells(string worksheetName, string[,] cells)
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
        }

        private static string[] ParseStateNames(string[,] cells)
        {
            int stateCount = cells.GetLength(1) - 1;
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

            return stateNames;
        }

        private static void ValidateSourceStateNames(string[,] cells, IReadOnlyList<string> stateNames)
        {
            for (int row = 0; row < stateNames.Count; row++)
            {
                string fromState = Normalize(cells[row + 1, 0]);
                if (!string.Equals(fromState, stateNames[row], StringComparison.Ordinal))
                    throw new InvalidDataException(
                        $"来源状态 {GetCellName(row + 1, 0)} 必须与目标状态顺序一致，应为 {stateNames[row]}。");
            }
        }

        private static StateTransitionCondition ParseCondition(string expression, int row, int column)
        {
            string conditionExpression = expression;
            int separatorIndex = expression.IndexOf(CellSeparator, StringComparison.Ordinal);
            if (separatorIndex >= 0)
                conditionExpression = expression.Substring(0, separatorIndex);

            conditionExpression = Normalize(conditionExpression);
            if (string.IsNullOrEmpty(conditionExpression))
                throw new InvalidDataException($"条件格式无效：{GetCellName(row, column)}。");

            var condition = new StateTransitionCondition();
            var requiredNames = new HashSet<string>(StringComparer.Ordinal);
            var forbiddenNames = new HashSet<string>(StringComparer.Ordinal);
            string[] tokens = conditionExpression.Split('+');
            foreach (string rawToken in tokens)
            {
                string token = Normalize(rawToken);
                if (string.IsNullOrEmpty(token))
                    throw new InvalidDataException($"条件格式无效：{GetCellName(row, column)}。");

                bool negated = token[0] == '!';
                if (negated)
                    token = Normalize(token.Substring(1));

                if (string.IsNullOrEmpty(token))
                    throw new InvalidDataException($"条件格式无效：{GetCellName(row, column)}。");

                if (string.Equals(token, nameof(CharacterIntention.None), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"None 不能与其他条件组合：{GetCellName(row, column)}。");
                if (!Enum.TryParse(token, false, out CharacterIntention intention) ||
                    !Enum.IsDefined(typeof(CharacterIntention), intention) || intention == CharacterIntention.None)
                    throw new InvalidDataException(
                        $"无法识别条件 {token}：{GetCellName(row, column)}。条件名称必须与 CharacterIntention 枚举名称一致。");

                if (negated)
                {
                    if (requiredNames.Contains(token))
                        throw new InvalidDataException($"条件 {token} 不能既要求为 1 又要求为 0：{GetCellName(row, column)}。");
                    if (!forbiddenNames.Add(token))
                        throw new InvalidDataException($"条件重复：!{token}，位置 {GetCellName(row, column)}。");
                    condition.Forbidden = condition.Forbidden | intention;
                }
                else
                {
                    if (forbiddenNames.Contains(token))
                        throw new InvalidDataException($"条件 {token} 不能既要求为 1 又要求为 0：{GetCellName(row, column)}。");
                    if (!requiredNames.Add(token))
                        throw new InvalidDataException($"条件重复：{token}，位置 {GetCellName(row, column)}。");
                    condition.Required = condition.Required | intention;
                }
            }

            return condition;
        }

        private static float ParseInterruptPoint(string expression, int row, int column)
        {
            int separatorIndex = expression.IndexOf(CellSeparator, StringComparison.Ordinal);
            if (separatorIndex < 0)
                return 0f;

            string pointText = Normalize(expression.Substring(separatorIndex + CellSeparator.Length));
            if (string.IsNullOrEmpty(pointText))
                throw new InvalidDataException($"打断点不能为空：{GetCellName(row, column)}，格式为 条件@打断点。");

            if (!float.TryParse(pointText, NumberStyles.Float, CultureInfo.InvariantCulture, out float interruptPoint) ||
                float.IsNaN(interruptPoint) || interruptPoint < 0f || interruptPoint > 1f)
                throw new InvalidDataException(
                    $"打断点必须是 0 到 1 之间的数字：{GetCellName(row, column)}，当前值为 {pointText}。");

            return interruptPoint;
        }

        /// <summary>
        /// 将转移条件格式化为表格风格字符串：先 Required 后 Forbidden，以 + 连接。
        /// 纯 Required 输出 "WantToMove"；纯 Forbidden 输出 "!WantToMove"；
        /// 混合输出 "AnimationCompleted+!WantToEvade"；全 None 输出 "None"。
        /// </summary>
        internal static string FormatCondition(StateTransitionCondition condition)
        {
            if (condition.Required == CharacterIntention.None && condition.Forbidden == CharacterIntention.None)
                return nameof(CharacterIntention.None);

            var parts = new List<string>();
            AppendFlags(parts, condition.Required, false);
            AppendFlags(parts, condition.Forbidden, true);
            return string.Join("+", parts);
        }

        private static void AppendFlags(List<string> parts, CharacterIntention flags, bool negated)
        {
            if (flags == CharacterIntention.None)
                return;
            foreach (CharacterIntention value in Enum.GetValues(typeof(CharacterIntention)))
            {
                if (value == CharacterIntention.None)
                    continue;
                if ((flags & value) != value)
                    continue;
                parts.Add(negated ? "!" + value : value.ToString());
            }
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