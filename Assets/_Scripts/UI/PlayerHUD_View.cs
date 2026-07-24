using SPCharacterController;
using UnityEngine;
using UnityEngine.UI;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 视图 - 监听数据模型事件，驱动 UI 刷新。绝不持有数据。
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

        [Header("能量节点")]
        [Tooltip("角色 1 能量节点 Image。")]
        [SerializeField] private Image _powerPoint1;
        [Tooltip("角色 2 能量节点 Image。")]
        [SerializeField] private Image _powerPoint2;
        [Tooltip("角色 3 能量节点 Image。")]
        [SerializeField] private Image _powerPoint3;

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

        private PlayerHUD_Model _model;
        private int _slot0DataIndex = -1;

        /// <summary>
        /// 绑定数据模型并订阅更新事件。
        /// </summary>
        /// <param name="model">PlayerHUD 数据模型</param>
        public void BindModel(PlayerHUD_Model model)
        {
            if (model == null)
                throw new System.ArgumentNullException(nameof(model));

            UnbindModel();
            _model = model;
            _model.DataUpdated += OnDataUpdated;
            RefreshAll();
        }

        /// <summary>
        /// 解绑当前数据模型。
        /// </summary>
        public void UnbindModel()
        {
            if (_model != null)
            {
                _model.DataUpdated -= OnDataUpdated;
                _model = null;
            }
        }

        private void OnDestroy()
        {
            UnbindModel();
        }

        private void Update()
        {
            float green = _hpBarGreenSlot1.fillAmount;
            float red = _hpBarRedSlot1.fillAmount;
            if (red > green)
            {
                _hpBarRedSlot1.fillAmount = Mathf.Max(green, red - _redBarDrainSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 数据更新回调 - 刷新全部 UI。
        /// </summary>
        private void OnDataUpdated()
        {
            RefreshAll();
        }

        /// <summary>
        /// 全量刷新 HUD 显示。激活角色始终在 UI 槽位 0，后续角色按队伍顺序排列。
        /// </summary>
        private void RefreshAll()
        {
            int activeIndex = _model.ActiveCharacterIndex;

            for (int slot = 0; slot < 3; slot++)
            {
                int dataIndex = (activeIndex + slot) % 3;
                RefreshAvatar(slot, dataIndex);
                RefreshHP(slot, dataIndex);
            }
        }

        /// <summary>
        /// 刷新指定 UI 槽位的头像显示。
        /// </summary>
        /// <param name="slot">UI 槽位 0-2</param>
        /// <param name="dataIndex">数据索引 0-2</param>
        private void RefreshAvatar(int slot, int dataIndex)
        {
            CharacterInfoSO info = _model.Characters[dataIndex];
            Image target = GetAvatarImage(slot);
            target.sprite = info.Avatar;
        }

        /// <summary>
        /// 刷新指定 UI 槽位的血条显示。
        /// </summary>
        /// <param name="slot">UI 槽位 0-2</param>
        /// <param name="dataIndex">数据索引 0-2</param>
        private void RefreshHP(int slot, int dataIndex)
        {
            CharacterInfoSO info = _model.Characters[dataIndex];
            float fill = (float)info.CurrentHP / info.MaxHP;
            Image target = GetHPBarGreenImage(slot);
            target.fillAmount = fill;

            if (slot == 0)
            {
                if (dataIndex != _slot0DataIndex || fill >= _hpBarRedSlot1.fillAmount)
                {
                    _hpBarRedSlot1.fillAmount = fill;
                    _slot0DataIndex = dataIndex;
                }
            }
        }

        /// <summary>
        /// 根据槽位索引获取对应的头像 Image。
        /// </summary>
        private Image GetAvatarImage(int index)
        {
            return index switch
            {
                0 => _characterAvatar1,
                1 => _characterAvatar2,
                2 => _characterAvatar3,
                _ => null
            };
        }

        /// <summary>
        /// 根据槽位索引获取对应的血条绿条 Image。
        /// </summary>
        private Image GetHPBarGreenImage(int index)
        {
            return index switch
            {
                0 => _hpBarGreenSlot1,
                1 => _hpBarGreenSlot2,
                2 => _hpBarGreenSlot3,
                _ => null
            };
        }
    }
}
