using EcsRx.Entities;
using UnityEngine;

namespace EcsRx.Unity.MonoBehaviours
{
    public class EntityView : MonoBehaviour
    {
        public IEntity Entity { get; set; }
    }
}