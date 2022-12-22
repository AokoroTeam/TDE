using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.LevelManagement.Resources
{
    public enum ResourceType
    {
        Water,
        Rock,

    }

    public class ResourceZone : MonoBehaviour
    {
        public int productionCapacity;
        public ResourceType resourceType;
    }
}
