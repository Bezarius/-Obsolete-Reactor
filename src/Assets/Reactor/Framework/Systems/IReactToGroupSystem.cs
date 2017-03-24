using Reactor.Entities;
using Reactor.Groups;
using UniRx;

namespace Reactor.Systems
{
    public interface IReactToGroupSystem : ISystem
    {
        IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group);
        void Execute(IEntity entity);
    }
}