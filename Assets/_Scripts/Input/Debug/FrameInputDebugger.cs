using UnityEngine;

using SPInput.Wiring;

namespace SPInput.Debug
{
    /// <summary>
    /// 帧输入调试器 - 每有操作按键被按下，通过 Debug.Log 集体打印当前帧完整原始输入数据
    /// </summary>
    [DefaultExecutionOrder(-300)]
    internal sealed class FrameInputDebugger : MonoBehaviour
    {
        [Header("调试器设置")]
        [Tooltip("是否启用输入调试日志打印")]
        [SerializeField] private bool _isDebugEnabled = true;

        [Tooltip("帧输入提供者槽位 SO，运行时从其 Provider 读取当前帧")]
        [SerializeField] private FrameInputProviderSO _inputProviderSO;

        private void Update()
        {
            if (!_isDebugEnabled) return;
            if (_inputProviderSO == null) return;

            var provider = _inputProviderSO.Provider;
            if (provider == null) return;

            var input = provider.CurrentFrame;

            bool hasAnyPressed = input.IsAttackPressed
                           || input.IsEvadePressed
                           || input.IsSkillPressed
                           || input.IsSwitchCharacterPressed
                           || input.IsUltimatePressed;

            if (!hasAnyPressed) return;

            UnityEngine.Debug.Log(
                $"<b>[输入调试]</b> 帧#{input.FrameIndex}  |  " +
                $"移动: ({input.MoveAxisValue.x:F2}, {input.MoveAxisValue.y:F2})  |  " +
                $"攻击:{Sym(input.IsAttackPressed)} " +
                $"闪避:{Sym(input.IsEvadePressed)} " +
                $"技能:{Sym(input.IsSkillPressed)} " +
                $"切换角色:{Sym(input.IsSwitchCharacterPressed)} " +
                $"终结技:{Sym(input.IsUltimatePressed)}"
            );
        }

        private static string Sym(bool value) => value ? "<color=#00FF00>按下</color>" : "<color=#888888>-</color>";
    }
}
