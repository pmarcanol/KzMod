using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using DuckGame;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class DisguiseBox : Equipment
    {
        private bool _isDisguised = false;
        private Sprite _disguiseSprite;
        private Thing _commonPropPrototype;
        private float _quackCooldown = 0f;
        private const float QuackCooldownTime = 0.5f;

        public SpriteMap _originalSprite;
        public SpriteMap _originalSpriteArms;
        public SpriteMap _originalSpriteQuack;
        public SpriteMap _originalSpriteControlled;

        public Color _originalColor;
        public Color _originalHatColor;
        private Dictionary<Equipment, Color> _equipmentOriginalColors = new Dictionary<Equipment, Color>();

        public DisguiseBox(float xpos, float ypos) : base(xpos, ypos)
        {
            _editorName = "Disguise Box";
            graphic = new SpriteMap(Mod.GetPath<KzMod>("disguiseBox"), 16, 16);

            this.scale = new Vec2(0.8f, 0.8f);
            this.position.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            this._equippedDepth = -16;

            this.center = new Vec2(8f, 8f);
            this.collisionOffset = new Vec2(-7f, -7f);
            this.collisionSize = new Vec2(13f, 13f);
            this._offset = new Vec2(-3f, 3f);
            this._wearOffset = new Vec2(-4.5f, -1f);
        }

        public override void Update()
        {
            base.Update();

            if (_quackCooldown > 0f)
            {
                _quackCooldown -= Maths.IncFrameTimer();
            }

            DropStuffWhileDisguised();

            if (_equippedDuck != null
                && _equippedDuck.inputProfile.Pressed(Triggers.Quack)
                && _quackCooldown <= 0f)
            {
                _quackCooldown = QuackCooldownTime;

                if (!_isDisguised)
                {
                    EnterDisguiseState();

                }
                else
                {
                    ExitDisguiseState();
                }
            }
        }

        private void DropStuffWhileDisguised()
        {
            if (_isDisguised
                && _equippedDuck != null
                )
            {
                DropHoldable();
                DropSpikeHelm();
            }
        }

        private void DropHoldable()
        {
            if (_equippedDuck.holdObject != null)
            {
                Holdable holdable = _equippedDuck.holdObject;
                _equippedDuck.holdObject = null;
                holdable.hSpeed = 0f;
                holdable.vSpeed = 0f;
                holdable.enablePhysics = true;
            }
        }

        private void DropSpikeHelm()
        {
            foreach (Equipment equipment in _equippedDuck._equipment.ToList())
            {
                if (equipment is SpikeHelm spikeHelm)
                {
                    _equippedDuck.Unequip(spikeHelm);

                    spikeHelm.position = _equippedDuck.position + new Vec2(0f, 5f);
                    spikeHelm.hSpeed = 1f;
                    spikeHelm.vSpeed = -2f;

                    spikeHelm.enablePhysics = true;

                    Level.Add(spikeHelm);

                    break;
                }
            }
        }

        private void EnterDisguiseState()
        {
            _commonPropPrototype = GetMostCommonSpecificPropPrototype();

            if (_commonPropPrototype == null)
            {
                _commonPropPrototype = GetRandomValidPropPrototype();
                if (_commonPropPrototype == null)
                {
                    return;
                }
            }

            if (_commonPropPrototype.graphic == null)
            {
                return;
            }

            _disguiseSprite = new SpriteMap(
                _commonPropPrototype.graphic.texture,
                _commonPropPrototype.graphic.width,
                _commonPropPrototype.graphic.height
            );
            _disguiseSprite.CenterOrigin();

            if (_equippedDuck == null)
            {
                return;
            }

            _originalSprite = _equippedDuck._sprite.CloneMap();
            _originalSpriteArms = _equippedDuck._spriteArms.CloneMap();
            _originalSpriteControlled = _equippedDuck._spriteControlled.CloneMap();
            _originalSpriteQuack = _equippedDuck._spriteQuack.CloneMap();
            _originalColor = _equippedDuck._sprite.color;

            if (_equippedDuck.hat != null)
            {
                _originalHatColor = _equippedDuck.hat.graphic.color;
            }

            if (_equippedDuck._equipment != null)
            {
                foreach (Equipment equipment in _equippedDuck._equipment)
                {
                    if (!_equipmentOriginalColors.ContainsKey(equipment))
                    {
                        if (equipment.graphic != null)
                        {
                            _equipmentOriginalColors[equipment] = equipment.graphic.color;
                        }
                    }
                }
            }

            _isDisguised = true;
        }

        private void ExitDisguiseState()
        {
            if (_originalSprite == null)
            {
                return;
            }

            _equippedDuck._sprite = _originalSprite;
            _equippedDuck._spriteArms = _originalSpriteArms;
            _equippedDuck._spriteControlled = _originalSpriteControlled;
            _equippedDuck._spriteQuack = _originalSpriteQuack;
            _isDisguised = false;

            foreach (var kvp in _equipmentOriginalColors)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.graphic.color = kvp.Value;
                }
            }
            _equipmentOriginalColors.Clear();
        }


        private Thing GetMostCommonSpecificPropPrototype()
        {
            Dictionary<string, int> propCounts = new Dictionary<string, int>();
            Dictionary<string, Thing> propPrototypes = new Dictionary<string, Thing>();
            HashSet<string> validProps = new HashSet<string> { "Crate", "Rock", "IceBlock", "Present" };

            foreach (Thing thing in Level.current.things)
            {
                if (thing is PhysicsObject prop && validProps.Contains(prop.GetType().Name))
                {
                    string propType = prop.GetType().Name;
                    if (!propCounts.ContainsKey(propType))
                    {
                        propCounts[propType] = 0;
                        propPrototypes[propType] = prop;
                    }
                    propCounts[propType]++;
                }
            }

            if (propCounts.Count == 0)
                return null;

            string mostCommonPropType = propCounts.OrderByDescending(kvp => kvp.Value).First().Key;
            return propPrototypes[mostCommonPropType];
        }

        private Thing GetRandomValidPropPrototype()
        {
            string[] propTypes = { "Crate", "Rock", "IceBlock", "Present" };
            string randomPropType = propTypes[new Random().Next(propTypes.Length)];

            switch (randomPropType)
            {
                case "Crate":
                    return new Crate(0, 0) { enablePhysics = false };
                case "Rock":
                    return new Rock(0, 0) { enablePhysics = false };
                case "IceBlock":
                    return new IceBlock(0, 0) { enablePhysics = false };
                case "Present":
                    return new Present(0, 0) { enablePhysics = false };

                default:
                    return null;
            }
        }

        public override void Draw()
        {
            if (_isDisguised && _equippedDuck != null)
            {
                float crouchValue = 7f;
                if (_commonPropPrototype != null
                    && (_commonPropPrototype.editorName == "Present"
                        || _commonPropPrototype.editorName == "Rock"
                    )
                )
                {
                    crouchValue = 8f;
                }
                float crouchAdjustment = _equippedDuck.crouch ? crouchValue : 4f;
                Vec2 adjustedPosition = _equippedDuck.position + new Vec2(0f, crouchAdjustment);

                _disguiseSprite.depth = _equippedDuck.depth + 15;
                _disguiseSprite.angle = _equippedDuck.angle;
                _disguiseSprite.scale = _equippedDuck.scale;
                _disguiseSprite.flipH = this.offDir <= (sbyte)0;
                _disguiseSprite.position = adjustedPosition;
                _disguiseSprite.Draw();

                if (_equippedDuck.crouch)
                {
                    DrawHideSprites();
                }
                else
                {
                    DrawShowSprites();
                }
            }
            else
            {
                base.Draw();
            }
        }

        private void DrawHideSprites()
        {
            SetSpritesColor(Color.Transparent);

            if (_equippedDuck.hat != null)
            {
                _equippedDuck.hat.graphic.color = Color.Transparent;
            }

            foreach (Equipment equipment in _equippedDuck._equipment)
            {
                if (equipment is ChestPlate chestPlate)
                {
                    SetSpriteOverColor(chestPlate, Color.Transparent);
                }
                if (equipment.graphic != null
                    && !(equipment is SpikeHelm)
                    )
                {
                    equipment.graphic.color = Color.Transparent; // Hide equipment
                }
            }
        }

        private void DrawShowSprites()
        {
            SetSpritesColor(_originalColor);

            if (_equippedDuck.hat != null)
            {
                _equippedDuck.hat.graphic.color = _originalHatColor;
            }

            foreach (Equipment equipment in _equippedDuck._equipment)
            {
                if (_equipmentOriginalColors.TryGetValue(equipment, out Color originalColor))
                {
                    if (equipment is ChestPlate chestPlate)
                    {
                        SetSpriteOverColor(chestPlate, originalColor);
                    }
                    if (equipment.graphic != null)
                    {
                        equipment.graphic.color = originalColor; // Restore original color
                    }
                }
            }
        }

        private void SetSpritesColor(Color color)
        {
            _equippedDuck._sprite.color = color;
            _equippedDuck._spriteArms.color = color;
            _equippedDuck._spriteQuack.color = color;
            _equippedDuck._spriteControlled.color = color;
            _equippedDuck.graphic.color = color;
        }

        private void SetSpriteOverColor(object chestplate, Color color)
        {
            FieldInfo spriteOverField = chestplate.GetType().GetField("_spriteOver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (spriteOverField != null)
            {
                var spriteOver = spriteOverField.GetValue(chestplate) as SpriteMap;
                if (spriteOver != null)
                {
                    spriteOver.color = color;
                }
            }
        }
    }
}
