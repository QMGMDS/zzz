using Core.Input;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家角色控制器——负责输入模块的装配与驱动。
    /// 在 Awake 中完成依赖注入，每帧驱动采集→翻译管线。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("输入阅读器配置")]
        [Tooltip("玩家输入阅读器，需挂载 PlayerInputReader 组件并拖拽引用")]
        [SerializeField] private PlayerInputReader _playerInputReader;

        [Header("Components")]
        [Tooltip("角色 CharacterController 组件，用于驱动移动与碰撞")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("角色 Animator 组件")]
        [SerializeField] private Animator _animator;

        #region 输入依赖

        private InputCollector _inputCollector;
        private MainProcessorPipeline _mainProcessorPipeline;

        #endregion

        #region 意图黑板

        private IntentionBlackboard _blackboard;
        /// <summary>
        /// 意图黑板——下游系统从此读取角色意图，无需接触原始输入数据。
        /// </summary>
        public IntentionBlackboard IntentionBlackboard => _blackboard;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            _blackboard = new IntentionBlackboard();
            _inputCollector = new InputCollector(_playerInputReader);
            _mainProcessorPipeline = new MainProcessorPipeline(_inputCollector, _blackboard);
        }

        private void Update()
        {
            // 原始数据采集+后处理
            _inputCollector.Update();

            // 后处理数据进行意图翻译
            _mainProcessorPipeline.UpdateIntentProcessors();
        }

        #endregion
    }
}
