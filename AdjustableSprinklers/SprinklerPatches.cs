using System;
using System.Collections.Generic;
using AdjustableSprinklers;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;

public class SprinklerPatches
{
    private static IMonitor Monitor;
    private static IModHelper Helper;

    // call this method from your Entry class
    public static void Initialize(IMonitor monitor, IModHelper helper)
    {
        Monitor = monitor;
        Helper = helper;
    }

    public static bool GetSprinklerTiles_Prefix(StardewValley.Object __instance, ref List<Vector2> __result)
    {
        if (!__instance.IsSprinkler())
            return true;

        try
        {
            var sprinklerData = SprinklerData.ReadSprinklerData(__instance);

            // If it is not configured, then we let the unpatched method run
            if (sprinklerData is null)
                return true;
            
            __result = sprinklerData.SprinklerTiles;
            return false;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in GetSprinklerTiles_Prefix:\n{ex}", LogLevel.Error);
            return true; // run original logic
        }
    }
}