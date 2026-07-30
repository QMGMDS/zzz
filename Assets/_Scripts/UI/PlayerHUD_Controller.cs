using SPTeam;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 控制器 - 装配视图与视图模型，管理订阅生命周期，不参与数据翻译。
    /// </summary>
    public class PlayerHUD_Controller : MonoBehaviour
    {
        [Header("数据依赖")]
        [Tooltip("队伍控制器引用，通过其运行时副本获取队伍状态。")]
        [SerializeField] private TeamController _teamController;

        [Header("视图")]
        [Tooltip("PlayerHUD 视图。")]
        [SerializeField] private PlayerHUD_View _view;

        private PlayerHUD_ViewModel _viewModel;

        private void Awake()
        {
            if (_teamController == null)
                throw new System.InvalidOperationException($"{name}: TeamController 未设置。");
            if (_view == null)
                throw new System.InvalidOperationException($"{name}: PlayerHUD_View 未设置。");

            _viewModel = new PlayerHUD_ViewModel(_teamController);
            _view.BindViewModel(_viewModel);
        }

        private void OnEnable()
        {
            _viewModel.Start();
        }

        private void OnDisable()
        {
            _viewModel.Stop();
        }

        private void OnDestroy()
        {
            _viewModel?.Stop();
            _view?.UnbindViewModel();
        }
    }
}