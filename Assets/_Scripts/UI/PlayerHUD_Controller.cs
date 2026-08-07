using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 控制器 - 装配视图与 Team 重写期的临时空视图模型，管理订阅生命周期。
    /// </summary>
    public class PlayerHUD_Controller : MonoBehaviour
    {
        [Header("视图")]
        [Tooltip("PlayerHUD 视图。")]
        [SerializeField] private PlayerHUD_View _view;

        private PlayerHUD_ViewModel _viewModel;

        private void Awake()
        {
            if (_view == null)
                throw new System.InvalidOperationException($"{name}: PlayerHUD_View 未设置。");

            _viewModel = new PlayerHUD_ViewModel();
            _view.BindViewModel(_viewModel);
        }

        private void OnEnable()
        {
            _viewModel?.Start();
        }

        private void OnDisable()
        {
            _viewModel?.Stop();
        }

        private void OnDestroy()
        {
            _viewModel?.Stop();
            _view?.UnbindViewModel();
        }
    }
}
