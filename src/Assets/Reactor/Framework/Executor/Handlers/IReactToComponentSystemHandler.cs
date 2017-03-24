using System.Collections.Generic;
using Reactor.Entities;
using Reactor.Pools;

namespace Reactor.Systems.Executor.Handlers
{
    public interface IEntityToEntityReactionSystemHandler
    {
        IPoolManager PoolManager { get; }
        IEnumerable<SubscriptionToken> Setup(IEntityToEntityReactionSystem system);
        SubscriptionToken ProcessEntity(IEntityToEntityReactionSystem system, IEntity entity);
    }
}