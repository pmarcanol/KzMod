using DuckGame;
using System.Collections.Generic;

namespace KzDuckMods.Things
{
    public class ElectricalChargeSafe : ElectricalCharge
    {
        private List<Vec2> _prevPositions = new List<Vec2>();
        private Vec2 _travelVec;
        private float alphaDecrease;

        public ElectricalChargeSafe(float xpos, float ypos, float alphaDecrease, Thing own) : base(xpos, ypos, 1, own)
        {
            this.alphaDecrease = alphaDecrease;
            this.offDir = (sbyte)1;
            this._travelVec = new Vec2(Rando.Float(-10f, 10f), Rando.Float(-10f, 10f));
            this.owner = own;
        }

        public override void Update()
        {
            if (this._prevPositions.Count == 0)
                this._prevPositions.Insert(0, this.position);
            Vec2 position = this.position;
            ElectricalChargeSafe electricalCharge = this;
            electricalCharge.position = electricalCharge.position + this._travelVec;
            this._travelVec = new Vec2(Rando.Float(-10f, 10f), Rando.Float(-10f, 10f));
            this._prevPositions.Insert(0, this.position);
            this.alpha -= alphaDecrease;
            if ((double)this.alpha < 0.0)
                Level.Remove((Thing)this);
        }

        public override void Draw()
        {
            Vec2 p2 = Vec2.Zero;
            bool flag = false;
            float num = 1f;
            foreach (Vec2 prevPosition in this._prevPositions)
            {
                if (!flag)
                {
                    flag = true;
                    p2 = prevPosition;
                }
                else
                {
                    Graphics.DrawLine(prevPosition, p2, Colors.DGYellow * num, 0.5f, (Depth)0.5f);
                    num -= 0.50f;
                }
                if ((double)num <= 0.0)
                    break;
            }
            base.Draw();
        }
    }


}
