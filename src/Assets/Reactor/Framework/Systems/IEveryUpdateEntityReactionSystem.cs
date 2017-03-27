using Reactor.Entities;

namespace Reactor.Systems
{
    public interface IEveryUpdateEntityReactionSystem : ISystem
    {
        void Reaction(IEntity entity);
    }
}