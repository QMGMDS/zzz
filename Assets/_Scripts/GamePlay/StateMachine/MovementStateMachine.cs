using GamePlay.State;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 移动状态机，管理 Idle 与 Walk 状态之间的切换
    /// </summary>
    public class MovementStateMachine : StateMachine
    {
        public MovementStateMachine()
        {
            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
        }
    }
}
