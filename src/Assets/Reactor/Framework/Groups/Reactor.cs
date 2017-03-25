﻿using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Entities;
using Reactor.Extensions;
using Reactor.Systems;
using Reactor.Systems.Executor;

namespace Reactor.Groups
{

    public interface ISystemContainer
    {
        ISetupSystem[] SetupSystems { get; }
        IEntityReactionSystem[] EntityReactionSystems { get; }
        IEntityToEntityReactionSystem[] EntityToEntityReactionSystems { get; }
        ITeardownSystem[] TeardownSystems { get; }
    }


    public class ReactorConnection : ISystemContainer
    {
        public SystemReactor UpReactor { get; private set; }
        public SystemReactor DownReactor { get; private set; }

        public ISetupSystem[] SetupSystems { get; private set; }
        public IEntityReactionSystem[] EntityReactionSystems { get; private set; }
        public IEntityToEntityReactionSystem[] EntityToEntityReactionSystems { get; private set; }
        public ITeardownSystem[] TeardownSystems { get; private set; }

        public ReactorConnection(SystemReactor upReactor, SystemReactor downReactor)
        {
            UpReactor = upReactor;
            DownReactor = downReactor;
            SetupSystems = upReactor.SetupSystems.Except(downReactor.SetupSystems).ToArray();
            EntityReactionSystems = upReactor.EntityReactionSystems.Except(downReactor.EntityReactionSystems).ToArray();
            EntityToEntityReactionSystems = upReactor.EntityToEntityReactionSystems.Except(downReactor.EntityToEntityReactionSystems).ToArray();
            TeardownSystems = upReactor.TeardownSystems.Except(downReactor.TeardownSystems).ToArray();
        }
    }

    public class SystemReactor : ISystemContainer
    {
        private readonly ISystemExecutor _systemExecutor;
        public readonly Type[] TargetTypes;

        public ISetupSystem[] SetupSystems { get; private set; }
        public IEntityReactionSystem[] EntityReactionSystems { get; private set; }
        public IEntityToEntityReactionSystem[] EntityToEntityReactionSystems { get; private set; }
        public ITeardownSystem[] TeardownSystems { get; private set; }

        private readonly Dictionary<Type, ReactorConnection> _connections = new Dictionary<Type, ReactorConnection>();

        public SystemReactor(ISystemExecutor systemExecutor, Type[] targetTypes)
        {
            _systemExecutor = systemExecutor;
            TargetTypes = targetTypes;

            var systems = _systemExecutor.Systems.Where(x => x.TargetGroup.TargettedComponents.All(targetTypes.Contains)).ToList();

            SetupSystems = systems.OfType<ISetupSystem>().OrderByPriority().ToArray();
            EntityReactionSystems = systems.OfType<IEntityReactionSystem>().OrderByPriority().ToArray();
            EntityToEntityReactionSystems = systems.OfType<IEntityToEntityReactionSystem>().OrderByPriority().ToArray();
            TeardownSystems = systems.OfType<ITeardownSystem>().OrderByPriority().ToArray();
        }

        public void AddReactorsConnection(Type componenType, ReactorConnection connection, SystemReactor reactor)
        {
            if (!_connections.ContainsKey(componenType))
            {
                _connections.Add(componenType, connection);
                reactor.AddReactorsConnection(componenType, connection, this);
            }
        }

        public void AddComponent(IEntity entity, Type componenType)
        {
            ReactorConnection connection;
            if (!_connections.TryGetValue(componenType, out connection))
            {
                var types = new Type[TargetTypes.Length + 1];
                Array.Copy(TargetTypes, types, TargetTypes.Length);
                types[TargetTypes.Length] = componenType;

                SystemReactor reactor = _systemExecutor.GetSystemReactor(types);
                connection = new ReactorConnection(reactor, this);
                this.AddReactorsConnection(componenType, connection, reactor);
            }
            entity.Reactor = connection.UpReactor;
            _systemExecutor.AddSystemsToEntity(entity, connection);
        }

        public void RemoveComponent(IEntity entity, Type componenType)
        {
            ReactorConnection connection;
            if (!_connections.TryGetValue(componenType, out connection))
            {
                var types = new Type[TargetTypes.Length + 1];
                Array.Copy(TargetTypes, types, TargetTypes.Length);
                types[TargetTypes.Length] = componenType;

                SystemReactor reactor = _systemExecutor.GetSystemReactor(types);
                connection = new ReactorConnection(this, reactor);
                this.AddReactorsConnection(componenType, connection, reactor);
            }
            entity.Reactor = connection.DownReactor;
            _systemExecutor.RemoveSystemsFromEntity(entity, connection);
        }
    }
}