using Core.Event;
using GamePlay.Attribute;
using GamePlay.Combat;
using CombatConfig = GamePlay.Combat.AttackComboConfigSO;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 状态上下文接口，提供状态执行所需的游戏世界依赖
    /// </summary>
    public interface IStateContext
    {
        /// <summary>角色 Animator 组件</summary>
        Animator Animator { get; }

        /// <summary>角色 CharacterController 组件</summary>
        CharacterController CharacterController { get; }

        /// <summary>角色 Transform</summary>
        Transform Transform { get; }

        /// <summary>当前帧输入方向（轮询模式）</summary>
        Vector2 MoveDirection { get; }

        /// <summary>主摄像机</summary>
        Camera MainCamera { get; }

        /// <summary>当前状态机引用，供状态内部触发切换</summary>
        StateMachineBase StateMachine { get; }

        /// <summary>输入缓冲时间（秒），在该时间内持续无输入才判定为停止</summary>
        float InputBufferTime { get; }

        /// <summary>前闪避硬直时间（秒），期间不可被其他状态打断</summary>
        float EvadeFrontCommitDuration { get; }

        /// <summary>后撤步硬直时间（秒），期间不可被其他状态打断</summary>
        float EvadeBackCommitDuration { get; }

        /// <summary>当前帧是否有闪避输入待消费</summary>
        bool IsEvadeTriggered { get; }

        /// <summary>消费闪避输入标记，防止重复触发</summary>
        void ConsumeEvade();

        /// <summary>当前帧是否有攻击输入待消费</summary>
        bool IsAttackTriggered { get; }

        /// <summary>消费攻击输入标记，防止重复触发</summary>
        void ConsumeAttack();

        /// <summary>连击窗口持续时间（秒），动画结束后在该时间内收到攻击输入则进入下一段</summary>
        float ComboWindowDuration { get; }

        /// <summary>当前锁定的敌人 Transform，未锁定时为 null</summary>
        Transform LockTarget { get; }

        /// <summary>攻击碰撞体组件，由 NormalAttackState 控制 Enable/Disable</summary>
        AttackHitbox AttackHitbox { get; }

        /// <summary>连击攻击配置 SO，按段索引取参数</summary>
        CombatConfig AttackConfig { get; }

        /// <summary>特效生成挂点 Transform，用于定位挥砍特效</summary>
        Transform EffectSpawnPoint { get; }

        /// <summary>震屏事件通道，NormalAttackState 通过此通道广播抖动力度</summary>
        FloatEventChannelSO CameraShakeChannel { get; }

        /// <summary>角色属性只读接口，供状态读取攻击力等属性</summary>
        IAttributeProvider Attributes { get; }
    }
}
