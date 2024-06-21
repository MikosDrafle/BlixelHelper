using Celeste.Mod.BlixelHelper.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Triangulator;

namespace Celeste.Mod.BlixelHelper.Entities
{
    [CustomEntity("BlixelHelper/ArbitraryShapeEntity")]
    [Tracked]
    public class ArbitraryShapeEntity : Entity
    {
        public Color color;

        public Vector2[] nodes;

        public VertexPositionColor[] objectVertices;

        public List<Vector3> verticesRelative;

        public List<Vector3> verticesSave;

        private int VertexLength;

        private string Effect;

        private float markerMovement;

        private float markerInterval;

        private float LeftmostX;

        internal string windingOrderString;
        public ArbitraryShapeEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            windingOrderString = data.Attr("windingOrder");
            Effect = data.Attr("effect");
            markerMovement = data.Float("markerEffectPixels");
            markerInterval = data.Float("markerInterval");
            nodes = data.NodesOffset(offset);

            for (int i = 0; i<nodes.Length; i++)
            {
                nodes[i].X = nodes[i].X;
                nodes[i].Y = nodes[i].Y;
            }
            color = data.HexColor("color", Color.White);
            Depth = data.Int("depth");

            objectVertices = ArbitraryShapeHelper.GetFillVertsFromNodes(this, Vector2.Zero, color, Effect=="Marker" ? markerMovement : 0f);
            verticesRelative = new List<Vector3>();
            VertexLength = objectVertices.Length;

            for (int i = 0; i < VertexLength; i++)
            {
                ref var vert = ref objectVertices[i];

                verticesRelative.Insert(i, new(vert.Position.X - X, vert.Position.Y - Y, vert.Position.Z));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            if (Effect=="Marker")
                Add(new Coroutine(MarkerRoutine()));
        }

        // effects for polygons

        public IEnumerator MarkerRoutine()
        {
            objectVertices = ArbitraryShapeHelper.GetFillVertsFromNodes(this, Vector2.Zero, color, Effect == "Marker" ? markerMovement : 0f);
            verticesRelative = new List<Vector3>();
            VertexLength = objectVertices.Length;

            for (int i = 0; i < VertexLength; i++)
            {
                var vert = objectVertices[i];

                verticesRelative.Insert(i, new Vector3(vert.Position.X - X, vert.Position.Y - Y, 0f));
            }

            yield return markerInterval;

            Add(new Coroutine(MarkerRoutine()));
        }

        public override void Render()
        {
            base.Render();

            Camera camera = (Scene as Level).Camera;

            GameplayRenderer.End();
            for (int i = 0; i < VertexLength; i++)
            {
                ref var vert = ref objectVertices[i];

                vert.Position = new Vector3(X, Y, 0f) + verticesRelative[i];
            }

            GFX.DrawVertices(camera.Matrix, objectVertices, objectVertices.Length, null, null);
            GameplayRenderer.Begin();
        }
    }
}
