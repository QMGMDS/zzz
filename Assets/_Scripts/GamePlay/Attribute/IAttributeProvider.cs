namespace GamePlay.Attribute
{
    /// <summary>属性只读接口，供战斗系统、状态机、UI 等外部模块读取角色最终属性</summary>
    public interface IAttributeProvider
    {
        /// <summary>获取指定属性的最终值（经过所有修饰器计算后的结果）</summary>
        /// <param name="type">属性类型</param>
        /// <returns>最终属性值</returns>
        float GetAttribute(AttributeType type);
    }
}
