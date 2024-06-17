using Celeste.Mod.BlixelHelper.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.Entities.Solids
{
    [CustomEntity("BlixelHelper/PrimeBlock")]
    [Tracked(false)]
    public class PrimeBlock : Solid
    {
        private MTexture[,] renderTiles;

        private MTexture capSprite;

        private MTexture primeText;

        internal float TextScale;
        public PrimeBlock(EntityData data, Vector2 offset) : base(data.Position+offset, data.Width, data.Height, false)
        {
            MikoUtils.PrepareTiles(this, GFX.Game["objects/primeBlocks/block"], out renderTiles);
            capSprite = GFX.Game["objects/primeBlocks/cap"];
            primeText = GFX.Game["objects/primeBlocks/text"];

            Depth = Depths.Solids;

            Collidable = false;

            TextScale = 1f;

            if (Width <= 32f || Height <= 32f)
            {
                TextScale = 0.75f;
            }

            if (Width <= 24f || Height <= 24f)
            {
                TextScale = 0.4f;
            }
        }

        public override void Update()
        {
            base.Update();

            Player player = Scene.Entities.OfType<Player>().FirstOrDefault();

            if (player is not null)
            {
                if (player.Left <= Right+1 && player.Right >= Left-1 && player.Top <= Bottom+1 && player.Bottom >= Top-1)
                {
                    player.Die((player.Center - base.Center).SafeNormalize());
                }
            }
        }

        public override void Render()
        {
            base.Render();
            int TileWidth = (int)Width / 8;
            int TileHeight = (int)Height / 8;

            for (int x = 0; x < TileWidth; x++)
            {
                for (int y = 0; y < TileHeight; y++)
                {
                    Vector2 vec = new Vector2(X + (x * 8), Y + (y * 8)) + Vector2.One * 4f;
                    var random1 = new Vector2(0.4f + Calc.Random.NextFloat(0.8f), 0.4f + Calc.Random.NextFloat(0.8f));
                    vec = Center + ((vec - Center) + random1);
                    renderTiles[x, y].DrawOutlineCentered(vec, Color.Black);
                }
            }

            for (int x = 0; x < TileWidth; x++)
            {
                for (int y = 0; y < TileHeight; y++)
                {
                    Vector2 vec = new Vector2(X + (x * 8), Y + (y * 8)) + Vector2.One * 4f;
                    var random1 = new Vector2(0.4f + Calc.Random.NextFloat(0.8f), 0.4f + Calc.Random.NextFloat(0.8f));
                    vec = Center + ((vec - Center) + random1);
                    renderTiles[x, y].DrawCentered(vec, Color.White);
                }
            }
            Vector2 randomness = new Vector2(0.4f + Calc.Random.NextFloat(0.8f), 0.4f + Calc.Random.NextFloat(0.8f));
            primeText.DrawCentered(Position + new Vector2(Width / 2f, Height / 2f) + randomness, Color.White, new Vector2(Width/(primeText.Width) - (4f/primeText.Width), Height/(primeText.Height) - (4f/primeText.Height)));
            capSprite.DrawOutlineCentered(Position + new Vector2(Width / 2f, -4), Color.Black);
            capSprite.DrawCentered(Position + new Vector2(Width/2f, -4));
        }
    }
}
