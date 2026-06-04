using Core.Event;
using GamePlay.Attribute;

namespace GamePlay.Combat
{
    /// <summary>
    /// 攻击碰撞体调度服务，根据归一化时间和段配置启停 AttackHitbox，
    /// 处理多段 HitWindow 的开闭判定与震屏触发。
    /// </summary>
    public static class HitboxService
    {
        /// <summary>
        /// 每帧调用，根据归一化时间维护碰撞体启停状态
        /// </summary>
        /// <param name="normalizedTime">当前攻击动画归一化时间</param>
        /// <param name="comboIndex">当前连击段索引</param>
        /// <param name="config">连击配置 SO</param>
        /// <param name="hitbox">攻击碰撞体组件</param>
        /// <param name="attributes">角色属性接口，用于读取攻击力</param>
        /// <param name="shakeChannel">震屏事件通道，为 null 时跳过震屏</param>
        /// <param name="currentWindowIndex">当前判定窗口索引，由外部传入并维护</param>
        /// <param name="hitboxEnabled">碰撞体是否已启用的标记，由外部传入并维护</param>
        public static void Update(
            float normalizedTime,
            int comboIndex,
            AttackComboConfigSO config,
            AttackHitbox hitbox,
            IAttributeProvider attributes,
            FloatEventChannelSO shakeChannel,
            ref int currentWindowIndex,
            ref bool hitboxEnabled)
        {
            if (config == null || config.Segments == null || hitbox == null) return;
            if (comboIndex >= config.Segments.Length) return;

            AttackSegmentConfig seg = config.Segments[comboIndex];
            HitWindow[] windows = seg.HitWindows;
            if (windows == null || windows.Length == 0) return;

            while (currentWindowIndex < windows.Length
                   && normalizedTime >= windows[currentWindowIndex].StartNormalizedTime)
            {
                HitWindow w = windows[currentWindowIndex];
                float attackDamage = attributes.GetAttribute(AttributeType.Attack);
                hitbox.SetDamage(attackDamage);
                hitbox.Enable();
                hitboxEnabled = true;
                currentWindowIndex++;

                if (w.ShakeForce > 0f)
                    shakeChannel?.Raise(w.ShakeForce);
            }

            if (hitboxEnabled)
            {
                int checkIndex = currentWindowIndex - 1;
                if (checkIndex < 0 || normalizedTime >= windows[checkIndex].EndNormalizedTime)
                {
                    hitbox.Disable();
                    hitboxEnabled = false;
                }
            }
        }
    }
}
