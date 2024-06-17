using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.utils
{
    public static class MikoUtils
    {
        public static void RenderBetter(this SimpleCurve curve, Vector2 offset, Color color, int resolution, float thickness = 1f)
        {
            Vector2 start = offset + curve.Begin;
            for (int i = 1; i <= resolution; i++)
            {
                Vector2 vector = offset + curve.GetPoint(Ease.Follow(Ease.QuadOut, Ease.QuadIn)((float)i / resolution));
                Draw.Line(start, vector, color, thickness);
                start = vector;
            }
        }

        public static Vector2 RoundDirections(Vector2 dir, int directions)
        {
            float angle = MathF.Atan2(dir.Y, dir.X);
            angle = (int)(MathF.Round((directions / 2) * angle / MathF.PI + directions) % directions) * MathF.PI / (directions / 2);
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
        
        public static void Raycast(Scene scene, Vector2 origin, Vector2 direction, out Vector2 hitPosition, out Vector2 finalDirection)
        {
            hitPosition = origin;
            finalDirection = Vector2.Zero;

            Level sceneAs = (scene as Level);

            while (hitPosition.X >= sceneAs.Bounds.Left && hitPosition.X <= sceneAs.Bounds.Right && hitPosition.Y >= sceneAs.Bounds.Top && hitPosition.Y <= sceneAs.Bounds.Bottom && Vector2.Distance(hitPosition, origin)<=direction.Length())
            {
                hitPosition += direction.SafeNormalize();

                Solid solid = scene.CollideFirst<Solid>(hitPosition);

                if (solid is not null)
                {
                    finalDirection = (origin + direction - origin);
                    break;
                }
            }
        }

        public static List<Entity> UnionEntitiesAlike(List<List<Entity>> nestedEntities)
        {
            List<Entity> union = new List<Entity>(0);
            foreach (List<Entity> nest in nestedEntities)
            {
                foreach (Entity entity in nest)
                {
                    if (!union.Contains(entity))
                    {
                        union.EnsureCapacity(union.Capacity + 1);
                        union.Add(entity);
                    }
                }
            }

            return union;
        }
        public static void PrepareTiles(Solid entity, MTexture tex, out MTexture[,] renderPatch) // ninepatch renderer made possible by catapillie's code!
        {
            MTexture[,] loadedTiles = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    loadedTiles[i, j] = tex.GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }

            int tileWidth = (int)(entity.Width / 8f);
            int tileHeight = (int)(entity.Height / 8f);
            renderPatch = new MTexture[tileWidth, tileHeight];

            for (int i = 0; i < tileWidth; i++)
            {
                for (int j = 0; j < tileHeight; j++)
                {
                    int x = (i != 0) ? ((i != tileWidth - 1f) ? 1 : 2) : 0;
                    int y = (j != 0) ? ((j != tileHeight - 1f) ? 1 : 2) : 0;
                    renderPatch[i, j] = loadedTiles[x, y];
                }
            }
        }
    }
}
