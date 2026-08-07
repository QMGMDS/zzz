using UnityEngine;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 意图供给者资产抽象基类 - 便于 SPCC 在 Inspector 直接序列化引用。
    /// </summary>
    public abstract class CharacterIntentionProviderAsset : ScriptableObject, ICharacterIntentionProvider
    {
        /// <inheritdoc />
        public abstract CharacterIntentionFrame CurrentFrame { get; }
    }

    /// <summary>
    /// 角色意图供给接口 - 仅产出 Contract 级数据，不接触 Core 黑板。
    /// </summary>
    public interface ICharacterIntentionProvider
    {
        /// <summary>当前帧的意图快照。</summary>
        CharacterIntentionFrame CurrentFrame { get; }
    }
}