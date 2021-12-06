using System;
using System.Collections.Generic;
using System.Linq;
// using ConfigurableSprinklers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Object = StardewValley.Object;

namespace AdjustableSprinklers
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private StardewValley.Object SelectedSprinkler;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SprinklerPatches.Initialize(Monitor, Helper);
            SprinklerData.Initialize(Helper);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            // helper.Events.Input.CursorMoved += this.OnCursorMoved;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object),
                    nameof(StardewValley.Object.GetSprinklerTiles)),
                prefix: new HarmonyMethod(typeof(SprinklerPatches), nameof(SprinklerPatches.GetSprinklerTiles_Prefix))
            );
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            // this.Monitor.Log($"{Game1.player.Name} mouse cursor from {e.OldPosition.GrabTile.X},{e.OldPosition.GrabTile.Y}.", LogLevel.Debug);
            // this.Monitor.Log($"{Game1.player.Name} mouse cursor to {e.NewPosition.GrabTile.X},{e.NewPosition.GrabTile.Y}.", LogLevel.Debug);

            // DrawUtils.DrawSprite(e.NewPosition.GrabTile, SpriteInfo.RED_HIGHLIGHT);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
            if (!e.Button.IsActionButton())
                return;

            // print button presses to the console window
            Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            var grabTile = Game1.player.GetGrabTile();
            Object selectedObject;
            Game1.currentLocation.objects.TryGetValue(grabTile, out selectedObject);
            if (selectedObject is not null && selectedObject.IsSprinkler())
            {
                SelectedSprinkler = SelectedSprinkler is null ? selectedObject : null;
            }
            else
            {
                if (SelectedSprinkler is null)
                    // Not currently configuring a sprinkler
                    return;

                var data = SprinklerData.ReadSprinklerData(SelectedSprinkler);
                if (data is null)
                {
                    data = new SprinklerData();
                    data.SprinklerTiles = SelectedSprinkler.GetSprinklerTiles();
                    data.UnusedTileCount = 0;
                }
                if (data.SprinklerTiles.Contains(grabTile))
                {
                    data.SprinklerTiles.Remove(grabTile);
                    data.UnusedTileCount += 1;
                }
                else
                {
                    if (data.UnusedTileCount <= 0) return;
                    
                    data.SprinklerTiles.Add(grabTile);
                    data.UnusedTileCount -= 1;
                }
                SprinklerData.WriteSprinklerData(SelectedSprinkler, data);
            }
        }
    }
}