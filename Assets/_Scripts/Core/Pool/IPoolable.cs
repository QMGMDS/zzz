namespace Core.Pool
{
    /// <summary>
    /// 池化接口，继承该接口的类，其类对象可被池化
    /// </summary>
    public interface IPoolable
    {
        /// <summary> 该对象所属池子 </summary>
        string PoolName { get; set; }
        /// <summary> 从池取出时调用 </summary>
        void OnSpawn();
        /// <summary> 回收时调用 </summary>
        void OnDespawn();
    }
}