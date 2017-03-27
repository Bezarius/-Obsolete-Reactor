using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Entities;
using Reactor.Events;
using Reactor.Extensions;
using Reactor.Groups;
using Reactor.Pools;
using Reactor.Systems.Executor.Handlers;
using UniRx;

namespace Reactor.Systems.Executor
{
    public sealed class SystemExecutor : ISystemExecutor, IDisposable
    {
        private readonly IList<ISystem> _systems;
        private readonly IList<IDisposable> _eventSubscriptions;
        private readonly Dictionary<ISystem, Dictionary<IEntity, SubscriptionToken>> _systemSubscriptions;
        private readonly List<SystemReactor> _systemReactors = new List<SystemReactor>();

        private SystemReactor _emptyReactor;
        private SystemReactor EmptyReactor
        {
            get { return _emptyReactor ?? (_emptyReactor = new SystemReactor(this, new HashSet<Type>())); }
        }

        public IEventSystem EventSystem { get; private set; }
        public IPoolManager PoolManager { get; private set; }
        public IEnumerable<ISystem> Systems { get { return _systems; } }

        public IEntityReactionSystemHandler EntityReactionSystemHandler { get; private set; }
        public IGroupReactionSystemHandler GroupReactionSystemHandler { get; private set; }
        public ISetupSystemHandler SetupSystemHandler { get; private set; }
        public IInteractReactionSystemHandler InteractReactionSystemHandler { get; private set; }
        public IManualSystemHandler ManualSystemHandler { get; private set; }

        public SystemExecutor(
            IPoolManager poolManager,
            IEventSystem eventSystem,
            IEntityReactionSystemHandler entityReactionSystemHandler,
            IGroupReactionSystemHandler groupReactionSystemHandler,
            ISetupSystemHandler setupSystemHandler,
            IInteractReactionSystemHandler interactReactionSystemHandler,
            IManualSystemHandler manualSystemHandler)
        {
            PoolManager = poolManager;
            EventSystem = eventSystem;
            EntityReactionSystemHandler = entityReactionSystemHandler;
            GroupReactionSystemHandler = groupReactionSystemHandler;
            SetupSystemHandler = setupSystemHandler;
            InteractReactionSystemHandler = interactReactionSystemHandler;
            ManualSystemHandler = manualSystemHandler;

            var addEntitySubscription = EventSystem.Receive<EntityAddedEvent>().Subscribe(OnEntityAddedToPool);
            var removeEntitySubscription = EventSystem.Receive<EntityRemovedEvent>().Subscribe(OnEntityRemovedFromPool);
            var addComponentSubscription = EventSystem.Receive<ComponentAddedEvent>().Subscribe(OnEntityComponentAdded);
            var removeComponentSubscription = EventSystem.Receive<ComponentRemovedEvent>().Subscribe(OnEntityComponentRemoved);

            _systems = new List<ISystem>();
            _systemSubscriptions = new Dictionary<ISystem, Dictionary<IEntity, SubscriptionToken>>();
            _eventSubscriptions = new List<IDisposable>
            {
                addEntitySubscription,
                removeEntitySubscription,
                addComponentSubscription,
                removeComponentSubscription
            };
        }


        public void OnEntityComponentAdded(ComponentAddedEvent args)
        {
            var entity = args.Entity;
            var type = args.Component.GetType();
            if (entity.Reactor != null)
            {
                args.Entity.Reactor.AddComponent(entity, type);
            }
            else
            {
                var reactor = this.GetSystemReactor(new HashSet<Type> {type});
                entity.Reactor = reactor;
                AddSystemsToEntity(entity, reactor);
            }
        }

        public void OnEntityComponentRemoved(ComponentRemovedEvent args)
        {
            args.Entity.Reactor.RemoveComponent(args.Entity, args.Component.GetType());
        }

        public void OnEntityAddedToPool(EntityAddedEvent args)
        {
        }

        public void OnEntityRemovedFromPool(EntityRemovedEvent args)
        {
        }


        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);

            if (system is IManualSystem)
            {
                ManualSystemHandler.Stop(system as IManualSystem);
            }

