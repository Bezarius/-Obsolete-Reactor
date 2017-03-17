using EcsRx.Entities;
using UniRx;

namespace EcsRx.Systems
{
    public interface IReactToDataSystem<T> : ISystem
    {
        IObservable<T> ReactToData(IEntity entity);

        void Execute(IEntity entity, T reactionData);
    }
}