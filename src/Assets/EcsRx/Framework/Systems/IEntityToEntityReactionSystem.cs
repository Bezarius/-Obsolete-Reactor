using EcsRx.Entities;
using UniRx;

namespace EcsRx.Systems
{
    public interface IEntityToEntityReactionSystem : ISystem
    {
        IObservable<IEntity> Reaction(IEntity entity);

        void Execute(IEntity sourceEntity, IEntity targetEntity);
    }
}