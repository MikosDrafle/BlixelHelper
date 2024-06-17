using Celeste.Mod.BlixelHelper.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.Entities.Hazards
{
    public class GlowstickConnection : Entity
    {
        private GlowstickSpinner reference;

        internal MTexture ConnectionTexture;

        internal float Rotation;
        public GlowstickConnection(Vector2 position, GlowstickSpinner refEntity)
        {
            reference = refEntity;
            Position = position;
            Depth = refEntity.Depth + 1;
            ConnectionTexture = refEntity.connectionSprite;
            Collider = new Hitbox(8f, 8f);

            Calc.PushRandom((int)(X + Y)^2);
            Rotation = Calc.Random.Next(0, 3) * (MathF.PI / 2);
            Calc.PopRandom();
        }

        public override void Render()
        {
            base.Render();
            ConnectionTexture.DrawCentered(Center, Color.White, 1f, Rotation);
        }
    }
    [CustomEntity("BlixelHelper/GlowstickSpinner")]
    [Tracked]
    public class GlowstickSpinner : Entity
    {
        private class SpinnerBorder : Entity
        {
            private GlowstickSpinner borderParent;

            public SpinnerBorder(GlowstickSpinner parent)
            {
                borderParent = parent;
                Position = borderParent.Center;
                Depth = borderParent.Depth + 2;
            }

            private void DrawSpriteOnPosition(MTexture sprite, Vector2 position, float scale = 1f, float rotation = 0f)
            {
                for (int i = 0; i < 4; i++) // doing math stuff in a for loop because i love clean code!!!
                {
                    Vector2 drawVector = position + Calc.Rotate(Vector2.UnitY, i * (MathF.PI / 2f)); //rotate for all 4 axis
                    sprite.DrawCentered(drawVector, Color.Black, scale, rotation);
                }
            }

            public override void Render()
            {
                base.Render();

                DrawSpriteOnPosition(borderParent.spinnerSprite, borderParent.Center, 1f, borderParent.SpinnerRotation);
                if (borderParent.IsConnected && borderParent.Connection is not null)
                {
                    DrawSpriteOnPosition(borderParent.Connection.ConnectionTexture, (Vector2)borderParent.Connection.Center, 1f, borderParent.Connection.Rotation);
                }
            }
        }

        private SpinnerBorder border;
        private float ConnectMaxDistance { get; } = 20f;

        private bool ConnectToSolids { get; set; }

        internal GlowstickSpinner ConnectedEntity { get; set; }

        internal GlowstickConnection Connection { get; set; }

        private bool IsConnected
        {
            get
            {
                return ConnectedEntity != null || ConnectedEntity.ConnectedEntity == this;
            }
        }

        internal MTexture spinnerSprite;

        internal MTexture connectionSprite;

        private float SpinnerRotation;

        private VertexLight glowLight;
        public GlowstickSpinner(EntityData data, Vector2 offset) : base(data.Position+offset)
        {
            Depth = Depths.CrystalSpinners;
            Collider = new ColliderList(new Circle(6f, 0f, 0f), new Hitbox(16f, 4f, -8f, -3f));
            spinnerSprite = GFX.Game["objects/glowstickSpinner/fg"];
            connectionSprite = GFX.Game["objects/glowstickSpinner/bg"];
            ConnectToSolids = data.Bool("attachToSolid");

            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(glowLight = new VertexLight(Vector2.Zero, Color.LightGreen, 1f, 8,24));

            Calc.PushRandom((int)X + (int)Y);
            SpinnerRotation = Calc.Random.Next(0, 3) * (MathF.PI / 2);
            Calc.PopRandom();
        }

        private void OnPlayer(Player player)
        {
            player.Die((player.Center - Center).SafeNormalize());
        }

        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }

        private void Connect(GlowstickSpinner spinner)
        {
            Vector2 middlePos = (Center + spinner.Center) / 2f;

            ConnectedEntity = spinner;
            ConnectedEntity.ConnectedEntity = this;
            Scene.Add(Connection = new GlowstickConnection(middlePos, this));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (ConnectToSolids)
            {
                Add(new StaticMover
                {
                    SolidChecker = delegate (Solid solid)
                    {
                        Rectangle thisRect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
                        Rectangle solidRect = new Rectangle((int)solid.X, (int)solid.Y, (int)solid.Width, (int)solid.Height);

                        if (thisRect.Intersects(solidRect))
                        {
                            return true;
                        }

                        return false;
                    }
                });
            }
            List<GlowstickSpinner> spinners = new List<GlowstickSpinner>();

            foreach (GlowstickSpinner spinner in scene.Entities.OfType<GlowstickSpinner>())
            {
                if (Vector2.Distance(Center, spinner.Center)<=ConnectMaxDistance)
                {
                    Connect(spinner);
                }
            }

            Scene.Add(border = new SpinnerBorder(this));
        }

        public override void Render()
        {
            base.Render();
            spinnerSprite.DrawCentered(Center, Color.White, 1f, SpinnerRotation);
        }
    }
}
