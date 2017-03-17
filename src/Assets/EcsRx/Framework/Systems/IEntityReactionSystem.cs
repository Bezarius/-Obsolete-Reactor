using EcsRx.Entities;
using UniRx;

namespace EcsRx.Systems
{
    public interface IEntityReactionSystem : ISystem
    {
        IObservable<IEntity> EntityReaction(IEntity entity);

        void Execute(IEntity entity);
    }
}