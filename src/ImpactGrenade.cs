//using DuckGame;

//namespace KzDuckMods
//{
//    [EditorGroup("KzMod")]
//    internal class ImpactGrenade : Grenade
//    {
//        public SpriteMap sprite;

//        public ImpactGrenade(float xval, float yval) : base(xval, yval)
//        {
//            sprite = new SpriteMap(GetPath("ImpactGrenade"), 11, 11);
//            graphic = sprite;
//            _editorName = "Impact Grenade";
//            collisionSize = new Vec2(9f, 14f);
//            collisionOffset = new Vec2(-5f, -7f);
//            center = new Vec2(16f, 16f);
//            _barrelOffsetTL = new Vec2(16f, 10f);
//        }

//        public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
//        {
//            if (with as Duck != null  && !_pin)
//            {
//                _timer = 0;
//                Level.Remove(this);
//            }
//            base.OnSoftImpact(with, from);
//        }

//        public override void Update()
//        {
//            sprite.frame = _pin ? 0 : 1;
//            base.Update();
//        }
//    }
//}