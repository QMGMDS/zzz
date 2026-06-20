using System.Collections.Generic;

namespace SPPlayer
{
    /// <summary>
    /// 状态机——持有所属角色引用与全部状态实例的缓存。
    /// 集中预创建所有状态，外部通过 GetState() 获取已有实例以复用。
    /// 不包含任何游戏逻辑，只负责状态的 Enter/Exit 生命周期调用。
    /// </summary>
    public class StateMachine
    {
        private readonly PlayerController _player;
        private Dictionary<PlayerStateType, BaseState> _states;

        /// <summary>当前激活状态</summary>
        public BaseState CurrentState { get; private set; }

        /// <summary>
        /// 创建状态机实例
        /// </summary>
        /// <param name="player">所属角色控制器引用，供状态构造函数使用</param>
        public StateMachine(PlayerController player)
        {
            _player = player;
            _states = new Dictionary<PlayerStateType, BaseState>
            {
                { PlayerStateType.Idle,new IdleState(_player) },
                { PlayerStateType.WalkStart, new WalkStartState(_player) },
                { PlayerStateType.WalkLoop, new WalkLoopState(_player) },
                { PlayerStateType.Stop, new StopState(_player) },
                { PlayerStateType.EvadeFront, new EvadeFrontState(_player) },
                { PlayerStateType.EvadeFrontEnd, new EvadeFrontEndState(_player) },
                { PlayerStateType.EvadeBack, new EvadeBackState(_player) },
                { PlayerStateType.EvadeBackEnd, new EvadeBackEndState(_player) },
                { PlayerStateType.RunStart, new RunStartState(_player) },
                { PlayerStateType.RunLoop, new RunLoopState(_player) },
                { PlayerStateType.RunTurn, new RunTurnState(_player) },
            };
        }

        /// <summary>
        /// 初始化状态机，设定最初状态
        /// </summary>
        /// <param name="startingState">起始状态</param>
        public void Initialize(PlayerStateType startingState)
        {
            CurrentState = GetState(startingState);
            CurrentState.Enter();
        }

        /// <summary>
        /// 按状态类型获取已缓存的实例。若类型未注册则返回 null。
        /// </summary>
        /// <param name="stateType">目标状态枚举</param>
        /// <returns>状态实例，未注册时返回 null</returns>
        public BaseState GetState(PlayerStateType stateType)
        {
            _states.TryGetValue(stateType, out var state);
            return state;
        }

        /// <summary>
        /// 切换状态——先 Exit 当前状态，再 Enter 新状态。
        /// </summary>
        /// <param name="newState">目标状态实例</param>
        public void ChangeState(BaseState newState)
        {
            if (CurrentState != null)
                CurrentState.Exit();

            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
