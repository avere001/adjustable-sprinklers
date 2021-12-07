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

            // print button presses to the console window
            Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            var grabTile = Game1.player.GetGrabTile();
            Game1.currentLocation.objects.TryGetValue(grabTile, out var selectedObject);
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