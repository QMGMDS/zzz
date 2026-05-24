using System.Collections.Generic;
using GamePlay.State;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 调试可视化组件，在屏幕左上角显示 C# 状态机与 Animator 的当前状态，
    /// 方便对比两者是否同步。
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDebugDisplay : MonoBehaviour
    {
        [Tooltip("显示状态信息（C# 状态、Animator 状态、过渡目标）")]
        [SerializeField] private bool _showStateInfo = true;

        [Tooltip("显示输入信息（移动方向、闪避标记）")]
        [SerializeField] private bool _showInputInfo = true;

        private IStateContext _context;
        private Animator _animator;

        private static readonly Dictionary<int, string> HashToName = new()
        {
            { Animator.StringToHash("Idle"),              "Idle" },
            { Animator.StringToHash("WalkStart"),         "WalkStart" },
            { Animator.StringToHash("Walk"),              "Walk" },
            { Animator.StringToHash("RunStart"),          "RunStart" },
            { Animator.StringToHash("Run"),               "Run" },
            { Animator.StringToHash("RunEnd"),            "RunEnd" },
            { Animator.StringToHash("EvadeFront"),        "EvadeFront" },
            { Animator.StringToHash("EvadeBack"),         "EvadeBack" },
            { Animator.StringToHash("NormalAttack_1"),    "NormalAttack_1" },
            { Animator.StringToHash("NormalAttack_2"),    "NormalAttack_2" },
            { Animator.StringToHash("NormalAttack_3"),    "NormalAttack_3" },
            { Animator.StringToHash("NormalAttack_4"),    "NormalAttack_4" },
            { Animator.StringToHash("NormalAttack_1_End"),"NormalAttack_1_End" },
            { Animator.StringToHash("NormalAttack_2_End"),"NormalAttack_2_End" },
            { Animator.StringToHash("NormalAttack_3_End"),"NormalAttack_3_End" },
            { Animator.StringToHash("NormalAttack_4_End"),"NormalAttack_4_End" },
        };

        private void Awake()
        {
            _context = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
        }

        private void OnGUI()
        {
            if (!_showStateInfo && !_showInputInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label("[ State Debug ]", CreateBoldStyle());

            if (_showStateInfo)
            {
                string csharp = _context?.StateMachine?.CurrentStateType?.Name ?? "N/A";
                GUILayout.Label($"C#  : {csharp}", CreateColoredStyle(Color.cyan));

                if (_animator != null)
                {
                    AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                    string animName = HashToName.TryGetValue(stateInfo.shortNameHash, out string n) ? n : "?";
                    GUILayout.Label($"Anim: {animName}", CreateColoredStyle(Color.green));

                    if (_animator.IsInTransition(0))
                    {
                        AnimatorStateInfo nextInfo = _animator.GetNextAnimatorStateInfo(0);
                        string nextName = HashToName.TryGetValue(nextInfo.shortNameHash, out string nn) ? nn : "?";
                        GUILayout.Label($"  -> {nextName}", CreateColoredStyle(Color.yellow));
                    }

                    GUILayout.Label($"Norm: {stateInfo.normalizedTime:F2}");
                }
            }

            if (_showInputInfo)
            {
                GUILayout.Space(6);
                GUILayout.Label("--- Input ---");
                if (_context != null)
                {
                    Vector2 dir = _context.MoveDirection;
                    GUILayout.Label($"Move : ({dir.x:F2}, {dir.y:F2})");
                    GUILayout.Label($"Evade: {_context.IsEvadeTriggered}");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static GUIStyle CreateBoldStyle()
        {
            return new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        }

        private static GUIStyle CreateColoredStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            return style;
        }
    }
}
