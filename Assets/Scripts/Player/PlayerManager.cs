using Game.LevelManagement.Grids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public class PlayerManager : MonoBehaviour
    {
        private Tile selectedTile;
        public void OnMouseMove(InputAction.CallbackContext ctx)
        {
            if(PlacementGrid.Instance.TryGetTileAtScreenPoint(ctx.ReadValue<Vector2>(), out var tile))
            {
                if(tile != selectedTile)
                {
                    if(selectedTile != null) selectedTile.UnSelect();

                    tile.Select();
                    selectedTile = tile;
                }
            }
            else if(selectedTile != null)
            {
                selectedTile.UnSelect();
                selectedTile = null;
            }
        }
    }
}