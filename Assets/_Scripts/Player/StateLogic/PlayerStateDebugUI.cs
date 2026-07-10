using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 调试脚本——OnGUI 实时显示角色当前所处的状态族和状态节点。
    /// 附加到 PlayerController 所在的 GameObject 上即可。
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerStateDebugUI : MonoBehaviour
    {
        private PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void OnGUI()
        {
            if (_player == null) return;

            var machine = _player.GroupStateMachine;
            if (machine == null) return;

            var currentGroup = machine.CurrentGroup;
            var currentNode = machine.CurrentNode;

            var boxWidth = 320f;
            var boxHeight = 180f;
            var x = Screen.width - boxWidth - 20f;
            var y = 20f;

            GUI.Box(new Rect(x, y, boxWidth, boxHeight), "Player State Debug");

            var labelX = x + 12f;
            var lineH = 22f;
            var offsetY = y + 25f;

            var groupName = currentGroup != null ? currentGroup.name : "(null)";
            var nodeName = currentNode != null ? currentNode.name : "(null)";

            GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                $"<b>State Group:</b> {groupName}");
            offsetY += lineH;

            GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                $"<b>State Node:</b> {nodeName}");
            offsetY += lineH + 4f;

            var brain = _player.PlayerBrainBlackboard;
            if (brain != null)
            {
                GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                    $"<b>Normalized Time:</b> {brain.CurrentNormalizedTime:F3}");
                offsetY += lineH;

                GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                    $"<b>Anim Completed:</b> {brain.AnimationCompleted}");
                offsetY += lineH;

                GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                    $"<b>Want To Attack:</b> {brain.WantToAttack}   <b>Held:</b> {brain.AttackHeld}");
                offsetY += lineH;

                GUI.Label(new Rect(labelX, offsetY, boxWidth - 24f, lineH),
                    $"<b>Want To Evade:</b> {brain.WantToEvade}   <b>Want To Move:</b> {brain.WantToMove}");
            }
        }
    }
}
