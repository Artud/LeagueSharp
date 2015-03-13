using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Poppy
{
    internal class Program
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
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("Harass", "Harass");
                {
                    harassMenu.AddItem(new MenuItem("harassQ", "Usar Q", true));
                    harassMenu.AddItem(new MenuItem("harassW", "Usar W", true));
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
                    var killStealMenu = new Menu("Kill Steal", "KillSteal");
                    {
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    champMenu.AddSubMenu(miscMenu);
                }
                var drawMenu = new Menu("Draw", "Draw");
                {
                    drawMenu.AddItem(new MenuItem("drawQ", "Draw Q", true));
                    drawMenu.AddItem(new MenuItem("drawW", "Draw W", true));
                    drawMenu.AddItem(new MenuItem("drawR", "Draw R", true));
                    champMenu.AddSubMenu(drawMenu);
                }
                Menu.AddSubMenu(champMenu);
            }
 
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
        }
      
 
        private static void Game_OnGameUpdate(EventArgs args)
        {
            /*if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }*/
 
            // I think that this is a better way to check combo's
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
            // draw handler
        }
 
        private static void Harass()
        {
            //harass handler
        } 
 
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
 
            if (Menu.Item("comboQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target))
            {
                Q.CastIfHitchanceEquals(target, HitChance.Medium, false);
            }
 
            // since you initialized it as Player it should be Player and not player
            if (Menu.Item("comboW").GetValue<bool>() && Player.Distance(target.Position)<E.Range && W.IsReady())
            {
                W.Cast();
            }
 
            if (Menu.Item("comboE").GetValue<bool>() && E.IsReady() && R.IsReady() && target.Distance(ObjectManager.Player.Position) > R.Range)
            {
                //ToDo
            }
 
            if (Menu.Item("comboR").GetValue<bool>() && R.IsReady() && R.IsInRange(target))
            {
                //ToDo
            }
 
        }
    }
}