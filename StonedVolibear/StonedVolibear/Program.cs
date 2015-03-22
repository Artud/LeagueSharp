using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace StonedVolibear
{
    internal class Program
    {
        private const string Champion = "Volibear";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell Q;

        private static Spell W;

        private static Spell E;

        private static Spell R;

        private static Menu _config;

        private static Items.Item RDO;

        private static Items.Item DFG;

        private static Items.Item YOY;

        private static Items.Item BOTK;

        private static Items.Item HYD;

        private static Items.Item CUT;

        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != Champion)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 405);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 125);


            RDO = new Items.Item(3143, 490f);
            HYD = new Items.Item(3074, 175f);
            DFG = new Items.Item(3128, 750f);
            YOY = new Items.Item(3142, 185f);
            BOTK = new Items.Item(3153, 450f);
            CUT = new Items.Item(3144, 450f);


            //Menu Volibear
            _config = new Menu(Champion, "StonedVolibear", true);

            //ts
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //orb
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo Menu
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("CountW", "Min % enemy hp for W use").SetValue(new Slider(100, 0, 100)));
            _config.SubMenu("Combo").AddItem(new MenuItem("AutoR", "Use Auto R in Combo")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("CountR", "Num of Enemy in Range to Ult").SetValue(new Slider(1, 5, 0)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("HarassE", "Use E in Harass")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind(97, KeyBindType.Press)));

            //Lane Clear
            _config.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));
            _config.SubMenu("Lane Clear").AddItem(new MenuItem("laneQ", "Use Q in lane clear?").SetValue(true));
            _config.SubMenu("Lane Clear").AddItem(new MenuItem("laneW", "Use W in lane clear?").SetValue(true));
            _config.SubMenu("Lane Clear").AddItem(new MenuItem("laneE", "Use E in lane clear?").SetValue(true));
            _config.SubMenu("Lane Clear")
                .AddItem(new MenuItem("Lane Clear MM", "Mana Manager").SetValue(new Slider(50, 1)));

            //Jungle Clear
            _config.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleQ", "Use Q in lane clear?").SetValue(true));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleW", "Use W in lane clear?").SetValue(true));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleE", "Use E in lane clear?").SetValue(true));
            _config.SubMenu("Jungle Clear")
                .AddItem(new MenuItem("Jungle Clear MM", "Mana Manager").SetValue(new Slider(50, 1)));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawWE", "Draw W and E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Drawing.OnDraw += Drawing_OnDraw;


            Game.PrintChat(
                "<font color='#FFFFFFF'>Stoned Volibear Loaded By</font> <font color='#0000FF'>Artud</font>\n <font color='#FFFFFFF'>Credits:</font> <font color='#0000FF'>TheKushStyle</font>");
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {}

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical));
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical));
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
        }

        private static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (_player.ManaPercentage() > _config.Item("Lane Clear MM").GetValue<Slider>().Value)
            {
                if (_config.Item("laneW").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            W.CastOnUnit(minion);
                        }
                    }
                }

                if (_config.Item("laneE").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.Cast();
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            var allMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (_player.ManaPercentage() > _config.Item("Jungle Clear MM").GetValue<Slider>().Value)
            {
                if (_config.Item("jungleQ").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.Cast();
                        }
                    }
                }

                if (_config.Item("jungleW").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            W.CastOnUnit(minion);
                        }
                    }
                }
                if (_config.Item("jungleE").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.Cast();
                        }
                    }
                }
            }
        }


        private static void Harass(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (_config.Item("HarassE").GetValue<bool>() && _player.Distance(target) <= E.Range && E.IsReady())
            {
                E.Cast();
            }
        }

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            //Combo
            if (_player.Distance(target) <= Q.Range && Q.IsReady() && (_config.Item("UseQCombo").GetValue<bool>()))
            {
                Q.Cast();
            }
            if (_player.Distance(target) <= E.Range && E.IsReady() && (_config.Item("UseECombo").GetValue<bool>()))
            {
                E.Cast();
            }
            //WLogic
            var health = target.Health;
            var maxhealth = target.MaxHealth;
            float wcount = _config.Item("CountW").GetValue<Slider>().Value;
            if (health < ((maxhealth * wcount) / 100))
            {
                if (_config.Item("UseWCombo").GetValue<bool>() && W.IsReady())
                {
                    W.Cast(target);
                }
            }
            if (_config.Item("AutoR").GetValue<bool>() && R.IsReady() &&
                (GetNumberHitByR(target) >= _config.Item("CountR").GetValue<Slider>().Value))
            {
                R.Cast();
            }
            if (_config.Item("UseItems").GetValue<bool>())
            {
                if (_player.Distance(target) <= RDO.Range)
                {
                    RDO.Cast(target);
                }
                if (_player.Distance(target) <= HYD.Range)
                {
                    HYD.Cast(target);
                }
                if (_player.Distance(target) <= DFG.Range)
                {
                    DFG.Cast(target);
                }
                if (_player.Distance(target) <= BOTK.Range)
                {
                    BOTK.Cast(target);
                }
                if (_player.Distance(target) <= CUT.Range)
                {
                    CUT.Cast(target);
                }
                if (_player.Distance(target) <= 125f)
                {
                    YOY.Cast();
                }
            }
        }

        private static int GetNumberHitByR(Obj_AI_Base target)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        current =>
                            current.IsEnemy &&
                            Vector3.Distance(_player.ServerPosition, current.ServerPosition) <= R.Range);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("DrawWE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Blue);
            }
        }
    }
}