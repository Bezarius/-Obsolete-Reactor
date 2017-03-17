using System;
using System.Collections.Generic;
using System.Linq;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Systems.Executor.Handlers;
using UniRx;

namespace EcsRx.Systems.Executor
{
    public sealed class SystemExecutor : ISystemExecutor, IDisposable
    {
        private readonly IList<ISystem> _systems;
        private readonly IList<IDisposable> _eventSubscriptions;
        private readonly Dictionary<ISystem, IList<SubscriptionToken>> _systemSubscriptions;

        private readonly Dictionary<SystemGroupKey, List<ISystem>> _systemGroups = new Dictionary<SystemGroupKey, List<ISystem>>();

        public IEventSystem EventSystem { get; private set; }
        public IPoolManager PoolManager { get; private set; }
        public IEnumerable<ISystem> Systems { get { return _systems; } }

        public IEntityReactionSystemHandler ReactToEntitySystemHandler { get; private set; }
        public IReactToGroupSystemHandler ReactToGroupSystemHandler { get; private set; }
        public ISetupSystemHandler SetupSystemHandler { get; private set; }
        public IReactToDataSystemHandler ReactToDataSystemHandler { get; private set; }
        public IEntityToEntityReactionSystemHandler ReactToComponentSystemHandler { get; private set; }
        public IManualSystemHandler ManualSystemHandler { get; private set; }

        public SystemExecutor(
            IPoolManager poolManager,
            IEventSystem eventSystem,
            IEntityReactionSystemHandler reactToEntitySystemHandler,
            IReactToGroupSystemHandler reactToGroupSystemHandler,
            ISetupSystemHandler setupSystemHandler,
            IReactToDataSystemHandler reactToDataSystemHandler,
            IEntityToEntityReactionSystemHandler reactToComponentSystemHandler,
            IManualSystemHandler manualSystemHandler)
        {
            PoolManager = poolManager;
            EventSystem = eventSystem;
            ReactToEntitySystemHandler = reactToEntitySystemHandler;
            ReactToGroupSystemHandler = reactToGroupSystemHandler;
            SetupSystemHandler = setupSystemHandler;
            ReactToDataSystemHandler = reactToDataSystemHandler;
            ReactToComponentSystemHandler = reactToComponentSystemHandler;
            ManualSystemHandler = manualSystemHandler;

            var addEntitySubscription = EventSystem.Receive<EntityAddedEvent>().Subscribe(OnEntityAddedToPool);
            var removeEntitySubscription = EventSystem.Receive<EntityRemovedEvent>().Subscribe(OnEntityRemovedFromPool);
            var addComponentSubscription = EventSystem.Receive<ComponentAddedEvent>().Subscribe(OnEntityComponentAdded);
            var addComponentsSubscription =
                EventSystem.Receive<ComponentsAddedEvent>().Subscribe(OnEntityComponentsAdded);
            var removeComponentSubscription = EventSystem.Receive<ComponentRemovedEvent>().Subscribe(OnEntityComponentRemoved);
            var removeComponentsSubscription = EventSystem.Receive<ComponentsRemovedEvent>().Subscribe(OnEntityComponentsRemoved);

            _systems = new List<ISystem>();
            _systemSubscriptions = new Dictionary<ISystem, IList<SubscriptionToken>>();
            _eventSubscriptions = new List<IDisposable>
            {
                addEntitySubscription,
                removeEntitySubscription,
                addComponentSubscription,
                removeComponentSubscription,
                addComponentsSubscription,
                removeComponentsSubscription
            };
        }

        private void RegisterSystemGroup()
        {

        }


        private void ProcessAddComponentsToEntity(IEntity entity, IEnumerable<IComponent> components)
        {
            // todo: add checks
            List<ISystem> systems;

            var entityTypes = entity.Components.Select(x => x.GetType());
            var entityGroupKey = new SystemGroupKey(entityTypes);

            if (!_systemGroups.ContainsKey(entityGroupKey))
            {
                // add new system group
                var applicableSystems = _systems.GetApplicableSystems(entity).ToList();

                _systemGroups.Add(entityGroupKey, applicableSystems);

                if (applicableSystems.Count == 0)
                    return;

                if (entityTypes.Count() == components.Count())
                {
                    systems = applicableSystems;
                }
                else
                {
                    var effectedSystems = new List<ISystem>();
                    foreach (var component in components)
                    {
                        effectedSystems.AddRange(
                            applicableSystems.Where(
                                x => x.TargetGroup.TargettedComponents.Contains(component.GetType())));
                    }
                    systems = effectedSystems;
                }
            }
            else
            {
                // work with existed system group
                var prevEntityGroupKey = new SystemGroupKey(entity.Components.Except(components)
                    .Select(x => x.GetType()));
                if (_systemGroups.ContainsKey(prevEntityGroupKey))
                {
                    var prevSystems = _systemGroups[prevEntityGroupKey];
                    var currentSystems = _systemGroups[entityGroupKey];
                    systems = currentSystems.Except(prevSystems).ToList();
                }
                else
                {
                    systems = _systemGroups[entityGroupKey];
                }
            }

            ApplyEntityToSystems(systems, entity); // todo: optimize
        }

