using System;
using EcsRx.Factories;
using EcsRx.Pools;
using Zenject;

namespace EcsRx.Entities
{
    public interface IEntityFactory : IFactory<IPool, Guid?, IEntity> {}
}