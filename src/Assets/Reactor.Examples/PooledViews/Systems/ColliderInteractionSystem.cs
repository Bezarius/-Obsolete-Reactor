using Reactor.Entities;
using Reactor.Groups;
using Reactor.Pools;
using Reactor.Systems;
using Reactor.Unity.Components;
using Reactor.Unity.MonoBehaviours;
using UniRx;
using UniRx.Triggers;

namespace Assets.Reactor.Examples.PooledViews.Systems
{
    public class ColliderInteractionSystem : IEntityToEntityReactionSystem
    {
        public IGroup TargetGroup { get; private set; }

        private readonly IPool _defaultPool;

        public ColliderInteractionSystem(IPoolManager poolManager)
        {
            _defaultPool = poolManager.GetPool();
            TargetGroup = new Group(typeof(SelfDestructComponent), typeof(ColliderComponent), typeof(ViewComponent));
        }

        public IObservable<IEntity> Reaction(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();

            return viewComponent.View
                .OnCollisionEnterAsObservable().Where(x => x.gameObject.GetComponent<EntityView>() != null)
                .Select(x => x.gameObject.GetComponent<EntityView>().Entity);
        }

        public void Execute(IEntity sourceEntity, IEntity targetEntity)
        {
            //var targetRigi = targetEntity.GetComponent<ColliderComponent>();
            //targetRigi.Rigidbody.AddRelativeForce(sourceEntity.GetComponent<ViewComponent>().View.transform.position);
        }
    }
}