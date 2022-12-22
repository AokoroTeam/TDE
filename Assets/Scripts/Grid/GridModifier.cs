using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.LevelManagement.Grids
{
    public class GridModifier : MonoBehaviour
    {
        [SerializeField]
        private Bounds bounds;

        public Bounds Bounds => new Bounds(transform.TransformPoint(bounds.center), bounds.size);

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
    }
}
