using UnityEngine;

namespace SPPlayer
{
    /* [DefaultExecutionOrder(-300)] 特性作用说明 DA☆ZE
        让 PlayerController 在装配阶段和每帧 Update 逻辑阶段都尽量早于大多数下游系统执行，
        使下游系统在同一帧、且执行顺序更晚时，能读到本帧已更新的输入/意图/状态。
    */

    /// <summary>
    /// 玩家角色控制器——整个角色系统的 Root Monobehaviour 驱动源。
    /// 不包含任何具体游戏逻辑，仅负责子系统装配和严格的时序指令分发。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Animancer.AnimancerComponent))]
    [RequireComponent(typeof(InputSource))]
    [DefaultExecutionOrder(-300)]
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("CharacterController 组件")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("Animancer 组件")]
        [SerializeField] private Animancer.AnimancerComponent _animancer;

        #region 自定义配置

        [Header("玩家输入源")]
        [Tooltip("玩家输入源，采集员的工作区")]
        [SerializeField] private InputSource _playerInputSource;

        [Header("全局拦截器")]
        [Tooltip("按优先级从上到下排列的拦截器列表——谁排前面谁先抢")]
        [SerializeField] private StateInterceptorSO[] _globalInterceptors;

        [Header("动画配置")]
        [Tooltip("玩家动画配置 SO——定义状态到动画的映射")]
        [SerializeField] private PlayerAnimationConfigSO _animationConfig;

        #endregion

        #region 子系统(Public 供状态类访问)

        public PlayerBrain PlayerBrainBlackboard { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public MainInterceptor MainInterceptor { get; private set; }

        #endregion

        #region 私有依赖

        private InputCollector _inputCollector;
        private InputMainProcessor _inputMainProcessor;
        private AnimationDriver _animationDriver;
        private AnimationSource _animationSource;
        private StateToAnimationAdapter _adapter;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            // --- 分配 + 依赖注入---

            // 确保组件引用
            if (_characterController == null) _characterController = GetComponent<CharacterController>();
            if (_animancer == null) _animancer = GetComponent<Animancer.AnimancerComponent>();
            if (_playerInputSource == null) Debug.LogError($"{name} 的 {nameof(PlayerController)} 缺少 {nameof(InputSource)} 引用，输入系统将无法工作。");

            // 角色大脑黑板
            PlayerBrainBlackboard = new PlayerBrain();

            // 输入采集员
            _inputCollector = new InputCollector(_playerInputSource);

            // 主输入翻译处理器
            _inputMainProcessor = new InputMainProcessor(_inputCollector, PlayerBrainBlackboard);

            // 角色状态机
            StateMachine = new StateMachine();

            // 主拦截处理器
            MainInterceptor = new MainInterceptor(this, _globalInterceptors);

            // 动画源
            _animationSource = new AnimationSource(_animancer);

            // 状态→动画适配器
            _adapter = new StateToAnimationAdapter(_animationConfig);

            // 动画驱动器
            _animationDriver = new AnimationDriver(PlayerBrainBlackboard, _animationSource, _adapter);
        }

        private void Update()
        {
            // 1. 原始数据采集+后处理 -> 后处理数据
            _inputCollector.Update();

            // 2. 后处理数据 + 主输入翻译处理器 -> 输入意图 -> 写入角色大脑黑板
            _inputMainProcessor.UpdateInputProcessors();

            // 3. 当前状态的逻辑更新 (含全局拦截器检查)
            StateMachine.CurrentState.LogicUpdate();

            // 4. 动画驱动器更新动画
            _animationDriver?.Update();
        }

        private void OnAnimatorMove()
        {
            /* OnAnimatorMove 中应用角色移动原因说明 DA☆ZE
                驱动器调用动画指令切换动画只是下达命令
                实际 Animator 在 Update 之后 OnAnimatorMove 之前刷骨骼
                此时才拥有最新鲜的动画根骨骼 Transform 数据
            */
            // 角色移动更新
            StateMachine.CurrentState.PhysicsUpdate();
        }

        private void LateUpdate()
        {
            // 清除输入意图标记，为下一帧做准备
            PlayerBrainBlackboard.ResetInputBrain();
        }

        #endregion
    }
}
