﻿using Assets.Reactor.Examples.SimpleMovement.Components;
using Reactor.Entities;
using Reactor.Groups;
using Reactor.Systems;
using Reactor.Unity.Components;
using UniRx;
using UnityEngine;

namespace Assets.Reactor.Examples.SimpleMovement.Systems
{
    public class CameraFollowSystem : ISetupSystem, IReactToGroupSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new GroupBuilder()
                    .WithComponent<CameraFollowsComponent>()
                    .WithComponent<ViewComponent>()
                    .Build();
            }
        }

        public void Setup(IEntity entity)
        {
            var cameraFollows = entity.GetComponent<CameraFollowsComponent>();
            cameraFollows.Camera = Camera.main;
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor @group)
        {
            return Observable.EveryUpdate().Select(x => @group);
        }

        public void Execute(IEntity entity)
        {
            var entityPosition = entity.GetComponent<ViewComponent>().View.transform.position;
            var trailPosition = entityPosition + (Vector3.back*5.0f);
            trailPosition += Vector3.up*2.0f;

            var camera = entity.GetComponent<CameraFollowsComponent>().Camera;
            camera.transform.position = trailPosition;
            camera.transform.LookAt(entityPosition);
        }
    }
}