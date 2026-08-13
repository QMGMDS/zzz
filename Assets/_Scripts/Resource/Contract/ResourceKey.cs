using System;

using UnityEngine;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源键 - 标识资源加载模块可解析的资源
    /// </summary>
    [Serializable]
    public struct ResourceKey : IEquatable<ResourceKey>
    {
        [SerializeField, Tooltip("资源定位键")]
        private string _value;

        /// <summary>资源定位键字符串</summary>
        public string Value => _value;

        /// <summary>是否包含可用资源定位键</summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        /// <summary>
        /// 创建资源键
        /// </summary>
        /// <param name="value">资源定位键字符串</param>
        public ResourceKey(string value)
        {
            _value = value;
        }

        /// <summary>
        /// 判断资源键是否相同
        /// </summary>
        /// <param name="other">另一个资源键</param>
        /// <returns>是否相同</returns>
        public bool Equals(ResourceKey other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        // 来自 Object
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ResourceKey other && Equals(other);
        }

        // 来自 Object
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_value ?? string.Empty);
        }

        // 来自 Object
        /// <inheritdoc />
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}
