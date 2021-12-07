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

    public static void GetSprinklerTiles_Postfix(StardewValley.Object __instance, ref List<Vector2> __result)
    {
        try
        {
            if (!__instance.IsSprinkler())
                return;
            
            var data = SprinklerData.ReadSprinklerData(__instance, false);
            if (data is not null)
            {
                __result = data.SprinklerTiles;
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(GetSprinklerTiles_Postfix)}:\n{ex}", LogLevel.Error);
        }
    }
}