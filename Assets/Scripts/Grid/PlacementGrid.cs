using Aokoro;
using Game.LevelManagement.Resources;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.LevelManagement.Grids
{
    
    public class PlacementGrid : Singleton<PlacementGrid>
    {
        [SerializeField, ReadOnly, BoxGroup("Tiles")] List<Tile> tiles;
        [SerializeField, BoxGroup("Tiles")] GameObject tilePrefab;
        [SerializeField, BoxGroup("Tiles")] Vector2 gridOffset;
        [BoxGroup("Tiles")] public float worldCellSize;
        [SerializeField, BoxGroup("Tiles")] LayerMask groundLayer;
        [SerializeField, BoxGroup("Tiles")] LayerMask tileLayer;
        [SerializeField, BoxGroup("Tiles")] Transform gridParent;


        [BoxGroup("Grid"), SerializeField] private Bounds worldBounds;
        [BoxGroup("Grid"), SerializeField] GridModifier[] gridModifiers;
        

        private void OnValidate()
        {
            //CreateTiles();
        }

        

        public bool TryGetTileAtScreenPoint(Vector2 screenPosition, out Tile tile)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 1000, tileLayer, QueryTriggerInteraction.Collide))
            {
                if (raycastHit.collider.TryGetComponent(out tile))
                    return true;
            }

            tile = null;
            return false;
        }

        [Button]
        private void CreateTiles()
        {
            CleanTiles();
            gridModifiers = GetComponentsInChildren<GridModifier>();

            Vector3 min = worldBounds.min;
            float xOffset = min.x % worldCellSize;
            float zOffset = min.z % worldCellSize;

            float minXWorldAlligned = min.x - xOffset;
            float minZWorldAlligned = min.z - zOffset;

            int CellCountX = Mathf.CeilToInt((worldBounds.size.x - Mathf.Abs(xOffset)) / worldCellSize) - 1;
            int CellCountZ = Mathf.CeilToInt((worldBounds.size.z - Mathf.Abs(zOffset)) / worldCellSize);

            for (int i = 0; i < CellCountX; i++)
            {
                for (int j = 1; j < CellCountZ; j++)
                {
                    float x = minXWorldAlligned + worldCellSize * i;
                    float z = minZWorldAlligned + worldCellSize * j;

                    Vector3 minY = new Vector3(x, worldBounds.min.y, z);
                    Vector3 maxY = new Vector3(x, worldBounds.max.y, z);

                    if (Physics.Linecast(maxY, minY, out RaycastHit hit, groundLayer))
                    {
                        Bounds cellBounds = new Bounds(hit.point, Vector3.one * worldCellSize);
                        if (CheckIfValid(cellBounds))
                        {
#if UNITY_EDITOR
                            var tileGO = UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab, gridParent) as GameObject;
#else
                            var tileGO = Instantiate(tilePrefab, gridParent);
#endif
                            Tile tile = tileGO.GetComponent<Tile>();
                            tile.Bind(this, hit.point, new Vector2Int(i,j));
                            tiles.Add(tile);
                        }
                    }
                }
            }
            //RaycastHit[] buffer = new RaycastHit[64];

        }

        public bool CheckIfValid(Bounds tileBounds)
        {

            Vector3 center = tileBounds.center;
            for (int k = 0; k < gridModifiers.Length; k++)
            {
                Bounds gridBounds = gridModifiers[k].Bounds;
                if (gridBounds.Intersects(tileBounds) || gridBounds.Contains(center))
                    return false;
            }
            NavMeshQueryFilter navMeshQueryFilter = new NavMeshQueryFilter()
            {
                areaMask = 1 << NavMesh.GetAreaFromName("Walkable"),
            };


            tileBounds.Expand(worldCellSize * .25f);
            if (NavMesh.FindClosestEdge(center, out NavMeshHit hit, navMeshQueryFilter))
            {
                if (tileBounds.Contains(hit.position))
                {
                    //Debug.DrawLine(center + Vector3.up, hit.position, Color.red, 2);
                    return false;
                }
                else
                    Debug.DrawLine(center + Vector3.up, hit.position, Color.green, 2);

            }
            else
                return false;

            return true;
        }

        public void RequestTileForResources(ResourceType resourceType)
        {
            foreach(var tile in tiles)
                tile.RequestTileForResource(resourceType);
        }
        public void ResetTileState()
        {
            foreach (var tile in tiles)
                tile.UnSelect();
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void CleanTiles()
        {
            foreach (var tile in tiles)
            {
                if(tile != null)
                {
                    if (Application.isPlaying)
                        Destroy(tile.gameObject);
                    else
                        DestroyImmediate(tile.gameObject);
                }
            }

            tiles.Clear();
        }

#if UNITY_EDITOR
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void RequestWater() => RequestTileForResources(ResourceType.Water);

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void RequestRock() => RequestTileForResources(ResourceType.Rock);
        
#endif
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }

        protected override void OnExistingInstanceFound(PlacementGrid existingInstance)
        {
            Destroy(gameObject);
        }
    }
}