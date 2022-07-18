using DuckGame;

namespace KzDuckMods.Things
{
    public class Flash : Thing
    {
        public float Timer;

        public Flash(float stayTime = 2.2f) : base(0, 0)
        {
            Timer = stayTime;
        }

        public override void Update()
        {
            base.Update();

            if (Timer > 0)
            {
                Timer -= 0.01f;
            }
            else
            {
                Level.Remove(this);
            }
        }

        public override void Draw()
        {
            base.Draw();
            Graphics.flashAdd = 1.3f;
        }


    }
}