        private void ProcessRemoveComponentsFromEntity(IEntity entity, IEnumerable<IComponent> components)
        {
            IEnumerable<ISystem> effectedSystems;
            var entityTypes = entity.Components.Select(x => x.GetType());
            var prevComponentsTypes = entity.Components.Union(components).Select(x => x.GetType());
            var prevEntityGroupKey = new SystemGroupKey(prevComponentsTypes);
            
            if (entityTypes.Any())
            {
                var entityGroupKey = new SystemGroupKey(entityTypes);
                effectedSystems = _systemGroups[entityGroupKey].Except(_systemGroups[prevEntityGroupKey]);
            }
            else
            {
                effectedSystems = _systemGroups[prevEntityGroupKey];
            }

            foreach (var effectedSystem in effectedSystems)
            {
                if (effectedSystem is ITeardownSystem)
                {
                    (effectedSystem as ITeardownSystem).Teardown(entity);
                }

                // todo: optimize. mb to dict?
                var subscriptionToken = _systemSubscriptions[effectedSystem]
                    .FirstOrDefault(x => x.AssociatedObject == entity);

                if (subscriptionToken != null)
                {
                    _systemSubscriptions[effectedSystem].Remove(subscriptionToken);
                    subscriptionToken.Disposable.Dispose();
                }
                //_systemSubscriptions[effectedSystem].RemoveAll(subscriptionTokens);
                //subscriptionTokens.DisposeAll();
            }
        }

        public void OnEntityComponentAdded(ComponentAddedEvent args)
        {
            ProcessAddComponentsToEntity(args.Entity, new[] { args.Component });
        }

        public void OnEntityComponentsAdded(ComponentsAddedEvent args)
        {
            ProcessAddComponentsToEntity(args.Entity, args.Components);
        }

        public void OnEntityComponentRemoved(ComponentRemovedEvent args)
        {
            ProcessRemoveComponentsFromEntity(args.Entity, new[] { args.Component });
        }

        public void OnEntityComponentsRemoved(ComponentsRemovedEvent args)
        {
            ProcessRemoveComponentsFromEntity(args.Entity, args.Components);
        }

        public void OnEntityAddedToPool(EntityAddedEvent args)
        {
            //if (!args.Entity.Components.Any()) { return; }

            //ProcessAddComponentsToEntity(args.Entity, args.Entity.Components);
        }

        public void OnEntityRemovedFromPool(EntityRemovedEvent args)
        {
            //ProcessRemoveComponentsFromEntity(args.Entity, args.Entity.Components);
        }

        private void ApplyEntityToSystems(List<ISystem> systems, IEntity entity)
        {
            systems.OfType<ISetupSystem>()
                .OrderByPriority() //todo: optimize
                .ForEachRun(x =>
                {
                    var possibleSubscription = SetupSystemHandler.ProcessEntity(x, entity);
                    if (possibleSubscription != null)
                    {
                        _systemSubscriptions[x].Add(possibleSubscription);
                    }
                });

            systems.OfType<IEntityReactionSystem>()
                //.OrderByPriority()
                .ForEachRun(x =>
                {
                    var subscription = ReactToEntitySystemHandler.ProcessEntity(x, entity); // todo: optimize - 69%
                    _systemSubscriptions[x].Add(subscription);
                });

            systems.OfType<IEntityToEntityReactionSystem>().ForEachRun(x =>
            {
                var subscription = ReactToComponentSystemHandler.ProcessEntity(x, entity);
                _systemSubscriptions[x].Add(subscription);
            });

            systems.Where(x => x.IsReactiveDataSystem())
                //.OrderByPriority()
                .ForEachRun(x =>
                {
                    var subscription = ReactToDataSystemHandler.ProcessEntityWithoutType(x, entity);
                    _systemSubscriptions[x].Add(subscription);
                });
        }

        public void RemoveSubscription(ISystem system, IEntity entity)
        {
            var subscriptionList = _systemSubscriptions[system];
            var subscriptionTokens = subscriptionList.GetTokensFor(entity).ToArray();

            if (!subscriptionTokens.Any()) { return; }

            subscriptionTokens.ForEachRun(x => subscriptionList.Remove(x));
            subscriptionTokens.DisposeAll();
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
                _systemSubscriptions[system].DisposeAll();
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

            if (system is IReactToGroupSystem)
            {
                var subscription = ReactToGroupSystemHandler.Setup(system as IReactToGroupSystem);
                subscriptionList.Add(subscription);
            }

            if (system is IEntityReactionSystem)
            {
                var subscriptions = ReactToEntitySystemHandler.Setup(system as IEntityReactionSystem);
                subscriptionList.AddRange(subscriptions);
            }

            if (system is IEntityToEntityReactionSystem)
            {
                var subscriptions = ReactToComponentSystemHandler.Setup(system as IEntityToEntityReactionSystem);
                subscriptionList.AddRange(subscriptions);
            }

            if (system.IsReactiveDataSystem())
            {
                var subscriptions = ReactToDataSystemHandler.SetupWithoutType(system);
                subscriptionList.AddRange(subscriptions);
            }

            if (system is IManualSystem)
            {
                ManualSystemHandler.Start(system as IManualSystem);
            }

            _systemSubscriptions.Add(system, subscriptionList);
        }

        public int GetSubscriptionCountForSystem(ISystem system)
        {
            if (!_systemSubscriptions.ContainsKey(system)) { return 0; }
            return _systemSubscriptions[system].Count;
        }

        public int GetTotalSubscriptions()
        { return _systemSubscriptions.Values.Sum(x => x.Count); }

        public void Dispose()
        {
            _systemSubscriptions.ForEachRun(x => x.Value.DisposeAll());
            _eventSubscriptions.DisposeAll();
        }
    }
}