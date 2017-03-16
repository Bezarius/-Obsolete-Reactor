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

        public override bool Equals(object obj)
        {
            return obj != null && this.GetHashCode() == obj.GetHashCode();
        }

        private int _hash = 0;

        public override int GetHashCode()
        {
            if (_hash == 0)
            {
                unchecked
                {
                    foreach (var component in TargettedComponents)
                    {
                        var cHash = component.GetHashCode();
                        _hash = (_hash * 397) ^ cHash;
                    }
                }
            }
            return _hash;
        }
    }
}