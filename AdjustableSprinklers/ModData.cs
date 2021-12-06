using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AdjustableSprinklers
{
    public class SprinklerData
    {
        static IModHelper Helper { get; set; }

        public static void Initialize(IModHelper helper)
        {
            Helper = helper;
        }

        public List<Vector2> SprinklerTiles { get; set; }
        public int UnusedTileCount { get; set; }
        // public int ExampleNumber { get; set; }

        public static string GetSaveKey(Object sprinkler)
        {
            var mapId = Game1.currentLocation.map.Id;
            return $"sprinkler-{sprinkler.tileLocation.X}-{sprinkler.tileLocation.Y}";
        }
        
        public static SprinklerData ReadSprinklerData(Object sprinkler)
        {
            return Helper.Data.ReadSaveData<SprinklerData>(GetSaveKey(sprinkler));
        }

        public static void WriteSprinklerData(Object sprinkler, SprinklerData data)
        {
            Helper.Data.WriteSaveData(GetSaveKey(sprinkler), data);
        }
    }
}