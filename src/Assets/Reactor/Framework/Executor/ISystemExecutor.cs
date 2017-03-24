using System.Collections.Generic;
using Reactor.Pools;

namespace Reactor.Systems.Executor
{
    public interface ISystemExecutor
    {
        IPoolManager PoolManager { get; }
        IEnumerable<ISystem> Systems { get; }

        void RemoveSystem(ISystem system);
        void AddSystem(ISystem system);
    }
}