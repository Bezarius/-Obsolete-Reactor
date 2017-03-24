using Reactor.Entities;
using Reactor.Groups;

namespace Reactor.Systems
{
    public interface ISystem
    {
        IGroup TargetGroup { get; }
    }
}