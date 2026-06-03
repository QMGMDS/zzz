using System.Collections.Generic;

namespace GamePlay.Attribute
{
    /// <summary>
    /// 角色运行时属性容器，从 CharacterAttributeSO 加载基础值，提供只读查询。
    /// 未来需要修饰器叠加时，可在此处重新引入 AttributeModifier 机制。
    /// </summary>
    public class CharacterAttributes : IAttributeProvider
    {
        private readonly Dictionary<AttributeType, float> _attributeValues = new();

        /// <summary>从 CharacterAttributeSO 加载初始基础值</summary>
        /// <param name="config">初始属性配置</param>
        public CharacterAttributes(CharacterAttributeSO config)
        {
            _attributeValues[AttributeType.MaxHealth] = config.MaxHealth;
            _attributeValues[AttributeType.Attack] = config.Attack;
            _attributeValues[AttributeType.Defense] = config.Defense;
            _attributeValues[AttributeType.MoveSpeed] = config.MoveSpeed;
        }

        /// <inheritdoc/>
        public float GetAttribute(AttributeType type)
        {
            return _attributeValues.TryGetValue(type, out float value) ? value : 0f;
        }
    }
}
