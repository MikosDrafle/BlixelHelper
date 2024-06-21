using Celeste.Mod.BlixelHelper.utils;
using Celeste.Mod.CommunalHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.Entities.Solids
{
    public class DashPulsePathRenderer : Entity
    {
        public DashPulseBlock DashPulseBlock;

        private MTexture cog;

        private Vector2 from;

        private Vector2 to;

        private Color ropeColor;
        private Color highlightColor;

        public DashPulsePathRenderer(DashPulseBlock pulseBlock)
        {
            base.Depth = 5000;
            DashPulseBlock = pulseBlock;
            ropeColor = DashPulseBlock.ropeColor;
            highlightColor = DashPulseBlock.highlightColor;

            from = DashPulseBlock.start + new Vector2(DashPulseBlock.Width / 2, DashPulseBlock.Height / 2);
            to = DashPulseBlock.end + new Vector2(DashPulseBlock.Width / 2, DashPulseBlock.Height / 2);

            cog = GFX.Game["objects/dashpulse/cogOuter"];
        }

        public override void Render()
        {
            DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
            base.Render();
        }

        public void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            Color rC = colorOverride.HasValue ? colorOverride.Value : ropeColor;
            Color hlC = colorOverride.HasValue ? colorOverride.Value : highlightColor;
            Color cC = colorOverride.HasValue ? colorOverride.Value : Color.White;

            Vector2 direction = (to - from).SafeNormalize();

            Vector2 topRopeOffset = direction.Perpendicular() * 3f;
            Vector2 bottomRopeOffset = -direction.Perpendicular() * 4f;

            float rotation = DashPulseBlock.percent * MathF.PI * 2f;

            Draw.Line(from + topRopeOffset + offset, to + topRopeOffset + offset, rC);
            Draw.Line(from + bottomRopeOffset + offset, to + bottomRopeOffset + offset, rC);

            for (float num = 4f - DashPulseBlock.percent * MathF.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
            {
                Vector2 topTrackOffset = from + topRopeOffset + direction.Perpendicular() + direction * num;
                Vector2 bottomTrackOffset = to + bottomRopeOffset - direction * num;

                Draw.Line(topTrackOffset + offset, topTrackOffset + direction * 2f + offset, hlC);
                Draw.Line(bottomTrackOffset + offset, bottomTrackOffset - direction * 2f + offset, hlC);
            }

            cog.DrawCentered(from + offset, cC, 1f, rotation);
            cog.DrawCentered(to + offset, cC, 1f, rotation);
        }
    }

    [CustomEntity("BlixelHelper/DashPulseBlock")]
    [Tracked]
    public class DashPulseBlock : Solid
    {
        private Sprite pulseArrow;
        private DashPulsePathRenderer renderer;

        internal Vector2 start;
        internal Vector2 end;

        public float PulseEndTime = 1f;
        public float PulseStrength = 256f;
        public float zeroPullSpeed = 128f;

        internal bool wallBouncePulse;

        internal int amountRefill;

        internal float percent;

        internal float distance;

        internal float adder;

        internal float pulseCooldown;

        internal static Color ropeColor = Calc.HexToColor("64CCA2");

        internal static Color highlightColor = Calc.HexToColor("7DE893");

        MTexture[,] renderPatch;

        MTexture[,] wouncePatch;

        MTexture cog;

        EventInstance moving;
        public DashPulseBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            Collidable = true;
            MTexture loadPatch = GFX.Game["objects/dashpulse/idle00"];

            MikoUtils.PrepareTiles(this, loadPatch, out renderPatch);
            MikoUtils.PrepareTiles(this, GFX.Game["objects/dashpulse/wounceTexture"], out wouncePatch);

            cog = GFX.Game["objects/dashpulse/cog"];

            start = Position;
            end = data.NodesOffset(offset)[0];

            adder = 0;
            percent = 0;
            distance = Vector2.Distance(start, end);

            OnDashCollide += Collide;

            PulseStrength = data.Float("pulseStrength");
            zeroPullSpeed = data.Float("zeroPullStrength");
            PulseEndTime = data.Float("pulseEndTime");
            wallBouncePulse = data.Bool("wallBouncePulse");
            amountRefill = data.Int("refillDashes");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(renderer = new DashPulsePathRenderer(this));
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(renderer);
            renderer = null;
            base.Removed(scene);
        }
        public override void Update()
        {
            base.Update();
            float totalApproach = (zeroPullSpeed - (adder)) * Engine.DeltaTime;

            adder = Calc.Approach(adder, 0, (PulseStrength / PulseEndTime) * Engine.DeltaTime);

            percent = Calc.Approach(percent, MathF.Sign(totalApproach) == 1 ? 0 : 1, MathF.Abs(totalApproach) / distance);
            MoveTo(Vector2.Lerp(start, end, percent));
        }

        private DashCollisionResults Collide(Player player, Vector2 dir)
        {
            if ((player.Left >= Right - 4f || player.Right < Left + 4f) && dir.Y == -1)
            {
                return DashCollisionResults.NormalCollision;
            }
            Pulse(player);
            if ((dir.Y != 0 && player.Speed.X != 0) || (dir.X != 0 && Input.Grab.Check))
            {
                if (dir.X != 0 && Input.GrabCheck)
                {
                    float totalApproach = (zeroPullSpeed - (adder));
                    player.Speed = (Vector2.UnitX * 5f * (player.Facing == Facings.Left ? -1f : 1f)) + (end - start).SafeNormalize() * Math.Abs(totalApproach);
                }
                return DashCollisionResults.NormalCollision;
            }
            return DashCollisionResults.Rebound;
        }
        internal void Pulse(Player player)
        {
            Audio.Play(CustomSFX.game_aero_block_impact);
            if (amountRefill != 0)
            {
                player.RefillStamina();
            }

            if (amountRefill < 0)
            {
                player.RefillDash();
            }
            else
            {
                if (amountRefill != 0)
                {
                    player.Dashes = (int)Math.Clamp(player.Dashes + amountRefill, 0, amountRefill);
                }
            }
            adder = PulseStrength;
        }

        public override void Render()
        {

            int TileWidth = (int)(Width / 8f);
            int TileHeight = (int)(Height / 8f);

            Draw.Rect(Position - Vector2.One, Width + 2f, Height + 2f, Color.Black);

            for (float x = 4f; x <= Width - 4f; x += 8f)
            {
                for (float y = 4f; y <= Height - 4f; y += 8f)
                {
                    float normalX = x - 4f;
                    float normalY = y - 4f;

                    bool Reverse = mod(normalY, 16) == 8f ? true : false;

                    if (mod(normalX, 16) == 8f)
                    {
                        Reverse = !Reverse;
                    }
                    float RotationWindingOrder = Reverse ? -1f : 1f;
                    Color renderColor = Reverse ? Color.LightGray : Color.White;
                    renderColor = Color.Lerp(renderColor, Color.Black, (y / Height) * 0.9f);
                    float rotation = (percent * MathF.PI * 4f) * RotationWindingOrder;
                    rotation += Reverse ? 22.5f : 0f;

                    cog.DrawCentered(Position + new Vector2(x, y), renderColor, 1.14f, rotation);
                }
            }

            Color borderColor = amountRefill switch
            {
                0 => Color.Black,
                1 => Color.LightGreen,
                2 => Color.LightPink,
                _ => Color.White
            };

            Draw.HollowRect(Position - Vector2.One, Width + 2f, Height + 2f, borderColor);

            for (int x = 0; x < TileWidth; x++)
            {
                for (int y = 0; y < TileHeight; y++)
                {
                    Vector2 vec = new Vector2(X + (x * 8), Y + (y * 8)) + Vector2.One * 4f;
                    vec = Center + ((vec - Center));
                    renderPatch[x, y].DrawCentered(vec, Color.White);
                }
            }

            if (wallBouncePulse)
            {
                for (int x = 0; x < TileWidth; x++)
                {
                    for (int y = 0; y < TileHeight; y++)
                    {
                        Vector2 vec = new Vector2(X + (x * 8), Y + (y * 8)) + Vector2.One * 4f;
                        vec = Center + ((vec - Center));
                        wouncePatch[x, y].DrawCentered(vec, Color.White);
                    }
                }
            }

            base.Render();
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
