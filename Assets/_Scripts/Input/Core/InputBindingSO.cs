using UnityEngine;
using UnityEngine.InputSystem;

namespace SPInput_Core
{
    /// <summary>
    /// 输入按键绑定 SO —— 持有各 InputActionReference。
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/Input Binding", fileName = "InputBinding")]
    public class InputBindingSO : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("移动输入（Vector2）")]
        public InputActionReference MoveAction;

        [Header("Actions")]
        [Tooltip("攻击输入")]
        public InputActionReference AttackAction;
        [Tooltip("闪避输入")]
        public InputActionReference EvadeAction;
        [Tooltip("技能输入")]
        public InputActionReference SkillAction;
        [Tooltip("切换角色输入")]
        public InputActionReference SwitchCharacterAction;
        [Tooltip("终结技输入")]
        public InputActionReference UltimateAction;
    }
}

