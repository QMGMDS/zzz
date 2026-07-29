using SPTeam;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 控制器 - 持有数据依赖，创建并注入 Model，对接输入与全局事件。
    /// </summary>
    public class PlayerHUD_Controller : MonoBehaviour
    {
        [Header("数据依赖")]
        [Tooltip("队伍控制器引用，通过其运行时副本获取队伍状态。")]
        [SerializeField] private TeamController _teamController;

        [Header("视图")]
        [Tooltip("PlayerHUD 视图。")]
        [SerializeField] private PlayerHUD_View _view;

        private PlayerHUD_Model _model;
        private bool _subscribed;

        private void Awake()
        {
            if (_teamController == null)
                throw new System.InvalidOperationException($"{name}: TeamController 未设置。");
            if (_view == null)
                throw new System.InvalidOperationException($"{name}: PlayerHUD_View 未设置。");
        }

        private void Start()
        {
            _model = new PlayerHUD_Model(_teamController);
            for (int i = 0; i < TeamInfoSO.CharacterCount; i++)
                _model.GetCharacterStats(i).HPChanged += OnCharacterStatsChanged;
            _subscribed = true;
            _view.BindModel(_model);
        }

        private void OnEnable()
        {
            SPEvent.GameEvent.CharacterSwitched += OnCharacterSwitched;
        }

        private void OnDisable()
        {
            SPEvent.GameEvent.CharacterSwitched -= OnCharacterSwitched;
            UnsubscribeStats();
        }

        private void OnDestroy()
        {
            UnsubscribeStats();
        }

        /// <summary>
        /// 响应角色切换事件，通知 Model 刷新。
        /// </summary>
        /// <param name="newIndex">新激活角色索引</param>
        private void OnCharacterSwitched(int newIndex)
        {
            _model.SetActiveCharacter(newIndex);
        }

        /// <summary>
        /// 任一角色生命值变化时，通知 Model 全量刷新 HUD。
        /// </summary>
        private void OnCharacterStatsChanged()
        {
            _model.NotifyDataChanged();
        }

        /// <summary>
        /// 解绑全部角色属性事件，防止 HUD 失活后回调空引用。
        /// </summary>
        private void UnsubscribeStats()
        {
            if (!_subscribed || _model == null) return;
            for (int i = 0; i < TeamInfoSO.CharacterCount; i++)
                _model.GetCharacterStats(i).HPChanged -= OnCharacterStatsChanged;
            _subscribed = false;
        }
    }
}