            if (_systemSubscriptions.ContainsKey(system))
            {
                _systemSubscriptions[system].Values.DisposeAll();
                _systemSubscriptions.Remove(system);
            }
        }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
            var subscriptionList = new List<SubscriptionToken>();

            if (system is ISetupSystem)
            {
                var subscriptions = SetupSystemHandler.Setup(system as ISetupSystem);
                subscriptionList.AddRange(subscriptions);
            }

            if (system is IGroupReactionSystem)
            {
                var subscription = GroupReactionSystemHandler.Setup(system as IGroupReactionSystem);
                subscriptionList.Add(subscription);
            }

            if (system is IEntityReactionSystem)
            {
                var subscriptions = EntityReactionSystemHandler.Setup(system as IEntityReactionSystem);
                subscriptionList.AddRange(subscriptions);
            }

            if (system is IInteractReactionSystem)
            {
                var subscriptions = InteractReactionSystemHandler.Setup(system as IInteractReactionSystem);
                subscriptionList.AddRange(subscriptions);
            }

            if (system is IManualSystem)
            {
                ManualSystemHandler.Start(system as IManualSystem);
            }

            _systemSubscriptions.Add(system, subscriptionList.ToDictionary(x=>x.AssociatedObject as IEntity));
        }

        public SystemReactor GetSystemReactor(HashSet<Type> targetTypes)
        {
            if (targetTypes.Count > 0)
            {
                //new HashSet<int>(first).SetEquals(second)
                SystemReactor reactor =
                    _systemReactors.FirstOrDefault(
                        x => x.TargetTypes.SetEquals(targetTypes));

                if (reactor == null)
                {
                    reactor = new SystemReactor(this, targetTypes);
                    _systemReactors.Add(reactor);
                }

                return reactor;
            }
            return EmptyReactor;
        }

        public void AddSystemsToEntity(IEntity entity, ISystemContainer container)
        {
            for (int i = 0; i < container.SetupSystems.Length; i++)
            {
                var system = container.SetupSystems[i];
                var subscription = SetupSystemHandler.ProcessEntity(system, entity);
                if (subscription != null)
                {
                    _systemSubscriptions[system].Add(entity, subscription);
                }
            }

            for (int i = 0; i < container.EntityReactionSystems.Length; i++)
            {
                var system = container.EntityReactionSystems[i];
                var subscription = EntityReactionSystemHandler.ProcessEntity(system, entity);
                if (subscription != null)
                {
                    _systemSubscriptions[system].Add(entity, subscription);
                }
            }

            for (int i = 0; i < container.InteractReactionSystems.Length; i++)
            {
                var system = container.InteractReactionSystems[i];
                var subscription = InteractReactionSystemHandler.ProcessEntity(system, entity);
                if (subscription != null)
                {
                    _systemSubscriptions[system].Add(entity,subscription);
                }
            }
        }

        private void RemoveEntitySubscriptionFromSystem(IEntity entity, ISystem system)
        {
            //todo: optimize. Method very slow 

            var subscriptionTokens = _systemSubscriptions[system][entity];
            subscriptionTokens.Disposable.Dispose();
            _systemSubscriptions[system].Remove(entity);
        }

        public void RemoveSystemsFromEntity(IEntity entity, ISystemContainer container)
        {
            for (int i = 0; i < container.TeardownSystems.Length; i++)
            {
                var system = container.TeardownSystems[i];

                system.Teardown(entity);

                RemoveEntitySubscriptionFromSystem(entity, system);
            }

            for (int i = 0; i < container.EntityReactionSystems.Length; i++)
            {
                RemoveEntitySubscriptionFromSystem(entity, container.EntityReactionSystems[i]);
            }

            for (int i = 0; i < container.InteractReactionSystems.Length; i++)
            {
                RemoveEntitySubscriptionFromSystem(entity, container.InteractReactionSystems[i]);
            }
        }

        public int GetSubscriptionCountForSystem(ISystem system)
        {
            if (!_systemSubscriptions.ContainsKey(system)) { return 0; }
            return _systemSubscriptions[system].Count;
        }

        public int GetTotalSubscriptions()
        {
            return _systemSubscriptions.Values.Sum(x => x.Count);
        }

        public void Dispose()
        {
            _systemSubscriptions.ForEachRun(x => x.Value.Values.DisposeAll());
            _eventSubscriptions.DisposeAll();
        }
    }
}