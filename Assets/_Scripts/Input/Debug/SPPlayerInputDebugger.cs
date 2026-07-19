using UnityEngine;

namespace SPPlayerInput
{
    /// <summary>
    /// 玩家输入调试器 - 每有操作按键被按下，通过 Debug.Log 集体打印当前帧完整原始输入数据，
    /// 避免 OnGUI 实时刷新导致的覆盖问题。
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public class SPPlayerInputDebugger : MonoBehaviour
    {
        [Header("调试器设置")]
        [Tooltip("是否启用输入调试日志打印。")]
        [SerializeField] private bool _debugEnabled = true;

        private void Update()
        {
            if (!_debugEnabled) return;

            var center = SPPlayerInputCenter.Instance;
            if (center == null) return;

            var input = center.CurrentFrameInput;

            bool anyPressed = input.AttackPressed
                           || input.EvadePressed
                           || input.SkillPressed
                           || input.SwitchCharacterPressed
                           || input.UltimatePressed;

            if (!anyPressed) return;

            Debug.Log(
                $"<b>[输入调试]</b> 帧#{input.FrameIndex}  |  " +
                $"移动: ({input.MoveAxisValue.x:F2}, {input.MoveAxisValue.y:F2})  |  " +
                $"攻击:{Sym(input.AttackPressed)} " +
                $"闪避:{Sym(input.EvadePressed)} " +
                $"技能:{Sym(input.SkillPressed)} " +
                $"切换角色:{Sym(input.SwitchCharacterPressed)} " +
                $"终结技:{Sym(input.UltimatePressed)}"
            );
        }

        private static string Sym(bool value) => value ? "<color=#00FF00>按下</color>" : "<color=#888888>-</color>";
    }
}
