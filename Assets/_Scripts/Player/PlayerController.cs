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

        [Header("Animator 组件")]
        [SerializeField] private Animator _animator;

        [Tooltip("Animancer 组件")]
        [SerializeField] private Animancer.AnimancerComponent _animancer;

        #region 自定义配置

        [Header("玩家输入源")]
        [Tooltip("玩家输入源，采集员的工作区")]
        [SerializeField] private InputSource _playerInputSource;

        [Header("初始状态族")]
        [Tooltip("角色初始所在的状态族")]
        [SerializeField] private StateGroupSO _initGroup;

        [Header("拦截器配置")]
        [Tooltip("节点拦截器配置 SO——定义跨族精确节点转移规则")]
        [SerializeField] private NodeInterceptorConfigSO _interceptorConfig;

        [Header("移动配置")]
        [Tooltip("玩家移动配置 SO——定义转向速度和重力参数")]
        [SerializeField] private PlayerMotorConfigSO _motorConfig;

        #endregion

        #region 子系统(Public 供拦截器和行为插件访问)

        /// <summary>角色大脑黑板</summary>
        public PlayerBrain PlayerBrainBlackboard { get; private set; }

        /// <summary>族长状态机——管理族间和族内状态转移</summary>
        public GroupStateMachine GroupStateMachine { get; private set; }

        #endregion

        #region 私有依赖

        private InputCollector _inputCollector;
        private InputMainProcessor _inputMainProcessor;
        private AnimationDriver _animationDriver;
        private AnimationSource _animationSource;
        private PlayerMotor _playerMotor;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            // --- 分配 + 依赖注入---

            // 确保组件引用
            if (_characterController == null) _characterController = GetComponent<CharacterController>();
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_animancer == null) _animancer = GetComponent<Animancer.AnimancerComponent>();
            if (_animancer.Animator == null) _animancer.Animator = _animator;

            if (_playerInputSource == null) Debug.LogError($"{name} 的 {nameof(PlayerController)} 缺少 {nameof(InputSource)} 引用，输入模块将无法工作。");
            if (_initGroup == null) Debug.LogError($"{name} 的 {nameof(PlayerController)} 缺少 {nameof(StateGroupSO)} 引用，初始状态族未设置。");
            if (_interceptorConfig == null) Debug.LogError($"{name} 的 {nameof(PlayerController)} 缺少 {nameof(NodeInterceptorConfigSO)} 引用，拦截器将无法工作。");
            if (_motorConfig == null) Debug.LogError($"{name} 的 {nameof(PlayerController)} 缺少 {nameof(PlayerMotorConfigSO)} 引用，移动层将无法工作。");

            // 实现代码接管根运动的前提
            if (_animator != null) _animator.applyRootMotion = true;

            // --- 数据中枢模块 ---

            // 角色大脑黑板
            PlayerBrainBlackboard = new PlayerBrain();

            // 写入玩家相机
            PlayerBrainBlackboard.CameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (PlayerBrainBlackboard.CameraTransform == null)
                Debug.LogError($"{name} 的 {nameof(PlayerController)}: 找不到 MainCamera，摄像机相对移动将失效。");

            // --- 玩家输入模块 ---

            // 输入采集员
            _inputCollector = new InputCollector(_playerInputSource);
            // 主输入翻译处理器
            _inputMainProcessor = new InputMainProcessor(_inputCollector, PlayerBrainBlackboard);

            // --- 角色状态模块 ---

            // 族长状态机
            GroupStateMachine = new GroupStateMachine(this, _interceptorConfig?.Interceptors);
            // 进入初始状态族
            GroupStateMachine.EnterGroup(_initGroup, _initGroup != null ? _initGroup.DefaultEntryIndex : 0);

            // 动画源
            _animationSource = new AnimationSource(_animancer);
            // 动画驱动器
            _animationDriver = new AnimationDriver(PlayerBrainBlackboard, _animationSource);

            // 玩家移动器
            _playerMotor = new PlayerMotor(_characterController, _animator, PlayerBrainBlackboard, _motorConfig);
        }

        private void Update()
        {
            // 1. 原始数据采集+后处理 -> 后处理数据
            _inputCollector.Update();

            // 2. 后处理数据 + 主输入翻译处理器 -> 输入意图 -> 写入角色大脑黑板
            _inputMainProcessor.UpdateInputProcessors();

            // 3. 族长状态机逻辑更新（含拦截器检查 + 族内转移）
            GroupStateMachine.LogicUpdate();

            // 4. 动画驱动器更新动画，仅发出指令
            _animationDriver.Update();

            // 5. 旋转更新，使 Animator 计算根运动时基于新朝向
            _playerMotor?.ApplyRotation();
        }

        // **Animator 此时更新动画，并更新该逻辑帧的最新动画进度**

        private void OnAnimatorMove()
        {
            /* OnAnimatorMove 中应用角色移动原因说明 DA☆ZE
                驱动器调用动画指令切换动画只是下达命令
                实际 Animator 在 Update 之后 OnAnimatorMove 之前刷骨骼
                此时才拥有最新鲜的动画根骨骼 Transform 数据
            */
            // 位移更新
            _playerMotor?.ApplyPosition();
        }

        private void LateUpdate()
        {
            // 动画驱动器同步最新动画进度到黑板
            _animationDriver.SyncAnimProgress();

            // 清除输入意图标记，为下一帧做准备
            PlayerBrainBlackboard.ResetInputBrain();
        }

        #endregion
    }
}
