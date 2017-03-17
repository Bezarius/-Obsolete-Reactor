using System;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using UniRx;

namespace Assets.EcsRx.Examples.PooledViews.Systems
{
    public class SelfDestructionSystem : IEntityReactionSystem
    {
        public IGroup TargetGroup { get; private set; }

        private readonly IPool _defaultPool;

        public SelfDestructionSystem(IPoolManager poolManager)
        {
            TargetGroup = new Group(typeof(SelfDestructComponent), typeof(ViewComponent));
            _defaultPool = poolManager.GetPool();
        }

        public IObservable<IEntity> EntityReaction(IEntity entity)
        {
            var selfDestructComponent = entity.GetComponent<SelfDestructComponent>();
            return Observable.Interval(TimeSpan.FromSeconds(selfDestructComponent.Lifetime)).Select(x => entity);
        }

        public void Execute(IEntity entity)
        {
            _defaultPool.RemoveEntity(entity);
        }
    }
}