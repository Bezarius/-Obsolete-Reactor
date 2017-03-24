using System.Collections.Generic;
using Reactor.Entities;
using Reactor.Extensions;
using Reactor.Groups;
using Reactor.Pools;
using UniRx;

namespace Reactor.Systems.Executor.Handlers
{
    public class ReactToEntitySystemHandler : IEntityReactionSystemHandler
    {
        public IPoolManager PoolManager { get; private set; }

        public ReactToEntitySystemHandler(IPoolManager poolManager)
        {
            PoolManager = poolManager;
        }

        public IEnumerable<SubscriptionToken> Setup(IEntityReactionSystem system)
        {
            var accessor = PoolManager.CreateGroupAccessor(system.TargetGroup);
            return accessor.Entities.ForEachRun(x => ProcessEntity(system, x));
        }

        public SubscriptionToken ProcessEntity(IEntityReactionSystem system, IEntity entity)
        {
            var hasEntityPredicate = system.TargetGroup is IHasPredicate;
            var subscription = system.EntityReaction(entity)
                .Subscribe(x =>
                {
                    if (hasEntityPredicate)
                    {
                        var groupPredicate = system.TargetGroup as IHasPredicate;
                        if (groupPredicate.CanProcessEntity(x))
                        {
                            system.Execute(x);
                        }
                        return;
                    }

                    system.Execute(x);
                });

            return new SubscriptionToken(entity, subscription);
        }
    }
}