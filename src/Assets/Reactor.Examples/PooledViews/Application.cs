using Assets.Reactor.Examples.SceneFirstSetup.Components;
using Reactor.Unity;
using Reactor.Unity.Components;

namespace Assets.Reactor.Examples.PooledViews
{
    public class Application : ReactorApplication
    {
        protected override void ApplicationStarting()
        {
            RegisterAllBoundSystems();
        }

        protected override void ApplicationStarted()
        {}
    }
}
