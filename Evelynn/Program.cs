﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Evelynn
{
    internal class Program
    {
        public const string ChampionName = "Evelynn";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu Config;

        //Player
        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 500f);
            W = new Spell(SpellSlot.W, Q.Range);
            E = new Spell(SpellSlot.E, 225f + 2 * 65f);
            R = new Spell(SpellSlot.R, 650f);

            R.SetSkillshot(0.25f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind("32".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));
            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(true));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E range").SetValue(true));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R range").SetValue(true));

            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("QRange").GetValue<bool>())
            {
                Drawing.DrawCircle(_player.Position, Q.Range, Color.Red);
            }
            if (Config.Item("ERange").GetValue<bool>())
            {
                Drawing.DrawCircle(_player.Position, E.Range, Color.Green);
            }
            if (Config.Item("RRange").GetValue<bool>())
            {
                Drawing.DrawCircle(_player.Position, R.Range, Color.Blue);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(40)) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
                return;
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                JungleFarm();
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                if (ObjectManager.Get<Obj_AI_Hero>()
                .Count(
                    hero =>
                        hero.IsValidTarget() &&
                        hero.Distance(new Vector2(args.EndPosition.X, args.EndPosition.Y)) <= R.Range) == 0)
                    args.Process = false;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.True);

            if (target != null)
            {
                if (Config.Item("UseQCombo").GetValue<bool>())
                    Q.Cast();

                if (Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() &&
                    _player.HasBuffOfType(BuffType.Slow))
                    W.Cast();

                if (Config.Item("UseECombo").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(target);

                if (Config.Item("UseRCombo").GetValue<bool>() && R.IsReady() && GetComboDamage(target) > target.Health)
                    R.Cast(target, false, true);
            }
        }

        private static void JungleFarm()
        {
            var mobs =
                MinionManager.GetMinions(500, MinionTypes.All, MinionTeam.Neutral).OrderBy(m => m.MaxHealth);
            foreach (var minion in mobs.FindAll(minion => minion.IsValidTarget(Q.Range)))
            if (mobs.FirstOrDefault().IsValidTarget())
            {
                if (Config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                    Q.Cast();

                if (Config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(minion);
            }
        }
        
        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(_player.ServerPosition, Q.Range);

            foreach (var minion in minions.FindAll(minion => minion.IsValidTarget(Q.Range)))
            {
                if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady())
                    Q.Cast();

                if (Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(minion);
            }
        }

        private static float GetComboDamage(Obj_AI_Base target)
        {
            float comboDamage = 0;

            if ((_player.Spellbook.GetSpell(SpellSlot.Q).Level) > 0)
                comboDamage += Q.GetDamage(target) * 3;
            if (E.IsReady())
                comboDamage += E.GetDamage(target);
            if (R.IsReady())
                comboDamage += R.GetDamage(target);

            return comboDamage;
        }
    }
}