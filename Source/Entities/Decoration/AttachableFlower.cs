using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.Entities.Decoration
{
    [CustomEntity("BlixelHelper/AttachableFlower")]
    [Tracked]
    public class AttachableFlower : Entity
    {
        private Color flowerColor;

        private MTexture flowerTexture;

        private float rotation;

        private float newRotation;

        private Solid attachEntity;

        private Vector2? attachEntityVector;

        private SineWave rotationalWave;
        public AttachableFlower(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

            Depth = Depths.Top;
            flowerColor = data.HexColor("color");
            flowerTexture = GFX.Game["objects/attachableFlowers/idle00"];

            Calc.PushRandom((int)(X + Y));
            rotation = Calc.Random.NextAngle();

            rotationalWave = new SineWave((float)Calc.Random.Next(6,52)/100);
            rotationalWave.Randomize();

            float rotationRandomize = Calc.Random.Next(11, 18)*Calc.DegToRad;
            Calc.PopRandom();
            rotationalWave.OnUpdate = (float sine) =>
            {
                newRotation = Calc.WrapAngle(rotation + (rotationRandomize * sine));
            };

            Add(rotationalWave);

            Add(new StaticMover
            {
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding
            });
        }

        public override void Render()
        {
            base.Render();

            flowerTexture.DrawCentered(Position+Vector2.UnitY, Color.Lerp(flowerColor, Color.Black, 0.4f), 1f, newRotation);
            flowerTexture.DrawCentered(Position, flowerColor, 1f, newRotation);
            Draw.Rect(Position - new Vector2(1,1), 2, 2, Color.White);
        }

        private bool IsRiding(Solid solid)
        {
            Vector2 centerPos = Position;
            if ((centerPos.X >= solid.Left && centerPos.X <= solid.Right) && (centerPos.Y >= solid.Top && centerPos.Y <= solid.Bottom))
            {
                Depth = solid.Depth;
                solid.Depth++;
                return true;
            }

            return false;
        }

        private bool IsRiding(JumpThru solid)
        {
            Vector2 centerPos = Position;
            if ((centerPos.X >= solid.Left && centerPos.X <= solid.Right) && (centerPos.Y >= solid.Top && centerPos.Y <= solid.Bottom))
            {
                Depth = solid.Depth;
                solid.Depth++;
                return true;
            }

            return false;
        }
    }
}
