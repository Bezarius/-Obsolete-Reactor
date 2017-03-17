using System;
using EcsRx.Events;
using EcsRx.Pools;

namespace EcsRx.Entities
{
    public class DefaultEntityFactory : IEntityFactory
    {
        private readonly IEventSystem _eventSystem;

        public DefaultEntityFactory(IEventSystem eventSystem)
        {
            _eventSystem = eventSystem;
        }

        public IEntity Create(IPool pool, Guid? id = null)
        {
            if (!id.HasValue)
            { id = Guid.NewGuid(); }

            return new Entity(id.Value, pool, _eventSystem);
        }
    }
}