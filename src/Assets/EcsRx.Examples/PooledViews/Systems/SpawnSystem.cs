using System;
using Assets.EcsRx.Examples.PooledViews.Blueprints;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using UniRx;

namespace Assets.EcsRx.Examples.PooledViews.Systems
{
    public class SpawnSystem : IEntityReactionSystem
    {
        private readonly IPool _defaultPool;

        public IGroup TargetGroup { get; private set; }

        public SpawnSystem(IPoolManager poolManager)
        {
            TargetGroup = new Group(typeof(SpawnerComponent), typeof(ViewComponent));
            _defaultPool = poolManager.GetPool();
        }

        public IObservable<IEntity> EntityReaction(IEntity entity)
        {
            var spawnComponent = entity.GetComponent<SpawnerComponent>();
            return Observable.Interval(TimeSpan.FromSeconds(spawnComponent.SpawnRate)).Select(x => entity);
        }

        public void Execute(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var blueprint = new SelfDestructBlueprint(viewComponent.View.transform.position);
            _defaultPool.CreateEntity(blueprint); //todo: optimize 68.5%
        }
    }
}