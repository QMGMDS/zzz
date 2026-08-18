using System;

using UnityEngine;

using SPCharacter.Contract;
using SPCharacter.Core;
using SPFramework.Service;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 代理意图接线胶水 - 实现代理会话契约 经胶水扩展窗口将 AI 等代理源的请求提交为角色意图
    /// 代理意图不受玩家操作锁约束
    /// </summary>
    internal sealed class AgentIntentionWiring : MonoBehaviour, ICCWiringExtension, ICharacterAgentSession
    {
        [Header("代理配置")]
        [SerializeField, Tooltip("角色唯一标识 供代理驱动源按 Id 获取代理会话")]
        private string _characterId;

        private Vector2 _moveAxis;
        private Vector2 _facingDirection;
        private bool _hasPendingAttack;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(_characterId))
                throw new InvalidOperationException($"{name}: 未设置角色唯一标识");
        }

        private void OnEnable()
        {
            InstanceServiceHub.Register<ICharacterAgentSession>(_characterId, this);
        }

        private void OnDisable()
        {
            InstanceServiceHub.Unregister<ICharacterAgentSession>(_characterId, this);
        }

        /// <inheritdoc />
        public void SetMoveAxis(Vector2 worldDirection)
        {
            _moveAxis = worldDirection;
        }

        /// <inheritdoc />
        public void SetFacingDirection(Vector2 worldDirection)
        {
            _facingDirection = worldDirection;
        }

        /// <inheritdoc />
        public void RequestAttack()
        {
            _hasPendingAttack = true;
        }

        /// <inheritdoc />
        public void UpdateWiring(CCWiringContext context, IWriteIntention writer)
        {
            bool hasMoveInput = _moveAxis.sqrMagnitude > 0f;

            // 移动优先 移动轴为零时朝向轴接管转向 避免两路输入互相覆盖
            writer.SetMoveAxis(hasMoveInput ? _moveAxis : _facingDirection);
            CommitIf(writer, CCIntention.WantToMove, hasMoveInput);

            if (_hasPendingAttack)
            {
                _hasPendingAttack = false;
                writer.SetIntention(CCIntention.WantToAttack, true);
            }

            // 缓存请求当帧消费后即清空 代理源停止调用时角色当帧停走
            _moveAxis = Vector2.zero;
            _facingDirection = Vector2.zero;
        }

        private static void CommitIf(IWriteIntention writer, CCIntention intention, bool value)
        {
            if (value)
                writer.SetIntention(intention, true);
        }
    }
}
