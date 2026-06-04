using Core.Event;
using GamePlay.Attribute;
using GamePlay.Combat;
using CombatConfig = GamePlay.Combat.AttackComboConfigSO;
using GamePlay.Player;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 状态上下文接口，提供状态执行所需的核心组件引用与配置参数。
    /// 输入意图与缓冲已迁移至 PlayerBlackboard，旋转与位移已迁移至 MotionDriver。
    /// </summary>
    public interface IStateContext
    {
        /// <summary>角色 Animator 组件</summary>
        Animator Animator { get; }

        /// <summary>角色 CharacterController 组件</summary>
        CharacterController CharacterController { get; }

        /// <summary>角色 Transform</summary>
        Transform Transform { get; }

        /// <summary>主摄像机</summary>
        Camera MainCamera { get; }

        /// <summary>当前状态机引用，供状态内部触发切换</summary>
        StateMachineBase StateMachine { get; }

        /// <summary>前闪避硬直时间（秒），期间不可被其他状态打断</summary>
        float EvadeFrontCommitDuration { get; }

        /// <summary>后撤步硬直时间（秒），期间不可被其他状态打断</summary>
        float EvadeBackCommitDuration { get; }

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

        /// <summary>玩家意图黑板，提供输入标记与配置参数</summary>
        PlayerBlackboard Blackboard { get; }

        /// <summary>运动驱动器，管理角色旋转与 Root Motion 位移</summary>
        MotionDriver MotionDriver { get; }
    }
}
