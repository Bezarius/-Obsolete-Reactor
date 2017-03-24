using Reactor.Attributes;
using Reactor.Entities;
using Reactor.Events;
using Reactor.Groups;
using Reactor.Pools;
using Reactor.Systems;
using Reactor.Unity.Components;
using UniRx;
using UnityEngine;

namespace Reactor.Unity.Systems
{
    [Priority(999)]
    public abstract class PooledViewResolverSystem : ISetupSystem
    {
        public IPoolManager PoolManager { get; private set; }
        public IEventSystem EventSystem { get; private set; }

        protected GameObject PrefabTemplate { get; set; }

        public virtual IGroup TargetGroup
        {
            get { return new Group(typeof(ViewComponent)); }
        }

        protected PooledViewResolverSystem(IPoolManager poolManager, IEventSystem eventSystem)
        {
            PoolManager = poolManager;
            EventSystem = eventSystem;

            PrefabTemplate = ResolvePrefabTemplate();
        }

        protected abstract GameObject ResolvePrefabTemplate();
        protected abstract void RecycleView(GameObject viewToRecycle);
        protected abstract GameObject AllocateView(IEntity entity);

        public virtual void Setup(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            if (viewComponent.View != null) { return; }

            var viewObject = AllocateView(entity);
            viewComponent.View = viewObject;

            EventSystem.Receive<EntityRemovedEvent>()
                .First(x => x.Entity == entity)
                .Subscribe(x => RecycleView(viewObject));
        }
    }
}