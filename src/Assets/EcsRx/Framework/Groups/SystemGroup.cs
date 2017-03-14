using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsRx.Groups
{
    public class SystemGroupKey : IGroup
    {
        public IEnumerable<Type> TargettedComponents { get; private set; }

        public SystemGroupKey(params Type[] targettedComponents)
        {
            TargettedComponents = targettedComponents;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return TargettedComponents.Aggregate(0, (current, t) => (current * 397) ^ t.GetHashCode());
            }
        }
    }
}