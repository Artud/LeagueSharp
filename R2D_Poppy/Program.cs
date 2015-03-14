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
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 900);
 
            var champMenu = new Menu("Plugin", ObjectManager.Player.BaseSkinName + "_Plugin");
            {
                var comboMenu = new Menu("Combo", "Combo");
                {
                    comboMenu.AddItem(new MenuItem("comboQ", "Usar Q", true));
                    comboMenu.AddItem(new MenuItem("comboW", "Usar W", true));
                    comboMenu.AddItem(new MenuItem("comboE", "Usar E", true));
                    comboMenu.AddItem(new MenuItem("comboR", "Usar R", true));
                    var RMenu = comboMenu.AddSubMenu(new Menu("R Priority", "rpri"));
                    foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy)) // gets every enemy and does the check for each of them.
                    {
                        RMenu.AddItem(new MenuItem(target.ChampionName + "prior", target.ChampionName).SetValue(new Slider(1,1,5))); // creates a slider for each of the enemies, you can have them arrange the priority for each of them.
                    }
                    RMenu.AddItem(new MenuItem("sdsdsa", "1 is lowest"));
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("Harass", "Harass");
                {
                    harassMenu.AddItem(new MenuItem("harassQ", "Usar Q", true));
                    champMenu.AddSubMenu(harassMenu);
                }
                var clearMenu = new Menu("Clear", "Clear");
                {
 
                    champMenu.AddSubMenu(clearMenu);
                }
                var lastHitMenu = new Menu("Last Hit", "LastHit");
                {
                    champMenu.AddSubMenu(lastHitMenu);
                }
                var fleeMenu = new Menu("Flee", "Flee");
                {
                    champMenu.AddSubMenu(fleeMenu);
                }
                var miscMenu = new Menu("Misc", "Misc");
                {
                    miscMenu.AddItem(new MenuItem("checkNO", "Number of E checks")).SetValue(new Slider(10, 1, 30)); // this is the number of checks that occur when casting E, the more tha laggier but more precise
                    var killStealMenu = new Menu("Kill Steal", "KillSteal");
                    {
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    champMenu.AddSubMenu(miscMenu);
                }
                var drawMenu = new Menu("Draw", "Draw");
                {
                    drawMenu.AddItem(new MenuItem("drawE", "Draw E", true));
                    drawMenu.AddItem(new MenuItem("drawR", "Draw R", true));
                    champMenu.AddSubMenu(drawMenu);
                }
                Menu.AddSubMenu(champMenu);
            }
 
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
               
        }
      
 
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }
 
        private static void Drawing_OnDraw(EventArgs args) {
            if (Menu.Item("drawE").GetValue<bool>())
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Green);
            }
            if (Menu.Item("drawR").GetValue<bool>())
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Green);
            }
            // draw handler
        }

        private static void Harass()
        {
            if (Menu.Item("harassQ").GetValue<bool>() && Q.IsReady() &&
                target.Distance(Player) <= Orbwalking.GetRealAutoAttackRange(target))
            {
                Q.Cast();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
 
            if (Menu.Item("comboQ").GetValue<bool>() && Q.IsReady() && target.Distance(Player) <= Orbwalking.GetRealAutoAttackRange(target))
            {
                Q.Cast();
            }
 
            if (Menu.Item("comboW").GetValue<bool>() && Player.Distance(target.Position) < R.Range + 300) //&& Player.Distance(target.Position) > Orbwalking.GetRealAutoAttackRange(target) && W.IsReady())
            {
                W.Cast();
            }
 
            if (Menu.Item("comboE").GetValue<bool>() && E.IsReady() && target.Distance(ObjectManager.Player.Position) <= E.Range)
            {
                if (E.GetDamage(target) > target.Health || ((E.GetDamage(target) + Q.GetDamage(target)) > target.Health && Q.IsReady()) )
                {
                    E.CastOnUnit(target);//Btw I did this so it e´s and q for kill, is it right?
                }
                else
                {
                    WallStunTarget(target);
                }
            }

            if (Menu.Item("comboR").GetValue<bool>() && R.IsReady())
            {
                if (Player.HealthPercent <= 30 || Player.CountEnemiesInRange(1500) >= 2) //TODO: Added Checks
                    return;
                int priority = 0;
                Obj_AI_Hero selectedUnit = null;
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && R.IsInRange(hero) && hero.IsValidTarget()))
                {
                    if (Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value > priority)
                    {
                        priority = Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value;
                        selectedUnit = enemy;
                        if (Menu.Item(enemy.ChampionName + "prior").GetValue<Slider>().Value == 5)
                            break;
                    }
                }
                if (!selectedUnit.IsValidTarget())
                    return;
                R.CastOnUnit(selectedUnit);
            }
 
        }

        public static bool UnderTower(this Vector3 pos)
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
            var checkNumber = (pushbackDist / Menu.Item("checkNO").GetValue<Slider>().Value); //Divides pushback dist by number of checks
            var predictedPosition = Prediction.GetPrediction(target, 0.5f);//Predicteded position of the target in the cast time
            for (int i = 1; i <= Menu.Item("checkNO").GetValue<Slider>().Value; i++)
            {
                if (predictedPosition.UnitPosition.Extend(Player.Position, -(i * checkNumber)).IsWall() || predictedPosition.UnitPosition.Extend(Player.Position, -(i * checkNumber)).UnderTower())
                {
                    if (Player.HealthPercent <= 30 || Player.CountEnemiesInRange(1500) >= 2 || R.IsReady())
                    {
                        R.CastOnUnit(target);
                        E.CastOnUnit(target);
                        break;
                    }
                    else
                    {
                        E.CastOnUnit(target);   //So now it throws enemy under tower right, ok does the wallcheck also checks for tower stun? Nice :) Btw are you sure this thing I coded is right? Oh yea I was gonna ask, Where do I add it though? Mind showing me, Sure
                        break;
                    }
                   
                }

            }
        }
    }
}