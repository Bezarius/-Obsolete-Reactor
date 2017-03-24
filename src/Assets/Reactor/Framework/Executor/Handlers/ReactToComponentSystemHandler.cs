using System.Collections.Generic;
using Reactor.Entities;
using Reactor.Extensions;
using Reactor.Groups;
using Reactor.Pools;
using UniRx;

namespace Reactor.Systems.Executor.Handlers
{
    public class EntityToEntityReactionSystemHandler : IEntityToEntityReactionSystemHandler
    {
        public IPoolManager PoolManager { get; private set; }

        public EntityToEntityReactionSystemHandler(IPoolManager poolManager)
        {
            PoolManager = poolManager;
        }

        public IEnumerable<SubscriptionToken> Setup(IEntityToEntityReactionSystem system)
        {
            var groupAccessor = PoolManager.CreateGroupAccessor(system.TargetGroup);
            return groupAccessor.Entities.ForEachRun(x => ProcessEntity(system, x));
        }

        public SubscriptionToken ProcessEntity(IEntityToEntityReactionSystem system, IEntity entity)
        {
            var hasEntityPredicate = system.TargetGroup is IHasPredicate;
            var subscription = system.Reaction(entity)
                .Subscribe(x =>
                {
                    if (hasEntityPredicate)
                    {
                        var groupPredicate = system.TargetGroup as IHasPredicate;
                        if (groupPredicate.CanProcessEntity(entity))
                        {
                            system.Execute(entity, x);
                        }
                        return;
                    }

                    system.Execute(entity, x);
                });

            return new SubscriptionToken(entity, subscription);
        }
    }
}