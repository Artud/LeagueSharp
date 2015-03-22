using System;
using System.Collections.Specialized;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace HecaCopter2
{
    internal class Program
    {
        private const string Champion = "Hecarim";
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;
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

            _q = new Spell(SpellSlot.Q, 350);
            _w = new Spell(SpellSlot.W, 525);
            _e = new Spell(SpellSlot.E, 0);
            _r = new Spell(SpellSlot.R, 1000);
            _r.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);



            //Menu Hecarim
            _config = new Menu(Champion, "HecaCopter", true);

            //ts
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //orb
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo Menu
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("comboQ", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("comboW", "Use W")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("countW", "Min Enemies for W use").SetValue(new Slider(3, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("healthPercentWCombo", "Min HP for W use").SetValue(new Slider(75, 1, 100)));
            _config.SubMenu("Combo").AddItem(new MenuItem("comboE", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("comboR", "Use R in TF")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("countR", "Min enemies to ult in TF").SetValue(new Slider(3, 1, 5)));
            _config.SubMenu("Combo").AddItem(new MenuItem("comboRGanks", "Use R in Ganks")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("countRGanks", "Min enemies to ult in Ganks").SetValue(new Slider(1, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Lane Clear
            _config.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));
            _config.SubMenu("Lane Clear").AddItem(new MenuItem("laneQ", "Use Q in lane clear?").SetValue(true));
            _config.SubMenu("Lane Clear").AddItem(new MenuItem("laneW", "Use W in lane clear?").SetValue(true));
            _config.SubMenu("Lane Clear")
                .AddItem(new MenuItem("laneClearMM", "Mana Manager").SetValue(new Slider(50, 1)));
            _config.SubMenu("Lane Clear")
                .AddItem(new MenuItem("minLaneQ", "Minimum Q targets").SetValue(new Slider(3, 1, 6)));

            _config.SubMenu("Lane Clear")
                .AddItem(new MenuItem("healthPercentW", "Health percent to use W").SetValue(new Slider(45, 1, 100)));

            //Jungle Clear
            _config.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleQ", "Use Q to clear jg?").SetValue(true));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleW", "Use W to clear jg?").SetValue(true));
            _config.SubMenu("Jungle Clear").AddItem(new MenuItem("jungleE", "Use E to clear jg?").SetValue(true));
            _config.SubMenu("Jungle Clear")
                .AddItem(new MenuItem("jungleClearMM", "Mana Manager").SetValue(new Slider(50, 1)));
            _config.SubMenu("Lane Clear")
                .AddItem(
                    new MenuItem("healthPercentWJungle", "Health percent to use W").SetValue(new Slider(45, 1, 100)));

            //Killsteal
            _config.AddSubMenu(new Menu("Kill Steal", "Kill Steal"));
            _config.SubMenu("Kill Steal").AddItem(new MenuItem("ksQ", "Use Q for kill steal?").SetValue(true));
            _config.SubMenu("Kill Steal").AddItem(new MenuItem("ksW", "Use W for kill steal?").SetValue(true));
            _config.SubMenu("Kill Steal")
                .AddItem(new MenuItem("useIgnite", "Use Ignite for kill steal?").SetValue(true));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("urfMode", "URF Mode?").SetValue(false));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQWR", "Draw Q, W and R")).SetValue(true);
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
                "<font color='#FFFFFFF'> HecaCopter By</font> <font color='#0000FF'>Artud and imsosharp</font>");
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {}

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
            KillSteal();
        }

        private static void Combo()
        {
            var nearbyEnemies =
                HeroManager.Enemies.Where(h => h.IsValidTarget() && h.Distance(_player) < 1000)
                    .OrderBy(h => h.Distance(_player));
            var QTarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var RTarget = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            if (!nearbyEnemies.Any())
                return;
            if (_config.Item("comboQ").GetValue<bool>() && _q.IsReady() && QTarget.IsValidTarget())
            {
                _q.Cast();
            }
            if (_config.Item("comboE").GetValue<bool>() && _e.IsReady())
            {
                if (!QTarget.IsFacing(_player) && _player.Distance(QTarget) > _q.Range)
                    //you ain't running away bitch :^) xd 
                {
                    _e.Cast();
                }
            }
            if (_config.Item("comboW").GetValue<bool>() && _w.IsReady())
            {
                if (_player.CountEnemiesInRange(700) >= _config.Item("countW").GetValue<Slider>().Value ||
                    _player.HealthPercentage() <= _config.Item("healthPercentWCombo").GetValue<Slider>().Value)
                {
                    _w.Cast();
                }
            }
            if (_config.Item("comboR").GetValue<bool>() && _r.IsReady())
            {
                if (_player.CountAlliesInRange(1000) <= 2)
                {
                    if (nearbyEnemies.FirstOrDefault().Distance(_player) < _q.Range)
                        return;
                    _r.CastIfWillHit(RTarget, _config.Item("countRGanks").GetValue<Slider>().Value);
                }
                else
                {
                    _r.CastIfWillHit(RTarget, _config.Item("countR").GetValue<Slider>().Value);
                }
            }

        }

        private static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);

            if (_player.ManaPercentage() > _config.Item("laneClearMM").GetValue<Slider>().Value ||
                _config.Item("urfMode").GetValue<bool>())
            {
                if (_config.Item("laneQ").GetValue<bool>() && _q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            if (allMinions.Count > _config.Item("minLaneQ").GetValue<Slider>().Value)
                            {
                                _q.Cast();
                            }
                        }
                    }
                }
            }
            if (_config.Item("laneW").GetValue<bool>() &&
                _player.HealthPercentage() <= _config.Item("healthPercentW").GetValue<Slider>().Value && _w.IsReady())
            {
                _w.Cast();
            }
        }

private static void JungleClear()
        {
            var jungleMobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral).OrderBy(m => m.MaxHealth);
            if (_config.Item("jungleQ").GetValue<bool>() && _q.IsReady() && jungleMobs.FirstOrDefault().IsValidTarget())
            {
                _q.Cast();
            }
            if (_config.Item("jungleClearMM").GetValue<Slider>().Value >= _player.ManaPercentage())
            {
                if (_config.Item("jungleE").GetValue<bool>() && _e.IsReady())
                {
                    _e.Cast();
                }
                if (_config.Item("healthPercentWJungle").GetValue<Slider>().Value <= _player.HealthPercentage() && _w.IsReady() && _config.Item("jungleW").GetValue<bool>())
                {
                    _w.Cast();
                }
            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("DrawQWR").GetValue<bool>())
            {
                Drawing.DrawCircle(_player.Position, _q.Range, Color.Red);
                Drawing.DrawCircle(_player.Position, _w.Range, Color.Green);
                Drawing.DrawCircle(_player.Position, _r.Range, Color.Blue);
            }
        }

        private static void KillSteal()
        {
            var qkSable = HeroManager.Enemies.Where(h => h.IsValidTarget() && h.Health < _q.GetDamage(h));
            var wkSable = HeroManager.Enemies.Where(h => h.IsValidTarget() && h.Health < _w.GetDamage(h));
            if (qkSable.Any() && _config.Item("ksQ").GetValue<bool>())
            {
                _q.Cast(qkSable.FirstOrDefault());
            }
            if (wkSable.Any() && _config.Item("ksW").GetValue<bool>())
            {
                _w.Cast(wkSable.FirstOrDefault());
            }
        }
    }
}