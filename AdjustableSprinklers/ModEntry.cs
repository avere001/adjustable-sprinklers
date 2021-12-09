using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
// using ConfigurableSprinklers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace AdjustableSprinklers
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private StardewValley.Object SelectedSprinkler;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SprinklerPatches.Initialize(Monitor, Helper);
            SprinklerData.Initialize(Helper);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.Rendered += this.OnRendered;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object),
                    nameof(StardewValley.Object.GetSprinklerTiles)),
                postfix: new HarmonyMethod(typeof(SprinklerPatches), nameof(SprinklerPatches.GetSprinklerTiles_Postfix))
            );
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
            if (!e.Button.IsActionButton())
                return;

            var targetTile = Game1.options.gamepadControls ? e.Cursor.GrabTile : e.Cursor.Tile;
            Game1.currentLocation.objects.TryGetValue(targetTile, out var selectedObject);
            if (selectedObject is not null && selectedObject.IsSprinkler())
            {
                SelectedSprinkler = SelectedSprinkler is null ? selectedObject : null;
            }
            else
            {
                if (SelectedSprinkler is null)
                    // Not currently configuring a sprinkler
                    return;

                var data = SprinklerData.ReadSprinklerData(SelectedSprinkler, true);

                if (data.SprinklerTiles.Contains(targetTile))
                {
                    data.SprinklerTiles.Remove(targetTile);
                    data.UnusedTileCount += 1;
                    var tileGraph = data.SprinklerTiles.DeepClone().AddItem(SelectedSprinkler.TileLocation);
                    var disconnected = GraphUtils.GetDisconnected(tileGraph, SelectedSprinkler.TileLocation);
                    foreach (var tile in disconnected)
                    {
                        data.SprinklerTiles.Remove(tile);
                        data.UnusedTileCount += 1;
                    }
                }
                else
                {
                    if (GetValidNewTiles().Contains(targetTile))
                    {
                        data.SprinklerTiles.Add(targetTile);
                        data.UnusedTileCount -= 1;
                    }
                }

                SprinklerData.WriteSprinklerData(SelectedSprinkler, data);
            }
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (SelectedSprinkler is null)
                return;

            var data = SprinklerData.ReadSprinklerData(SelectedSprinkler, true);
            var validNewTiles = GetValidNewTiles();

            var cursor = Helper.Input.GetCursorPosition();
            var targetTile = Game1.options.gamepadControls ? cursor.GrabTile : cursor.Tile;
            var graph = data.SprinklerTiles.DeepClone();
            graph.Add(SelectedSprinkler.TileLocation);
            graph.Remove(targetTile);
            var tilesToRemove = GraphUtils.GetDisconnected(graph, SelectedSprinkler.TileLocation).ToArray()
                .AddToArray(targetTile);
            // tilesToRemove.AddItem(targetTile);

            foreach (var sprinklerTile in data.SprinklerTiles)
            {
                DrawUtils.DrawTileHighlight(sprinklerTile,
                    tilesToRemove.Contains(sprinklerTile) ? Color.DarkRed : Color.Green);
            }

            foreach (var validNewTile in validNewTiles)
            {
                if (!validNewTile.Equals(targetTile))
                {
                    if (DateTime.Now.Millisecond % 1000 <= 500)
                    {
                        DrawUtils.DrawTileHighlight(validNewTile, Color.DimGray);
                    }
                }
                else
                {
                    DrawUtils.DrawTileHighlight(validNewTile, Color.Blue);
                }
            }
        }

        private HashSet<Vector2> GetValidNewTiles()
        {
            var data = SprinklerData.ReadSprinklerData(SelectedSprinkler, true);

            var adjacentTiles = new HashSet<Vector2>();

            // If no unused tiles exist, then we cannot add new tiles
            if (data.UnusedTileCount == 0) return adjacentTiles;

            void AddAdjacentTiles(Vector2 vector2)
            {
                adjacentTiles.Add(vector2 - new Vector2(0, 1));
                adjacentTiles.Add(vector2 - new Vector2(0, -1));
                adjacentTiles.Add(vector2 - new Vector2(1, 0));
                adjacentTiles.Add(vector2 - new Vector2(-1, 0));
            }

            AddAdjacentTiles(SelectedSprinkler.TileLocation);
            foreach (var sprinklerTile in data.SprinklerTiles)
            {
                AddAdjacentTiles(sprinklerTile);
            }

            adjacentTiles.RemoveWhere(x => data.SprinklerTiles.Contains(x) || x.Equals(SelectedSprinkler.TileLocation));

            return adjacentTiles;
        }
    }
}