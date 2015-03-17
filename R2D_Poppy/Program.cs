using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Poppy
{
    internal static class Program
    {
        //GITHUB TEST
        /*public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;*/

        // can also be done this way, saves some lines.
        public static Spell Q, W, E, R;
        public static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _menu = new Menu("R2D_Poppy", "Poppy", true);

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            _menu.AddSubMenu(tsMenu);

            _menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_menu.SubMenu("Orbwalking"));

            _menu.AddSubMenu(new Menu("Combo", "Combo"));
            _menu.SubMenu("Combo").AddItem(new MenuItem("comboQ", "Use Q in Combo?").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("comboW", "Use W in combo?").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("comboE", "Use E in combo?").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("comboR", "Use R in combo?").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("Use Ignite", "Use Ignite in combo?").SetValue(true));

            _menu.AddSubMenu(new Menu("Harass", "Harass"));
            _menu.SubMenu("Harass").AddItem(new MenuItem("Use Q", "Use Q in harass?").SetValue(true));
            _menu.SubMenu("Harass").AddItem(new MenuItem("Use W", "Use W in harass?").SetValue(true));
            _menu.SubMenu("Harass").AddItem(new MenuItem("Use E", "Use E in harass?").SetValue(true));
            _menu.SubMenu("Harass").AddItem(new MenuItem("Use R", "Use R in harass?").SetValue(true));
            _menu.SubMenu("Harass").AddItem(new MenuItem("Mana Manager", "Harass MM").SetValue(new Slider(50, 1, 100)));

            _menu.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));
            _menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use Q", "Use Q in lane clear?").SetValue(true));
            _menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use W", "Use W in lane clear?").SetValue(true));
            _menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use E", "Use E in lane clear?").SetValue(true));
            _menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use R", "Use R in lane clear?").SetValue(true));
            _menu.SubMenu("Lane Clear")
                .AddItem(new MenuItem("Mana Manager", "Lane Clear MM").SetValue(new Slider(50, 1, 100)));

            _menu.AddSubMenu(new Menu("Last Hit", "Last Hit"));
            _menu.SubMenu("Last Hit").AddItem(new MenuItem("Use Q", "Use Q in last hit?").SetValue(true));
            _menu.SubMenu("Last Hit").AddItem(new MenuItem("Use W", "Use W in last hit?").SetValue(true));
            _menu.SubMenu("Last Hit").AddItem(new MenuItem("Use E", "Use E in last hit?").SetValue(true));
            _menu.SubMenu("Last Hit")
                .AddItem(new MenuItem("Mana Manager", "Last Hit MM").SetValue(new Slider(50, 1, 100)));

            _menu.SubMenu("Last Hit")
                .AddItem(
                    new MenuItem("Last Hit Key", "Last Hit Key Binding").SetValue(
                        new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _menu.SubMenu("Last Hit")
                .AddItem(
                    new MenuItem("Q Last Hit Toggle", "Last Hit Q Toggle").SetValue(
                        new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));

            _menu.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            _menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use Q", "Use Q in jungle clear?").SetValue(true));
            _menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use W", "Use W in jungle clear?").SetValue(true));
            _menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use E", "Use E in jungle clear?").SetValue(true));
            _menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use R", "Use R in jungle clear?").SetValue(true));
            _menu.SubMenu("Jungle Clear")
                .AddItem(new MenuItem("Mana Manager", "Jungle Clear MM").SetValue(new Slider(50, 1, 100)));

            _menu.AddSubMenu(new Menu("Kill Steal", "Kill Steal"));
            _menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use Q", "Use Q for kill steal?").SetValue(true));
            _menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use W", "Use W for kill steal?").SetValue(true));
            _menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use E", "Use E for kill steal?").SetValue(true));
            _menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use Ignite", "Use Ignite for kill steal?").SetValue(true));

            _menu.AddSubMenu(new Menu("Misc", "Misc"));
            _menu.SubMenu("Misc")
                .AddItem(
                    new MenuItem("Use Ignite", "Use Ignite in misc?").SetValue(
                        new StringList(new[] { "Combo", "Kill Steal" })));
            _menu.SubMenu("Misc").AddItem(new MenuItem("Use R first?", "Use R first?").SetValue(true));
            _menu.SubMenu("Misc").AddItem(new MenuItem("W on Gap Closer", "Use W on Gap Closer?").SetValue(true));

            _menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("Draw Q", "Draw Q?").SetValue(true));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("Draw W", "Draw W?").SetValue(true));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E?").SetValue(true));

            _menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Drawing.OnDraw += Drawing_OnDraw;

            _player = ObjectManager.Player;

        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {}

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical));
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical));
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    //LaneClear();
                    //JungleClear();
                    break;
            }
            //LastHit();

            var ksTarget =
                ObjectManager.Get<Obj_AI_Hero>().Where(t => t.IsValidTarget()).OrderBy(t => t.Health).FirstOrDefault();
            if (ksTarget != null)
                KillSteal(ksTarget);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu.Item("Draw Q").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Green);
            }
            if (_menu.Item("Draw W").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Red);
            }
            if (_menu.Item("Draw E").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Blue);
            }
        }

        private static void Harass(Obj_AI_Hero target)
        {

            if (_menu.Item("harassQ").GetValue<bool>() && Q.IsReady() &&
                target.Distance(_player) <= Orbwalking.GetRealAutoAttackRange(target)) //&&
                //(ObjectManager._player.ManaPercentage() > _menu.Item("manaH").GetValue<Slider>().Value))
            {
                Q.Cast();
            }
        }

        private static void Combo(Obj_AI_Hero target)
        {

            if (_menu.Item("comboQ").GetValue<bool>() && Q.IsReady() &&
                target.Distance(_player) <= Orbwalking.GetRealAutoAttackRange(target))
            {
                Q.Cast();
            }

            if (_menu.Item("comboW").GetValue<bool>() && _player.Distance(target.Position) < R.Range + 300)
                //&& _player.Distance(target.Position) > Orbwalking.GetRealAutoAttackRange(target) && W.IsReady())
            {
                W.Cast();
            }

            if (_menu.Item("comboE").GetValue<bool>() && E.IsReady() &&
                target.Distance(ObjectManager.Player.Position) <= E.Range)
            {
                if (E.GetDamage(target) > target.Health ||
                    ((E.GetDamage(target) + Q.GetDamage(target)) > target.Health && Q.IsReady()))
                {
                    E.CastOnUnit(target); //Btw I did this so it e´s and q for kill, is it right?
                }
                else
                {
                    WallStunTarget(target);
                }
            }

            if (_menu.Item("comboR").GetValue<bool>() && R.IsReady())
            {
                if (_player.HealthPercent <= 30 || _player.CountEnemiesInRange(1500) >= 2) //TODO: Added Checks
                {
                    return;
                }
                var priority = 0;
                Obj_AI_Hero selectedUnit = null;
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsEnemy && R.IsInRange(hero) && hero.IsValidTarget()))
                {
                    if (_menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value > priority)
                    {
                        priority = _menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value;
                        selectedUnit = enemy;
                        if (_menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value == 5)
                        {
                            break;
                        }
                    }
                }
                if (!selectedUnit.IsValidTarget())
                {
                    return;
                }
                R.CastOnUnit(selectedUnit);
            }
        }

        private static void LastHit()
        {
            var keyActive = _menu.Item("Last Hit Key Binding").GetValue<KeyBind>().Active;
            if (!keyActive)
                return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (_menu.Item("Use Q").GetValue<bool>() && Q.IsReady() &&
                _menu.Item("Last Hit Q Toggle").GetValue<KeyBind>().Active)
            {
                foreach (
                    var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (minion.IsValidTarget())
                    {
                        Q.Cast();
                    }
                }
        }
    }

    private static void KillSteal(Obj_AI_Hero Target)
        {
            var Champions = ObjectManager.Get<Obj_AI_Hero>();
            if (_menu.Item("ksQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.Q)))
                {
                    if (champ.IsValidTarget())
                    {
                        Q.CastOnUnit(champ);
                    }
                }

            }
        }

        /*  private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(_player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (minion == null)
                return;


        } */

        private static bool UnderTower(this Vector3 pos)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(
                        tower =>
                            tower.IsValid && tower.IsAlly && tower.Health > 0 && pos.Distance(tower.Position) < 1000);
        }

        private static void WallStunTarget(Obj_AI_Hero target)
        {
            var pushbackDist = 100;
            var checkNumber = (pushbackDist / _menu.Item("checkNO").GetValue<Slider>().Value);
                //Divides pushback dist by number of checks
            var predictedPosition = Prediction.GetPrediction(target, 0.5f);
                //Predicteded position of the target in the cast time
            for (var i = 1; i <= _menu.Item("checkNO").GetValue<Slider>().Value; i++)
            {
                if (predictedPosition.UnitPosition.Extend(_player.Position, -(i * checkNumber)).IsWall() ||
                    predictedPosition.UnitPosition.Extend(_player.Position, -(i * checkNumber)).UnderTower())
                {
                    if (_player.HealthPercent <= 30 || _player.CountEnemiesInRange(1500) >= 2 || R.IsReady())
                    {
                        R.CastOnUnit(target);
                        E.CastOnUnit(target);
                        break;
                    }
                    E.CastOnUnit(target);
                    break;
                }
            }
        }
    }
}