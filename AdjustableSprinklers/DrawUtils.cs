using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdjustableSprinklers
{
    static class DrawUtils
    {
        public static void DrawTileHighlight(Vector2 tile, Color tint)
        {
            // Some mathematical magic to allow us to change the color, rather than add a tint
            // https://stackoverflow.com/a/18576834
            var greenAverageColor = new Color(94f, 173f, 59f, 0.49f);
            var q = (greenAverageColor.R + greenAverageColor.G + greenAverageColor.B) / 3f;
            q /= greenAverageColor.A; // optional - undo alpha premultiplication
            tint = new Color(tint.R * q, tint.G * q, tint.B * q, tint.A * greenAverageColor.A);
            
            var viewportLocation = new Vector2(Game1.viewport.X, Game1.viewport.Y);
            var tileScreenLocation = tile * Game1.tileSize - viewportLocation;
            
            const int highlightSpriteTileX = 194;
            const int highlightSpriteTileY = 388;
            Game1.spriteBatch.Draw(
                Game1.mouseCursors,  // Bad name for this, has nothing to with cursors specifically
                tileScreenLocation,
                new Rectangle(highlightSpriteTileX, highlightSpriteTileY, 16, 16),
                tint, 0.0f, Vector2.Zero,
                Game1.pixelZoom, SpriteEffects.None,
                0.01f);
        }
    }
}