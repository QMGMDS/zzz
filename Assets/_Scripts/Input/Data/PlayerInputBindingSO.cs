using UnityEngine;
using UnityEngine.InputSystem;

namespace SPPlayerInput
{
    /// <summary>
    /// 玩家输入绑定 SO
    /// </summary>
    [CreateAssetMenu(menuName = "SPPlayerInput/Player Input Binding", fileName = "PlayerInputBindingSO")]
    public class PlayerInputBindingSO : ScriptableObject
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
