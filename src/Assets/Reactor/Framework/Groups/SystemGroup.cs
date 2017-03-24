using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Extensions;
using Reactor.Systems;

namespace Reactor.Groups
{
    public class SystemGroupKey : IGroup
    {
        public IEnumerable<Type> TargettedComponents { get; private set; }

        private readonly int _hash = 0;

        public SystemGroupKey(IEnumerable<Type> targettedComponents)
        {
            TargettedComponents = targettedComponents;

            unchecked
            {
                foreach (var component in TargettedComponents)
                {
                    var cHash = component.GetHashCode();
                    _hash = (_hash*397) ^ cHash;
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null && this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }

    public class SystemGroup
    {
        public ISetupSystem[] SetupSystems { get; private set; }

        public IEntityReactionSystem[] EntityReactionSystems { get; private set; }

        public IEntityToEntityReactionSystem[] EntityToEntityReactionSystems { get; private set; }

        public SystemGroup(List<ISystem> systems)
        {
            SetupSystems = systems.OfType<ISetupSystem>().OrderByPriority().ToArray();
            EntityReactionSystems = systems.OfType<IEntityReactionSystem>().OrderByPriority().ToArray();
            EntityToEntityReactionSystems =
                systems.OfType<IEntityToEntityReactionSystem>().OrderByPriority().ToArray();
        }
    }
}