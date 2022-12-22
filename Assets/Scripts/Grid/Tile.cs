using Game.LevelManagement.Resources;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Game.LevelManagement.Grids
{

    [Flags]
    public enum TileStatus
    {
        Selected = 1,
        Compatible = 2,
    }

    public class Tile : MonoBehaviour
    {
        private static Collider[] buffer = new Collider[16];

        [SerializeField, ReadOnly, BoxGroup("Global Infos")] 
        PlacementGrid placementGrid;
        [SerializeField]
        LayerMask resourceLayer;


        [SerializeField, ReadOnly, BoxGroup("Global Infos")]
        private ResourceZone[] resourceZones;


        [SerializeField, BoxGroup("Status")]
        TileStatus status = 0 >> 1 + 0 >> 2;
        [ShowNativeProperty]
        public bool IsSelected => status.HasFlag(TileStatus.Selected);
        [ShowNativeProperty]
        public bool IsCompatible => status.HasFlag(TileStatus.Selected);

        [SerializeField, BoxGroup("Materials")]
        Material defaultMat;
        [SerializeField, BoxGroup("Materials")]
        Material compatibleMat;
        [SerializeField, BoxGroup("Materials")]
        Material nonCompatibleMat;
        [SerializeField, BoxGroup("Materials")]
        private Vector2Int coordinates;

        [SerializeField, BoxGroup("Animations")]
        AnimationCurve startAnimationCurve;
        [SerializeField, BoxGroup("Animations")]
        private float squareSize;

        public Vector3 CellSize => placementGrid == null ? Vector3.one : Vector3.one * placementGrid.worldCellSize;

        private BoxCollider boxCollider;
        private MeshRenderer meshRenderer;
        private Animator animator;

        

        private void Awake()
        {

            boxCollider = GetComponent<BoxCollider>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            animator = GetComponentInChildren<Animator>();
            
            Vector3 endScale = meshRenderer.transform.localScale;
            meshRenderer.transform.localScale = Vector3.zero;

            int AMTHash = Animator.StringToHash("ApperanceMotionTime");
            animator.SetFloat(AMTHash, 0);

            DOTween.To(
                () => animator.GetFloat(AMTHash),
                ctx => animator.SetFloat(AMTHash, ctx),
                1f, 
                .75f)
                .SetDelay((coordinates.x + coordinates.y) * .02f)
                .SetTarget(this)
                .SetAutoKill()
                .Play();
        }

        public void Bind(PlacementGrid placementGrid, Vector3 pos, Vector2Int coord)
        {
            this.placementGrid = placementGrid;
            coordinates = coord;
            transform.position = pos;
            transform.localScale = CellSize;
            
            int resourcesCount = Physics.OverlapBoxNonAlloc(transform.position, CellSize * .5f, buffer, transform.rotation, resourceLayer, QueryTriggerInteraction.Collide);
            resourceZones = new ResourceZone[resourcesCount];
            
            for (int i = 0; i < resourcesCount; i++)
            {
                resourceZones[i] = buffer[i].GetComponent<ResourceZone>();
            }
        }

        public void RequestTileForResource(ResourceType resourceType)
        {
            for (int i = 0; i < resourceZones.Length; i++)
            {
                if (resourceZones[i].resourceType == resourceType)
                {
                    SetCompatible();
                    return;
                }
            }

            SetNonCompatible();
        }



        public void Select()
        {
            if (!IsSelected)
            {
                animator.SetBool("Selected", true);
                status |= TileStatus.Selected;
            }
        }
        public void UnSelect()
        {
            if (IsSelected)
            {
                animator.SetBool("Selected", false);
                status ^= TileStatus.Selected;
            }
        }

        public void SetCompatible()
        {
            meshRenderer.material = compatibleMat;
            status |= TileStatus.Compatible;
        }

        public void SetNonCompatible()
        {   
            meshRenderer.material = nonCompatibleMat;
            status ^= TileStatus.Compatible;
        }

        private void OnValidate()
        {
            if (placementGrid != null)
            {
                transform.localScale = CellSize;
            }
        }

    }
}
