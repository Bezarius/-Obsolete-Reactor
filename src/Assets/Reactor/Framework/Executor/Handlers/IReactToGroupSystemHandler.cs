using Reactor.Pools;

namespace Reactor.Systems.Executor.Handlers
{
    public interface IReactToGroupSystemHandler
    {
        IPoolManager PoolManager { get; }
        SubscriptionToken Setup(IReactToGroupSystem system);
    }
}