using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry.Champions
{
    internal class Thresh : Champion
    {
        public const int QFollowTime = 3000;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell E;
        public static Spell Q;
        public static int QFollowTick;
        public static Spell R;
        public static Spell W;

        public Thresh()
        {
            LoadMenu();
            LoadSpells();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            PluginLoaded();
        }

        public Obj_AI_Hero LastQTarget { get; set; }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight_follow", "Follow Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight")
                .AddItem(new MenuItem("useW_TeamFight_shield", "W for Shield").SetValue(true));
            Program.Menu.SubMenu("TeamFight")
                .AddItem(new MenuItem("useW_TeamFight_enagage", "W for Engage").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "E to me").SetValue(true));
            Program.Menu.SubMenu("TeamFight")
                .AddItem(new MenuItem("useR_TeamFight", "Use R if Hit").SetValue(new Slider(2, 5, 0)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass_safe", "W for SafeFriend").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "E away").SetValue(true));
            AddManaManager("Harass", 50);

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));
            AddManaManager("LaneClear", 20);

            Program.Menu.AddSubMenu(new Menu("SupportMode", "SupportMode"));
            Program.Menu.SubMenu("SupportMode").AddItem(new MenuItem("hitMinions", "Hit Minions").SetValue(false));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useQ_Interupt", "Q Interrupt").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useE_Interupt", "E Interrupt").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useE_GapCloser", "E for gapcloser").SetValue(false));

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

            Program.Menu.AddItem(new MenuItem("WQMouse" , "W and then Q to mouse").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
        }
        

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1025);
            Q.SetSkillshot(0.5f, 50f, 1900, true, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 950);

            E = new Spell(SpellSlot.E, 400);

            R = new Spell(SpellSlot.R, 400);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
            {
                return;
            }

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
            {
                if (Q.Level > 0)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
                }
            }

            if (Program.Menu.Item("Draw_W").GetValue<bool>())
            {
                if (W.Level > 0)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
                }
            }

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
            {
                if (E.Level > 0)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
                }
            }

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
            {
                if (R.Level > 0)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
                }
            }
        }

        public void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.IsReady() && Program.Menu.Item("useE_GapCloser").GetValue<bool>() && gapcloser.Sender.IsValidTarget())
            {
                E.Cast(gapcloser.Start);
            }
        }

        private void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (target.IsAlly)
            {
                return;
            }
            if (Program.Menu.Item("useE_Interupt").GetValue<bool>())
            {
                if (E.IsReady())
                {
                    if (target.IsValidTarget(W.Range))
                    {
                        E.Cast(target);
                        return;
                    }
                }
            }
            if (!Program.Menu.Item("useQ_Interupt").GetValue<bool>() || !target.IsValidTarget(Q.Range) ||
                Q.GetPrediction(target).Hitchance < HitChance.Low || Environment.TickCount - QFollowTick < QFollowTime ||
                !Q.IsReady())
            {
                return;
            }
            QFollowTick = Environment.TickCount;
            Q.Cast(target, Packets());
            QFollowTick = Environment.TickCount;
            LastQTarget = target;
        }

        public void Game_OnGameUpdate(EventArgs args)
        {
            if (LastQTarget != null)
            {
                if (Environment.TickCount - QFollowTick >= QFollowTime)
                {
                    LastQTarget = null;
                }
            }

            switch (Program.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>() &&
                        Environment.TickCount - QFollowTick >= QFollowTime)
                    {
                        var target = Cast_BasicLineSkillshot_Enemy(Q, TargetSelector.DamageType.Magical);
                        if (target != null)
                        {
                            QFollowTick = Environment.TickCount;
                            LastQTarget = target;
                        }
                    }
                    if (Program.Menu.Item("useQ_TeamFight_follow").GetValue<bool>() &&
                        Environment.TickCount <= QFollowTick + QFollowTime && LastQTarget != null)
                    {
                        Q.Cast();
                    }
                    if (Program.Menu.Item("useW_TeamFight_shield").GetValue<bool>())
                    {
                        Cast_Shield_onFriend(W, 50, true);
                    }
                    if (Program.Menu.Item("useW_TeamFight_enagage").GetValue<bool>())
                    {
                        EngageFriendLatern();
                    }
                    if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
                    {
                        CastE("ToMe");
                    }
                    if (Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value >= 1)
                    {
                        if (Utility.CountEnemiesInRange((int) R.Range) >=
                            Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value)
                        {
                            R.Cast();
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Program.Menu.Item("useQ_Harass").GetValue<bool>() &&
                        Environment.TickCount - QFollowTick >= QFollowTime)
                    {
                        var target = Cast_BasicLineSkillshot_Enemy(Q, TargetSelector.DamageType.Magical);
                        if (target != null)
                        {
                            QFollowTick = Environment.TickCount;
                            LastQTarget = target;
                        }
                    }
                    if (Program.Menu.Item("useE_Harass").GetValue<bool>())
                    {
                        CastE();
                    }
                    if (Program.Menu.Item("useW_Harass").GetValue<bool>())
                    {
                        SafeFriendLatern();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Program.Menu.Item("useE_LaneClear").GetValue<bool>())
                    {
                        Cast_BasicCircleSkillshot_AOE_Farm(E);
                    }
                    break;
            }

            if (Program.Menu.Item("WQMouse").GetValue<KeyBind>().Active && Q.IsReady() && W.IsReady())
            {
                W.Cast(Player.Position);
                Q.Cast(Game.CursorPos);
            }
            if (Program.Menu.Item("WQMouse").GetValue<KeyBind>().Active)
            {
                try
                {
                    var target =
                        (Obj_AI_Minion)
                            MinionManager.GetMinions(
                                ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.Neutral)
                                .FirstOrDefault();
                    if (target.IsValidTarget())
                    {
                        if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            Utility.DelayAction.Add(
                                2000, () =>
                                {
                                    if (target.IsValidTarget() && target.HasBuff("ThreshQ"))
                                    {
                                        Q.Cast();
                                    }
                                });
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void EngageFriendLatern()
        {
            if (!W.IsReady())
            {
                return;
            }
            var bestcastposition = new Vector3(0f, 0f, 0f);
            foreach (var friend in
                Program.Helper.OwnTeam.Where(
                    hero =>
                        !hero.IsMe && hero.Distance(ObjectManager.Player) <= W.Range + 300 &&
                        hero.Distance(ObjectManager.Player) <= W.Range - 300 && hero.Health / hero.MaxHealth * 100 >= 20 &&
                        Utility.CountEnemiesInRange(150) >= 1))
            {
                var center = ObjectManager.Player.Position;
                const int points = 36;
                var radius = W.Range;

                const double slice = 2 * Math.PI / points;
                for (var i = 0; i < points; i++)
                {
                    var angle = slice * i;
                    var newX = (int) (center.X + radius * Math.Cos(angle));
                    var newY = (int) (center.Y + radius * Math.Sin(angle));
                    var p = new Vector3(newX, newY, 0);
                    if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                    {
                        bestcastposition = p;
                    }
                }
                if (friend.Distance(ObjectManager.Player) <= W.Range)
                {
                    W.Cast(bestcastposition, Packets());
                    return;
                }
            }
            if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
            {
                W.Cast(bestcastposition, Packets());
            }
        }

        private void SafeFriendLatern()
        {
            if (!W.IsReady())
            {
                return;
            }
            var bestcastposition = new Vector3(0f, 0f, 0f);
            foreach (var friend in
                Program.Helper.OwnTeam.Where(
                    hero =>
                        !hero.IsMe && hero.Distance(ObjectManager.Player) <= W.Range + 300 &&
                        hero.Distance(ObjectManager.Player) <= W.Range - 200 && hero.Health / hero.MaxHealth * 100 >= 20 &&
                        !hero.IsDead))
            {
                foreach (var enemy in Program.Helper.EnemyTeam)
                {
                    if (!(friend.Distance(enemy) <= 300))
                    {
                        continue;
                    }
                    var center = ObjectManager.Player.Position;
                    const int points = 36;
                    var radius = W.Range;

                    const double slice = 2 * Math.PI / points;
                    for (var i = 0; i < points; i++)
                    {
                        var angle = slice * i;
                        var newX = (int) (center.X + radius * Math.Cos(angle));
                        var newY = (int) (center.Y + radius * Math.Sin(angle));
                        var p = new Vector3(newX, newY, 0);
                        if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                        {
                            bestcastposition = p;
                        }
                    }
                    if (friend.Distance(ObjectManager.Player) <= W.Range)
                    {
                        W.Cast(bestcastposition, Packets());
                        return;
                    }
                }
                if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
                {
                    W.Cast(bestcastposition, Packets());
                }
            }
        }

        private void CastE(string mode = "")
        {
            if (!E.IsReady() || !ManaManagerAllowCast(E))
            {
                return;
            }
            var target = TargetSelector.GetTarget(E.Range - 10, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }
            E.Cast(
                mode == "ToMe" ? GetReversePosition(ObjectManager.Player.Position, target.Position) : target.Position);
        }
        private static bool IsSecondQ()
        {
            return Q.Instance.Name == "threshqleap";
        }
    }
}