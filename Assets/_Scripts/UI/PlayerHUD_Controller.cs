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

        private void Awake()
        {
            if (_teamController == null)
                throw new System.InvalidOperationException($"{name}: TeamController 未设置。");
            if (_view == null)
                throw new System.InvalidOperationException($"{name}: PlayerHUD_View 未设置。");
        }

        private void Start()
        {
            _model = new PlayerHUD_Model(_teamController.RuntimeTeamInfo);
            _view.BindModel(_model);
        }

        private void OnEnable()
        {
            SPEvent.GameEvent.CharacterSwitched += OnCharacterSwitched;
        }

        private void OnDisable()
        {
            SPEvent.GameEvent.CharacterSwitched -= OnCharacterSwitched;
        }

        /// <summary>
        /// 响应角色切换事件，通知 Model 刷新。
        /// </summary>
        /// <param name="newIndex">新激活角色索引</param>
        private void OnCharacterSwitched(int newIndex)
        {
            _model.SetActiveCharacter(newIndex);
        }
    }
}
