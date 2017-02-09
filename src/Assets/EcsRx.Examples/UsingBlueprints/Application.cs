﻿using Assets.EcsRx.Examples.UsingBlueprints.Blueprints;
using EcsRx.Unity;

namespace Assets.EcsRx.Examples.UsingBlueprints
{
    public class Application : EcsRxApplication
    {
        protected override void ApplicationStarting()
        {
            RegisterAllBoundSystems();
        }

        protected override void ApplicationStarted()
        {
            var defaultPool = PoolManager.GetPool();

            defaultPool.CreateEntity(new PlayerBlueprint("Player One"));
            defaultPool.CreateEntity(new PlayerBlueprint("Player Two", 150.0f));
        }
    }
}