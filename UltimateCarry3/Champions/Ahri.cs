using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry.Champions
{
    internal class Ahri : Champion
    {
        private const float _spellQSpeed = 2500;
        private const float _spellQSpeedMin = 400;
        private const float _spellQFarmSpeed = 1600;
        private readonly Menu _menu;
        private readonly Spell _spellE;
        private readonly Spell _spellQ;
        private readonly Spell _spellR;
        private readonly Spell _spellW;
        private Items.Item _itemDFG;

        public Ahri()
        {
            _menu = Program.Menu;

            var comboMenu = _menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboROnlyUserInitiate", "Use R only if user initiated").SetValue(false));

            var harassMenu = _menu.AddSubMenu(new Menu("Harass", "Harass"));
            harassMenu.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassPercent", "Skills until Mana %").SetValue(new Slider(20)));

            var farmMenu = _menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            farmMenu.AddItem(new MenuItem("farmQ", "Use Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmW", "Use W").SetValue(false));
            farmMenu.AddItem(new MenuItem("farmPercent", "Skills until Mana %").SetValue(new Slider(20)));
            farmMenu.AddItem(new MenuItem("farmStartAtLevel", "Only AA until Level").SetValue(new Slider(8, 1, 18)));

            var drawMenu = _menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            drawMenu.AddItem(
                new MenuItem("drawQE", "Draw Q, E range").SetValue(new Circle(true, Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(
                new MenuItem("drawW", "Draw W range").SetValue(new Circle(false, Color.FromArgb(125, 0, 0, 255))));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw Combo Damage").SetValue(true);
                //copied from esk0r Syndra
            drawMenu.AddItem(dmgAfterComboItem);


            _itemDFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            _spellQ = new Spell(SpellSlot.Q, 990);
            _spellW = new Spell(SpellSlot.W, 795 - 95);
            _spellE = new Spell(SpellSlot.E, 1000 - 10);
            _spellR = new Spell(SpellSlot.R, 1000 - 100);

            _spellQ.SetSkillshot(.215f, 100, 1600f, false, SkillshotType.SkillshotLine);
            _spellW.SetSkillshot(.71f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotLine);
            _spellE.SetSkillshot(.23f, 60, 1500f, true, SkillshotType.SkillshotLine);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;

            PluginLoaded();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            switch (Program.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    break;
            }
        }

        private void Harass()
        {
            if (_menu.Item("harassE").GetValue<bool>() &&
                GetManaPercent() >= _menu.Item("harassPercent").GetValue<Slider>().Value)
            {
                CastE();
            }

            if (_menu.Item("harassQ").GetValue<bool>() &&
                GetManaPercent() >= _menu.Item("harassPercent").GetValue<Slider>().Value)
            {
                CastQ();
            }
        }

        private void LaneClear()
        {
            _spellQ.Speed = _spellQFarmSpeed;
            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);

            var jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            if ((_menu.Item("farmQ").GetValue<bool>() &&
                 GetManaPercent() >= _menu.Item("farmPercent").GetValue<Slider>().Value &&
                 ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) || jungleMobs)
            {
                var farmLocation = _spellQ.GetLineFarmLocation(minions);

                if (farmLocation.Position.IsValid())
                {
                    if (farmLocation.MinionsHit >= 2 || jungleMobs)
                    {
                        CastQ(farmLocation.Position);
                    }
                }
            }

            minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count() > 0)
            {
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                if ((_menu.Item("farmW").GetValue<bool>() &&
                     GetManaPercent() >= _menu.Item("farmPercent").GetValue<Slider>().Value &&
                     ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) ||
                    jungleMobs)
                {
                    CastW(true);
                }
            }
        }

        private void CastE()
        {
            if (!_spellE.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_spellE.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                _spellE.CastIfHitchanceEquals(target, HitChance.High, Packets());
            }
        }

        private void CastQ()
        {
            if (!_spellQ.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_spellQ.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                var predictedPos = Prediction.GetPrediction(target, _spellQ.Delay).UnitPosition;
                    //correct pos currently not possible with spell acceleration
                _spellQ.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos));
                _spellQ.CastIfHitchanceEquals(target, HitChance.High, Packets());
            }
        }

        private void CastQ(Vector2 pos)
        {
            if (!_spellQ.IsReady())
            {
                return;
            }

            _spellQ.Cast(pos, Packets());
        }

        private void CastW(bool ignoreTargetCheck = false)
        {
            if (!_spellW.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_spellW.Range, TargetSelector.DamageType.Magical);

            if (target != null || ignoreTargetCheck)
            {
                _spellW.Cast(ObjectManager.Player.Position, Packets());
            }
        }

        private void Combo()
        {
            if (_menu.Item("comboE").GetValue<bool>())
            {
                CastE();
            }

            if (_menu.Item("comboQ").GetValue<bool>())
            {
                CastQ();
            }

            if (_menu.Item("comboW").GetValue<bool>())
            {
                CastW();
            }

            if (_menu.Item("comboR").GetValue<bool>() && _spellR.IsReady())
            {
                if (OkToUlt())
                {
                    _spellR.Cast(Game.CursorPos, Packets());
                }
            }
        }

        private List<SpellSlot> GetSpellCombo()
        {
            var spellCombo = new List<SpellSlot>();

            if (_spellQ.IsReady())
            {
                spellCombo.Add(SpellSlot.Q);
            }
            if (_spellW.IsReady())
            {
                spellCombo.Add(SpellSlot.W);
            }
            if (_spellE.IsReady())
            {
                spellCombo.Add(SpellSlot.E);
            }
            if (_spellR.IsReady())
            {
                spellCombo.Add(SpellSlot.R);
            }
            return spellCombo;
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = (float) ObjectManager.Player.GetComboDamage(target, GetSpellCombo());

            return (float) (comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
        }

        private bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player.ServerPosition) < 500))
                //any enemies around me?
            {
                return true;
            }

            var mousePos = Game.CursorPos;

            var enemiesNearMouse =
                Program.Helper.EnemyTeam.Where(
                    x => x.Distance(ObjectManager.Player.ServerPosition) < _spellR.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                {
                    return true;
                }

                var enoughMana = ObjectManager.Player.Mana >
                                 ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                                 ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ManaCost +
                                 ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (_menu.Item("comboROnlyUserInitiate").GetValue<bool>() || !(_spellQ.IsReady() && _spellE.IsReady()) ||
                    !enoughMana)
                    //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                {
                    return false;
                }

                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650);
                    //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    var enemy = enemiesNearMouse.FirstOrDefault();

                    var underTower = enemy.UnderTurret();

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1);
                        //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                var lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1);
                    //dont count low health enemies

                var totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 ||
                       ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
            }

            return false;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQE = _menu.Item("drawQE").GetValue<Circle>();
                var drawW = _menu.Item("drawW").GetValue<Circle>();

                if (drawQE.Active)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQE.Color);
                }

                if (drawW.Active)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spellW.Range, drawW.Color);
                }
            }
        }

        private float GetDynamicQSpeed(float distance)
        {
            var accelerationrate = _spellQ.Range / (_spellQSpeedMin - _spellQSpeed); // = -0.476...
            return _spellQSpeed + accelerationrate * distance;
        }

        private bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble", true);
        }

        private int GetRStacks()
        {
            var tumble = ObjectManager.Player.Buffs.FirstOrDefault(x => x.Name == "AhriTumble");
            return tumble != null ? tumble.Count : 0;
        }
    }
}