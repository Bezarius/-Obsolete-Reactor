using System.Collections.Generic;
using EcsRx.Entities;
using EcsRx.Pools;

namespace EcsRx.Systems.Executor.Handlers
{
    public interface IEntityReactionSystemHandler
    {
        IPoolManager PoolManager { get; }
        IEnumerable<SubscriptionToken> Setup(IEntityReactionSystem system);
        SubscriptionToken ProcessEntity(IEntityReactionSystem system, IEntity entity);
    }
}