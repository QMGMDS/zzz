using UnityEngine;
using UnityEngine.UI;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 视图 - 仅订阅视图模型投影字段刷新 UI，不接触领域类型，不持有数据状态。
    /// </summary>
    public class PlayerHUD_View : MonoBehaviour
    {
        [Header("角色头像")]
        [Tooltip("角色 1 头像 Image。")]
        [SerializeField] private Image _characterAvatar1;
        [Tooltip("角色 2 头像 Image。")]
        [SerializeField] private Image _characterAvatar2;
        [Tooltip("角色 3 头像 Image。")]
        [SerializeField] private Image _characterAvatar3;

        [Header("血条")]
        [Tooltip("角色 1 血条红底。")]
        [SerializeField] private Image _hpBarRedSlot1;
        [Tooltip("角色 1 血条绿条。")]
        [SerializeField] private Image _hpBarGreenSlot1;
        [Tooltip("角色 2 血条绿条。")]
        [SerializeField] private Image _hpBarGreenSlot2;
        [Tooltip("角色 3 血条绿条。")]
        [SerializeField] private Image _hpBarGreenSlot3;

        [Header("红底消退")]
        [Tooltip("红底血条向绿条消退的速度（fillAmount / 秒）。")]
        [SerializeField] private float _redBarDrainSpeed = 0.2f;

        private PlayerHUD_ViewModel _vm;
        private float _lastRedSetpoint0 = -1f;

        /// <summary>
        /// 绑定视图模型并订阅投影刷新。
        /// </summary>
        /// <param name="vm">PlayerHUD 视图模型</param>
        public void BindViewModel(PlayerHUD_ViewModel vm)
        {
            if (vm == null)
                throw new System.ArgumentNullException(nameof(vm));

            UnbindViewModel();
            _vm = vm;
            _vm.Updated += OnUpdated;
            ApplyAll();
        }

        /// <summary>
        /// 解绑当前视图模型。
        /// </summary>
        public void UnbindViewModel()
        {
            if (_vm != null)
            {
                _vm.Updated -= OnUpdated;
                _vm = null;
                _lastRedSetpoint0 = -1f;
            }
        }

        private void OnDestroy()
        {
            UnbindViewModel();
        }

        private void Update()
        {
            float green = _vm != null ? _vm.HpFill0 : 0f;
            float red = _hpBarRedSlot1.fillAmount;
            if (red > green)
                _hpBarRedSlot1.fillAmount = Mathf.Max(green, red - _redBarDrainSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 投影刷新回调 - 全量应用视图。
        /// </summary>
        private void OnUpdated()
        {
            ApplyAll();
        }

        /// <summary>
        /// 全量应用视图模型投影到 UI 控件。红底仅在目标值变化时重设，下降由 Update 插值追下来。
        /// </summary>
        private void ApplyAll()
        {
            _characterAvatar1.sprite = _vm.Avatar0;
            _characterAvatar2.sprite = _vm.Avatar1;
            _characterAvatar3.sprite = _vm.Avatar2;

            _hpBarGreenSlot1.fillAmount = _vm.HpFill0;
            _hpBarGreenSlot2.fillAmount = _vm.HpFill1;
            _hpBarGreenSlot3.fillAmount = _vm.HpFill2;

            float rs = _vm.RedSetpoint0;
            if (rs != _lastRedSetpoint0)
            {
                _hpBarRedSlot1.fillAmount = rs;
                _lastRedSetpoint0 = rs;
            }
        }
    }
}