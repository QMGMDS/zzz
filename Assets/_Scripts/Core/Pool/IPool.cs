namespace Core.Pool
{
    /// <summary>
    /// 非泛型池接口，供 PoolManager 统一管理不同类型的池子
    /// </summary>
    internal interface IPool
    {
        int Count { get; }
        void Clear();
        IPoolable GetGeneric();
        void RecycleGeneric(IPoolable obj);
    }
}