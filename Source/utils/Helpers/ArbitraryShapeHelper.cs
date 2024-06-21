using Celeste.Mod.BlixelHelper.Entities;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.utils
{
    public class ArbitraryShapeHelper // fillverts code by JaThePlayer (FrostHelper)
    {
        public static Vector2 randScaleModifier(float threshold)
        {
            float x = -threshold + Calc.Random.NextFloat(threshold * 2);
            float y = -threshold + Calc.Random.NextFloat(threshold * 2);

            return new(x,y);
        }
        public static VertexPositionColor[] GetFillVertsFromNodes(ArbitraryShapeEntity entity, Vector2 offset, Color color, float randScale)
        {

            var nodes = entity.nodes;
            Vector2[] input = new Vector2[nodes.Length + 1];
            input[0] = entity.Position + offset + randScaleModifier(randScale);
            for (int i = 1; i < input.Length; i++)
            {
                input[i] = nodes[i - 1] + randScaleModifier(randScale);
            }

            Triangulator.Triangulator.Triangulate(input, Triangulator.WindingOrder.Clockwise, (entity.windingOrderString!="Auto" ? null : (entity.windingOrderString=="Clockwise" ? Triangulator.WindingOrder.Clockwise : Triangulator.WindingOrder.CounterClockwise)), out var verts, out var indices);

            VertexPositionColor[] fill = new VertexPositionColor[1024];
            for (int i = 0; i < indices.Length; i++)
            {
                ref var f = ref fill[i];

                f.Position = new(verts[indices[i]], 0f);
                f.Color = color;
            }

            for (int i = 0; i < fill.Length; i++)
            {
                var fillObj = fill[i];
            }

            return fill;
        }
    }
}
