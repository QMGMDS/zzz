using UnityEngine;
using UnityEngine.InputSystem;

namespace SPInput_Core
{
    /// <summary>
    /// 输入按键绑定 SO - 持有各 InputActionReference。
    /// 字段为运行时只读配置，经只读属性对外暴露。
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/Input Binding", fileName = "InputBinding")]
    public class InputBindingSO : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("移动输入（Vector2）")]
        [SerializeField] private InputActionReference _moveAction;

        [Header("Actions")]
        [Tooltip("攻击输入")]
        [SerializeField] private InputActionReference _attackAction;

        [Tooltip("闪避输入")]
        [SerializeField] private InputActionReference _evadeAction;

        [Tooltip("技能输入")]
        [SerializeField] private InputActionReference _skillAction;

        [Tooltip("切换角色输入")]
        [SerializeField] private InputActionReference _switchCharacterAction;

        [Tooltip("终结技输入")]
        [SerializeField] private InputActionReference _ultimateAction;

        /// <summary>移动输入引用</summary>
        public InputActionReference MoveAction => _moveAction;

        /// <summary>攻击输入引用</summary>
        public InputActionReference AttackAction => _attackAction;

        /// <summary>闪避输入引用</summary>
        public InputActionReference EvadeAction => _evadeAction;

        /// <summary>技能输入引用</summary>
        public InputActionReference SkillAction => _skillAction;

        /// <summary>切换角色输入引用</summary>
        public InputActionReference SwitchCharacterAction => _switchCharacterAction;

        /// <summary>终结技输入引用</summary>
        public InputActionReference UltimateAction => _ultimateAction;
    }
}