using Reactor.Entities;
using UniRx;

namespace Reactor.Systems
{
    public interface IEntityReactionSystem : ISystem
    {
        IObservable<IEntity> EntityReaction(IEntity entity);

        void Execute(IEntity entity);
    }
}