using Reactor.Entities;
using UniRx;

namespace Reactor.Systems
{
    public interface IEntityToEntityReactionSystem : ISystem
    {
        IObservable<IEntity> Reaction(IEntity entity);

        void Execute(IEntity sourceEntity, IEntity targetEntity);
    }
}