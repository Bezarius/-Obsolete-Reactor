using System;
using Reactor.Entities;
using Reactor.Unity.Components;
using Reactor.Unity.MonoBehaviours;
using UnityEngine;

namespace Assets.Reactor.Unity.Extensions
{
    public static class GameObjectExtensions
    {
        public static void LinkEntity(this GameObject gameObject, IEntity entity)
        {
            if(gameObject.GetComponent<EntityView>())
            { throw new Exception("GameObject already has an EntityView monobehaviour applied"); }

            if (gameObject.GetComponent<RegisterAsEntity>())
            { throw new Exception("GameObject already has a RegisterAsEntity monobehaviour applied"); }

            if (!entity.HasComponents(typeof(ViewComponent)))
            { entity.AddComponent<ViewComponent>(); }

            var entityViewMb = gameObject.AddComponent<EntityView>();
            entityViewMb.Entity = entity;
            
            var viewComponent = entity.GetComponent<ViewComponent>();
            viewComponent.View = gameObject;
        }
    }
}