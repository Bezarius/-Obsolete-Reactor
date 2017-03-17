using System.Collections.Generic;
using EcsRx.Entities;
using EcsRx.Pools;

namespace EcsRx.Systems.Executor.Handlers
{
    public interface IEntityToEntityReactionSystemHandler
    {
        IPoolManager PoolManager { get; }
        IEnumerable<SubscriptionToken> Setup(IEntityToEntityReactionSystem system);
        SubscriptionToken ProcessEntity(IEntityToEntityReactionSystem system, IEntity entity);
    }
}