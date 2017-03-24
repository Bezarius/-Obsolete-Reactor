﻿using System;
using Reactor.Entities;
using Reactor.Groups;
using Reactor.Systems;
using Reactor.Unity.Components;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Reactor.Examples.ManuallyRegisterSystems.Systems
{
    public class RandomMovementSystem : IReactToGroupSystem
    {
        public IGroup TargetGroup { get { return new Group(typeof (ViewComponent)); } }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor @group)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1)).Select(x => @group);
        }

        public void Execute(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var positionChange = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            viewComponent.View.transform.position += positionChange;
        }
    }
}