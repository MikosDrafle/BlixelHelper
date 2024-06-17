using Celeste.Mod.BlixelHelper.utils;
using Celeste.Mod.CommunalHelper.Entities;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Celeste.Mod.BlixelHelper.Entities
{
    public class Trail : Solid
    {
        private bool DoRender = false;

        public Color DisplayColor = Color.White;

        private float RectTimer = 40;
        public Trail(EntityData data, Vector2 offset) : base(data.Position, data.Width, data.Height, true) {
            Collidable = false;
            Depth = Depths.SolidsBelow + 1;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            DoRender = true;
        }
        public override void Render()
        {
            base.Render();

            if (RectTimer > 0 && DoRender)
            {
                RectTimer--;
                Draw.HollowRect(X, Y, Width, Height, (DisplayColor*(RectTimer/40f))*0.5f);
            }
            else
            {
                RemoveSelf();
            }
        }
    }

    [CustomEntity("BlixelHelper/FactoryBounceBlock")]
    [Tracked(true)]
    public class FactoryBounceBlock : Solid // simple entity that acts like a BounceBlock, except in random directions
    {
        private float PrepareTime = 0.1f;
        private float BounceTime = 0.2f;
        private float MoveBackTime = 0.4f;
        private float ReactivationDelay = 3f;
        private float MaxBounceDist = 20f;
        private float RepulseFactor = 3.2f;

        private bool Useable;

        private Vector2 ogSize;

        private Hitbox hitbox;

        private readonly Vector2 SavedPos;

        private readonly Vector2 SavedCenter;

        private readonly Vector2 BounceDirection = Vector2.Zero;

        const float empty = 0f;

        const float spacing = 8f;

        private Vector2 scale = Vector2.One;

        private Color drawColor = Color.White;

        private Color UnderlayColor = Color.LightGreen;

        private MTexture[,] blockTiles;

        private float ColorLerp = 0f;

        private float CurveTransparency = 1f;

        private int MoveDirections = 8;

        private Trail[] trails;


        private SolidTiles tiles;
        public FactoryBounceBlock(EntityData data, Vector2 offset) : base(data.Position+offset, data.Width, data.Height, true)
        {
            Collider = (hitbox = new Hitbox(data.Width, data.Height));
            ogSize = new Vector2(data.Width, data.Height);
            Collidable = true; // being paranoid
            SavedPos = base.Position;
            SavedCenter = SavedPos + new Vector2(Width / 2, Height / 2);
            OnDashCollide += OnDashed;

            ReactivationDelay = data.Float("reformTime");
            RepulseFactor = data.Float("repulseFactor");
            MoveDirections = data.Int("moveDirections");

            // monstrous ninepatch code ahead

            MTexture mainTex = GFX.Game["objects/bouncer/idle00"];

            MikoUtils.PrepareTiles(this, mainTex, out blockTiles);

            Depth = Depths.Solids;
        }

        private IEnumerator MovementCoroutine(Player player) // movement of the bounce block
        {
            yield return null;

            Vector2 centeredPosition = Position + new Vector2(Width / 2, Height / 2);
            Vector2 playerPosition = player.Position;
            Vector2 direction = (centeredPosition - playerPosition).SafeNormalize();

            direction = MikoUtils.RoundDirections(direction, MoveDirections);
            Audio.Play(CommunalHelper.CustomSFX.game_aero_block_impact);

            Tween beginTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineOut, PrepareTime);
            beginTween.OnUpdate = (Tween tween) =>
            {

                Vector2 newPosition = SavedPos + (direction * MaxBounceDist);

                MoveTo(Vector2.Lerp(SavedPos, newPosition, tween.Eased));
            };
            Add(beginTween);
            beginTween.Start();

            yield return PrepareTime;

            // add trail coroutine

            Add(new Coroutine(CreateTrails(this)));

            Tween bounceTween = Tween.Create(Tween.TweenMode.Persist, Ease.Linear, BounceTime);
            bounceTween.OnUpdate = (Tween tween) =>
            {

                Vector2 dir = Vector2.Lerp(Vector2.Zero, (-direction * MaxBounceDist * RepulseFactor * 10), MathF.Sin(tween.Eased*180f*Calc.DegToRad));
                 
                this.Speed = dir;
            };
            Add(bounceTween);
            bounceTween.Start();

            yield return BounceTime;

            Vector2 newPos = new Vector2(Position.X, Position.Y);

            Tween returnTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, MoveBackTime);
            returnTween.OnUpdate = (Tween tween) =>
            {
                MoveTo(Vector2.Lerp(newPos, SavedPos, tween.Eased));
            };
            Add(returnTween);
            returnTween.Start();

            yield return 0.2f;

            Tween transparencyTween = Tween.Create(Tween.TweenMode.Persist, Ease.Linear, 0.4f);
            transparencyTween.OnUpdate = (Tween tween) =>
            {
                ColorLerp = tween.Eased * 0.5f;
                CurveTransparency = 1f - tween.Eased;
            };
            Add(transparencyTween);
            transparencyTween.Start();

            yield return ReactivationDelay + 0.4f;

            Audio.Play(SFX.char_bad_booster_reappear);

            CurveTransparency = 1f;
            ColorLerp = 0f;

            Useable = true;
        }

        private void ShakeTiles(Vector2 dir) // shakes on dash collision
        {
            drawColor = Color.LimeGreen;

            // color tweening
            Tween colorTween = Tween.Create(Tween.TweenMode.Persist, Ease.Linear, 0.5f);
            colorTween.OnUpdate = (Tween tween) =>
            {
                drawColor = Color.Lerp(Color.LimeGreen, Color.White, tween.Eased);
            };
            Add(colorTween);
            colorTween.Start();
            // bounce

            Vector2 BounceScale = new Vector2(
                1f + (Math.Abs(dir.Y) * 0.35f) - (Math.Abs(dir.X) * 0.35f),
                1f + (Math.Abs(dir.X) * 0.35f) - (Math.Abs(dir.Y) * 0.35f));
            Tween bounce = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, 0.1f);
            bounce.OnUpdate = (Tween tween) =>
            {
                scale = Vector2.Lerp(BounceScale, Vector2.One, tween.Eased);
            };
            Add(bounce);
            bounce.Start();
        }

        private IEnumerator CreateTrails(Solid entity)
        {
            yield return null;
            
            for (int i = 0; i < 3; i++)
            {
                EntityData data = new EntityData();
                data.Position = entity.Position;
                data.Width = (int)entity.Width;
                data.Height = (int)entity.Height;

                Trail trail = new Trail(data, Vector2.Zero);

                trail.DisplayColor = UnderlayColor;

                Scene.Add(trail);

                yield return 0.01f;
            }
        }
        
        public override void Render() // more tile rendering from CH!
        {
            // draw mount glow

            Draw.Circle(SavedCenter, 11f, UnderlayColor * 0.25f, 12);
            Draw.Circle(SavedCenter, 9f, UnderlayColor * 0.35f, 12);
            Draw.Circle(SavedCenter, 7f, UnderlayColor * 0.6f, 12);
            Draw.Circle(SavedCenter, 5f, UnderlayColor, 12);

            // draw mount

            Draw.Circle(SavedCenter, 6f, Color.Black, 12);

            int tileWidth = (int)(Width / 8f);
            int tileHeight = (int)(Height / 8f);

            for (int i=0; i<tileWidth; i++)
            {
                for (int j=0; j<tileHeight; j++)
                {
                    Vector2 vec = new Vector2(X + (i * 8), Y + (j * 8)) + Vector2.One*4f;
                    vec = Center + ((vec - Center) * scale);
                    blockTiles[i, j].DrawCentered(vec, Color.Lerp(drawColor, Color.Black, ColorLerp), scale);
                }
            }

            base.Render();
        }

        private DashCollisionResults OnDashed(Player player, Vector2 dir)
        {
            if (((player.Left >= Right - 4f || player.Right < Left + 4f) && dir.Y == -1) || (player.CollideCheck<Spikes>() && !SaveData.Instance.Assists.Invincible))
            {
                return DashCollisionResults.NormalCollision;
            }

            if (Useable)
            {
                if (player.Dashes<player.MaxDashes)
                {
                    player.Dashes++;
                }
                Useable = false;

                ShakeTiles(dir);

                Coroutine cor = new Coroutine(MovementCoroutine(player));
                Add(cor);
            }

            Vector2 applySpeed = new Vector2(((player.Right>Left && player.Left<Right ) ? 0f : (float)Math.Sign(player.X - X) * 120f), -160f);

            player.StateMachine.State = Player.StNormal;
            player.Speed = applySpeed;
            player.LiftSpeed = applySpeed;

            return DashCollisionResults.NormalCollision;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            EnableStaticMovers();

            foreach (Spikes spikes in this.staticMovers.OfType<Spikes>())
            {
                foreach (Image image in spikes.Components.OfType<Image>())
                {
                    image.Color = Color.Red;
                }
            }

            Useable = true;
        }
    }
}