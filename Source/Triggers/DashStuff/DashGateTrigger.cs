using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BlixelHelper.Triggers.DashStuff
{
    [CustomEntity("BlixelHelper/DashGateTrigger")]
    public class DashGateTrigger:Trigger
    {
        public Vector2 dashDir;

        public TempleGate templeNode;

        private DashListener CurrentListener;

        private Vector2 selNode;
        public DashGateTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashDir = new Vector2(data.Float("dirX"), data.Float("dirY")).SafeNormalize();

            var nodes = data.NodesOffset(offset);

            if (nodes.Length > 0)
            {
                selNode = nodes[0];
            }
        }

        private void Dash(Vector2 dir) {
            var rectNode = new Rectangle((int)selNode.X, (int)selNode.Y, 1, 1);

            foreach (TempleGate gate in Scene.Entities.OfType<TempleGate>())
            {
                var rectGate = new Rectangle((int)gate.TopLeft.X, (int)gate.TopLeft.Y, (int)gate.Width, (int)gate.Height);

                if (rectGate.Intersects(rectNode))
                {
                    templeNode = gate;
                }
            }

            if (templeNode!=null && dir==dashDir && PlayerIsInside)
            {
                templeNode.Open();
                foreach (SeekerBarrier barrier in Scene.HelperEntity.OfType<SeekerBarrier>())
                {
                    var rectBarrier = new Rectangle((int)barrier.TopLeft.X, (int)barrier.TopLeft.Y, (int)barrier.Width, (int)barrier.Height);
                    var rectThis = new Rectangle((int)TopLeft.X, (int)TopLeft.Y, (int)Width, (int)Height);

                    if (rectThis.Intersects(rectBarrier))
                    {
                        barrier.Flash = 1f;
                        barrier.Flashing = true;
                    }
                }
            }
        }

        public override void Added(Scene scene)
        {
            CurrentListener = new DashListener(Dash);
            Add(CurrentListener);
            base.Added(scene);
        }

        public override void Removed(Scene scene)
        {
            CurrentListener.RemoveSelf();
            base.Removed(scene);
        }
    }
}
