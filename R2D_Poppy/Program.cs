﻿using System;
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
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("R2D_Poppy", "Poppy", true);

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("comboQ", "Use Q in Combo?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("comboW", "Use W in combo?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("comboE", "Use E in combo?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("comboR", "Use R in combo?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use Ignite", "Use Ignite in combo?").SetValue(true));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use Q", "Use Q in harass?").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use W", "Use W in harass?").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use E", "Use E in harass?").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use R", "Use R in harass?").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Mana Manager", "Harass MM").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));
            Menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use Q", "Use Q in lane clear?").SetValue(true));
            Menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use W", "Use W in lane clear?").SetValue(true));
            Menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use E", "Use E in lane clear?").SetValue(true));
            Menu.SubMenu("Lane Clear").AddItem(new MenuItem("Use R", "Use R in lane clear?").SetValue(true));
            Menu.SubMenu("Lane Clear")
                .AddItem(new MenuItem("Mana Manager", "Lane Clear MM").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Last Hit", "Last Hit"));
            Menu.SubMenu("Last Hit").AddItem(new MenuItem("Use Q", "Use Q in last hit?").SetValue(true));
            Menu.SubMenu("Last Hit").AddItem(new MenuItem("Use W", "Use W in last hit?").SetValue(true));
            Menu.SubMenu("Last Hit").AddItem(new MenuItem("Use E", "Use E in last hit?").SetValue(true));
            Menu.SubMenu("Last Hit")
                .AddItem(new MenuItem("Mana Manager", "Last Hit MM").SetValue(new Slider(50, 1, 100)));

            Menu.SubMenu("Last Hit")
                .AddItem(
                    new MenuItem("Last Hit Key", "Last Hit Key Binding").SetValue(
                        new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Last Hit")
                .AddItem(
                    new MenuItem("Q Last Hit Toggle", "Last Hit Q Toggle").SetValue(
                        new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));

            Menu.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            Menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use Q", "Use Q in jungle clear?").SetValue(true));
            Menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use W", "Use W in jungle clear?").SetValue(true));
            Menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use E", "Use E in jungle clear?").SetValue(true));
            Menu.SubMenu("Jungle Clear").AddItem(new MenuItem("Use R", "Use R in jungle clear?").SetValue(true));
            Menu.SubMenu("Jungle Clear")
                .AddItem(new MenuItem("Mana Manager", "Jungle Clear MM").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Kill Steal", "Kill Steal"));
            Menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use Q", "Use Q for kill steal?").SetValue(true));
            Menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use W", "Use W for kill steal?").SetValue(true));
            Menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use E", "Use E for kill steal?").SetValue(true));
            Menu.SubMenu("Kill Steal").AddItem(new MenuItem("Use Ignite", "Use Ignite for kill steal?").SetValue(true));

            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc")
                .AddItem(
                    new MenuItem("Use Ignite", "Use Ignite in misc?").SetValue(
                        new StringList(new[] { "Combo", "Kill Steal" })));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Use R first?", "Use R first?").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("W on Gap Closer", "Use W on Gap Closer?").SetValue(true));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw Q", "Draw Q?").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw W", "Draw W?").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E?").SetValue(true));

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {}

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
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
            if (Menu.Item("Draw Q").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Green);
            }
            if (Menu.Item("Draw W").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Red);
            }
            if (Menu.Item("Draw E").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Blue);
            }
        }

        private static void Harass(Obj_AI_Hero target)
        {

            if (Menu.Item("harassQ").GetValue<bool>() && Q.IsReady() &&
                target.Distance(Player) <= Orbwalking.GetRealAutoAttackRange(target)) //&&
                //(ObjectManager.Player.ManaPercentage() > Menu.Item("manaH").GetValue<Slider>().Value))
            {
                Q.Cast();
            }
        }

        private static void Combo(Obj_AI_Hero target)
        {

            if (Menu.Item("comboQ").GetValue<bool>() && Q.IsReady() &&
                target.Distance(Player) <= Orbwalking.GetRealAutoAttackRange(target))
            {
                Q.Cast();
            }

            if (Menu.Item("comboW").GetValue<bool>() && Player.Distance(target.Position) < R.Range + 300)
                //&& Player.Distance(target.Position) > Orbwalking.GetRealAutoAttackRange(target) && W.IsReady())
            {
                W.Cast();
            }

            if (Menu.Item("comboE").GetValue<bool>() && E.IsReady() &&
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

            if (Menu.Item("comboR").GetValue<bool>() && R.IsReady())
            {
                if (Player.HealthPercent <= 30 || Player.CountEnemiesInRange(1500) >= 2) //TODO: Added Checks
                {
                    return;
                }
                var priority = 0;
                Obj_AI_Hero selectedUnit = null;
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsEnemy && R.IsInRange(hero) && hero.IsValidTarget()))
                {
                    if (Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value > priority)
                    {
                        priority = Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value;
                        selectedUnit = enemy;
                        if (Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value == 5)
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
            var keyActive = Menu.Item("Last Hit Key Binding").GetValue<KeyBind>().Active;
            if (!keyActive)
                return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (Menu.Item("Use Q").GetValue<bool>() && Q.IsReady() &&
                Menu.Item("Last Hit Q Toggle").GetValue<KeyBind>().Active)
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
            if (Menu.Item("ksQ").GetValue<bool>() && Q.IsReady())
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
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
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
            var checkNumber = (pushbackDist / Menu.Item("checkNO").GetValue<Slider>().Value);
                //Divides pushback dist by number of checks
            var predictedPosition = Prediction.GetPrediction(target, 0.5f);
                //Predicteded position of the target in the cast time
            for (var i = 1; i <= Menu.Item("checkNO").GetValue<Slider>().Value; i++)
            {
                if (predictedPosition.UnitPosition.Extend(Player.Position, -(i * checkNumber)).IsWall() ||
                    predictedPosition.UnitPosition.Extend(Player.Position, -(i * checkNumber)).UnderTower())
                {
                    if (Player.HealthPercent <= 30 || Player.CountEnemiesInRange(1500) >= 2 || R.IsReady())
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