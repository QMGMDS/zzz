using System;

namespace SPCharacterController
{
    /// <summary>
    /// 角色运行时属性 - 由 CharacterStatsSO 生成的可变副本，集中管控当前生命值变更并广播事件，供战斗与 UI 系统订阅。
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        /// <summary>最大生命值，构造时确定不再变化。</summary>
        public int MaxHP { get; private set; }

        /// <summary>当前生命值，仅由本类方法修改以保证事件必发。</summary>
        public int CurrentHP { get; private set; }

        /// <summary>攻击力，构造时确定不再变化。</summary>
        public int Attack { get; private set; }

        /// <summary>当前生命值变化后广播的事件。</summary>
        public event Action HPChanged;

        /// <summary>
        /// 由静态数据资产构造运行时副本，当前生命值初始化为基础生命值。
        /// </summary>
        /// <param name="source">角色属性静态数据资产</param>
        public CharacterStats(CharacterStatsSO source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            MaxHP = source.MaxHP;
            CurrentHP = source.MaxHP;
            Attack = source.Attack;
        }

        /// <summary>
        /// 扣减生命值，下限钳制到 0。
        /// </summary>
        /// <param name="amount">伤害量，必须为正</param>
        public void TakeDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentException("伤害量必须为正数。", nameof(amount));
            CurrentHP = Math.Max(0, CurrentHP - amount);
            HPChanged?.Invoke();
        }

        /// <summary>
        /// 恢复生命值，上限钳制到最大生命值。
        /// </summary>
        /// <param name="amount">治疗量，必须为正</param>
        public void Heal(int amount)
        {
            if (amount <= 0) throw new ArgumentException("治疗量必须为正数。", nameof(amount));
            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
            HPChanged?.Invoke();
        }
    }
}