using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 移动输入意图翻译器
    /// </summary>
    public class MoveInputProcessor : IInputProcessor
    {
        /// <summary>
        /// 将处理后的输入翻译为移动意图，写入黑板
        /// </summary>
        /// <param name="current">当前帧处理后的输入数据</param>
        /// <param name="last">上一帧处理后的输入数据</param>
        /// <param name="blackboard">玩家大脑黑板</param>
        public void UpdateIntentionTranslation(in ProcessedInputData current, in ProcessedInputData last, PlayerBrain blackboard)
        {
            blackboard.WantToMove = current.Move != Vector2.zero;
            blackboard.MoveInput = current.Move;

            blackboard.LastMoveDirection = ToCameraRelativeDirection(last.Move, blackboard.CameraTransform);
            blackboard.CurrentMoveDirection = ToCameraRelativeDirection(current.Move, blackboard.CameraTransform);

            if (current.Move.sqrMagnitude > 0.0001f && last.Move.sqrMagnitude > 0.0001f)
                blackboard.IsMoveDirectionFlipped = Vector2.Dot(current.Move.normalized, last.Move.normalized) <= -0.75f;
            else
                blackboard.IsMoveDirectionFlipped = false;
        }

        private static Vector3 ToCameraRelativeDirection(Vector2 moveInput, Transform cameraTransform)
        {
            if (cameraTransform == null)
                return Vector3.zero;

            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var direction = forward * moveInput.y + right * moveInput.x;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
        }
    }
}
