using DuckGame;
using System;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class VerticalDestroyBeam : DestroyBeam
    {
        public VerticalDestroyBeam(float xpos, float ypos) : base(xpos, ypos)
        {
            this._editorName = "Destroy Beam Vertical";
            this.editorTooltip = "Same as the horizontal destroy beam";
            this.hugWalls = WallHug.Ceiling;
            this.angleDegrees = 90f;
            this.collisionOffset = new Vec2(-5f, -2f);
            this.collisionSize = new Vec2(10f, 4f);
            this._placementCost += 2;
        }

        public override void Draw()
        {
            if (Editor.editorDraw)
                return;

            if (this._prev != this.position)
            {
                this._endPoint = Vec2.Zero;
                for (int index = 0; index < 32; ++index)
                {
                    Thing thing = (Thing)Level.CheckLine<Block>(this.position + new Vec2(0.0f, (float)(4 + index * 16)), this.position + new Vec2(0.0f, (float)((index + 1) * 16 - 6)));
                    if (thing != null)
                    {
                        this._endPoint = new Vec2(this.x, thing.top - 2f);
                        break;
                    }
                }
                this._prev = this.position;
            }
            if (this._endPoint != Vec2.Zero)
            {
                this.graphic.flipH = true;
                this.graphic.depth = this.depth;
                this.graphic.angleDegrees = 90f;
                Graphics.Draw(this.graphic, this._endPoint.x, this._endPoint.y);
                this.graphic.flipH = false;
                this._beam.depth = this.depth - 2;
                float y = this._endPoint.y - this.y;
                int num = (int)Math.Ceiling((double)y / 16.0);
                for (int index = 0; index < num; ++index)
                {
                    if (index == num - 1)
                        this._beam.cutWidth = 16 - (int)((double)y % 16.0);
                    else
                        this._beam.cutWidth = 0;
                    this._beam.angleDegrees = 90f;
                    Graphics.Draw((Sprite)this._beam, this.x, this.y + (float)(index * 16));
                }
                this.collisionOffset = new Vec2(-4f, -1f);
                this.collisionSize = new Vec2(8f, y);
            }
            else
            {
                this.collisionOffset = new Vec2(-5f, -1f);
                this.collisionSize = new Vec2(10f, 4f);
            }
            base.Draw();
        }
    }
}
