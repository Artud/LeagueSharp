using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashJukeAssistant
{
    class Program
    {
        static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Spell _juker;
        public static Spell juker { get { return _juker; } }
        public static float lastWardTime = -60.0f;
        public static float lastFlashTime = -60.0f;
        public static float delayTime = 0.1f;
        public static int flashSpot = 0;
        public static int setdist1 = 650; //FOR CHECKS
        public static int thick = 1;
        public static bool mustCheckForWards = true; //ally wards
        private static Menu Config;
        private static SpellSlot flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");
        private static SpellSlot recall = SpellSlot.Recall;
        public static int[][] Spots = new int[28][];
        public static int[][] SpotsFinal = new int[28][];
        public static Vector3[] SpotsVector = new Vector3[28];
        public static Vector3[] SpotsFinalVector = new Vector3[28];
        public static Vector2[] SpotsVector2 = new Vector2[28];
        public static Vector2[] SpotsFinalVector2 = new Vector2[28];



        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnGameUpdate;
            setupCoords();
            Config = new Menu("Flash Juke Assistant", "Flash Juke Assistant", true);
            Config.AddItem(new MenuItem("key", "Keybind: ")).SetValue(new KeyBind((byte)'T', KeyBindType.Press));
            Config.AddItem(new MenuItem("ward", "Use Wards: ")).SetValue(true);
            Config.AddItem(new MenuItem("disabledraw", "Disable Drawings: ")).SetValue(false);
            Config.AddToMainMenu();

        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (!(Config.Item("key").GetValue<KeyBind>().Active))
            {
                mustCheckForWards = true;
            }
            findClosest();
            if (!Config.Item("key").GetValue<KeyBind>().Active || !(Game.ClockTime > lastFlashTime + 60.0f)
                || (ObjectManager.Player.Spellbook.CanUseSpell(flash) != SpellState.Ready))
            {
                return;
            }
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, SpotsVector[flashSpot]);
            if (condMatch())
            {
                doTheFlash();
            }
        }

        private static bool condMatch()
        {
            const double setDist = 30;
            var rangeCond = (((Spots[flashSpot][0] - ObjectManager.Player.Position.X)
                              * (Spots[flashSpot][0] - ObjectManager.Player.Position.X))
                             + ((Spots[flashSpot][2] - ObjectManager.Player.Position.Y)
                                * (Spots[flashSpot][2] - ObjectManager.Player.Position.Y)) < setDist * setDist);
            return rangeCond;
        }



        public static void doTheFlash()
        {
            if (mustCheckForWards)
            {
                mustCheckForWards = false;
                if (!Config.Item("ward").GetValue<bool>())
                {
                    return;
                }
                if (!(haveVision())) //pink not down, put it if we have it
                {
                    if (CanWard(1) == true)
                    {
                        ward(1);
                    }
                    else
                    {
                        if (!(haveSight()))
                        {
                            ward(0);
                        }
                    }
                }
                else
                {
                    if (haveSight())
                    {
                        return;
                    }
                    if (CanWard(0) == true)
                    {
                        ward(0);
                    }
                }
            }
            else if (Game.ClockTime - (Game.Ping * 0.001f + delayTime) > lastFlashTime &&
                !(checkForEnemyPlayer() || checkForEnemyWard()))
            {
                ObjectManager.Player.Spellbook.CastSpell(flash, SpotsFinalVector[flashSpot]);
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, SpotsFinalVector[flashSpot]);
                ObjectManager.Player.Spellbook.CastSpell(recall);
                lastFlashTime = Game.ClockTime;
            }

       
        }
        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("disabledraw").GetValue<bool>())
            {
                return;
            }
            for (var i = 0; i < 28; i++)
            {
                if (i == flashSpot && SpotsVector[i].IsOnScreen())
                {
                    if (ObjectManager.Player.Spellbook.CanUseSpell(flash) == SpellState.Ready)
                    {
                        Drawing.DrawLine(
                            Drawing.WorldToScreen(SpotsVector[i]),
                            Drawing.WorldToScreen(SpotsFinalVector[i]),
                            2,
                            System.Drawing.Color.LightGreen);
                    }
                    Render.Circle.DrawCircle(SpotsVector[i], 50, System.Drawing.Color.LightGreen, thick);
                }
                else if (i != flashSpot && SpotsVector[i].IsOnScreen())
                {
                    if (ObjectManager.Player.Spellbook.CanUseSpell(flash) == SpellState.Ready)
                    {
                        Drawing.DrawLine(
                            Drawing.WorldToScreen(SpotsVector[i]),
                            Drawing.WorldToScreen(SpotsFinalVector[i]),
                            2,
                            System.Drawing.Color.Red);
                    }
                    Render.Circle.DrawCircle(SpotsVector[i], 50, System.Drawing.Color.Red, thick);
                }
            }
        }

        private static void setupCoords()
        {
            const int constXmove = 425;
            const int constZmove = 275;
            //-----------------------
            Spots[0] = new int[] { 5926 + constXmove, 40, 12500 + constZmove };
            Spots[1] = new int[] { 8155 + constXmove, 54, 1885 + constZmove };
            Spots[2] = new int[] { 5815 + constXmove, 53, 11357 + constZmove };
            Spots[3] = new int[] { 8135 + constXmove, 57, 3058 + constZmove };
            Spots[4] = new int[] { 11530 + constXmove, 53, 4733 + constZmove };
            Spots[5] = new int[] { 2485 + constXmove, 53, 9643 + constZmove };
            Spots[6] = new int[] { 9943 + constXmove, 55, 6415 + constZmove };
            Spots[7] = new int[] { 4123 + constXmove, 52, 7979 + constZmove };
            Spots[8] = new int[] { 3935 + constXmove, 53, 7170 + constZmove };
            Spots[9] = new int[] { 8900 + constXmove, 64, 2380 + constZmove };
            Spots[10] = new int[] { 5718 + constXmove, 53, 3502 + constZmove };
            Spots[11] = new int[] { 7052 + constXmove, 55, 3223 + constZmove };
            Spots[12] = new int[] { 7059 + constXmove, 55, 3081 + constZmove };
            Spots[13] = new int[] { 5054 + constXmove, 41, 11998 + constZmove };
            Spots[14] = new int[] { 6966 + constXmove, 53, 11282 + constZmove };
            Spots[15] = new int[] { 6959 + constXmove, 53, 11416 + constZmove };
            Spots[16] = new int[] { 8185 + constXmove, 50, 11103 + constZmove };
            Spots[17] = new int[] { 9949 + constXmove, 55, 7249 + constZmove };
            Spots[18] = new int[] { 11361 + constXmove, -62, 4190 + constZmove };
            Spots[19] = new int[] { 5200 + constXmove, -65, 9190 + constZmove };
            Spots[20] = new int[] { 5405 + constXmove, 55, 9926 + constZmove };
            Spots[21] = new int[] { 8655 + constXmove, -64, 5195 + constZmove };
            Spots[22] = new int[] { 8583 + constXmove, 55, 4408 + constZmove };
            Spots[23] = new int[] { 2788 + constXmove, -65, 10204 + constZmove };
            Spots[24] = new int[] { 3460 + constXmove, 55, 7525 + constZmove };
            Spots[25] = new int[] { 10595 + constXmove, 54, 6915 + constZmove };
            Spots[26] = new int[] { 5924 + constXmove, 51, 4975 + constZmove };
            Spots[27] = new int[] { 8002 + constXmove, 53, 9460 + constZmove };
            //-----------------------
            SpotsFinal[0] = new int[] { 5521 + constXmove, 40, 12511 + constZmove };
            SpotsFinal[1] = new int[] { 8537 + constXmove, 55, 1900 + constZmove };
            SpotsFinal[2] = new int[] { 6109 + constXmove, 54, 11160 + constZmove };
            SpotsFinal[3] = new int[] { 7867 + constXmove, 56, 3293 + constZmove };
            SpotsFinal[4] = new int[] { 11885 + constXmove, 45, 4933 + constZmove };
            SpotsFinal[5] = new int[] { 2120 + constXmove, 53, 9519 + constZmove };
            SpotsFinal[6] = new int[] { 9594 + constXmove, 51, 6319 + constZmove };
            SpotsFinal[7] = new int[] { 4446 + constXmove, 34, 8148 + constZmove };
            SpotsFinal[8] = new int[] { 4283 + constXmove, 54, 6970 + constZmove };
            SpotsFinal[9] = new int[] { 8875 + constXmove, 55, 2000 + constZmove };
            SpotsFinal[10] = new int[] { 5350 + constXmove, 54, 3265 + constZmove };
            SpotsFinal[11] = new int[] { 7391 + constXmove, 55, 3297 + constZmove };
            SpotsFinal[12] = new int[] { 6650 + constXmove, 55, 2860 + constZmove };
            SpotsFinal[13] = new int[] { 5082 + constXmove, 40, 12374 + constZmove };
            SpotsFinal[14] = new int[] { 6590 + constXmove, 54, 11145 + constZmove };
            SpotsFinal[15] = new int[] { 7303 + constXmove, 52, 11500 + constZmove };
            SpotsFinal[16] = new int[] { 8600 + constXmove, 54, 11141 + constZmove };
            SpotsFinal[17] = new int[] { 9665 + constXmove, 54, 7497 + constZmove };
            SpotsFinal[18] = new int[] { 11435 + constXmove, -55, 3820 + constZmove };
            SpotsFinal[19] = new int[] { 4900 + constXmove, -63, 8940 + constZmove };
            SpotsFinal[20] = new int[] { 5772 + constXmove, 54, 10033 + constZmove };
            SpotsFinal[21] = new int[] { 8935 + constXmove, -64, 5417 + constZmove };
            SpotsFinal[22] = new int[] { 8267 + constXmove, 55, 4415 + constZmove };
            SpotsFinal[23] = new int[] { 2685 + constXmove, -64, 10567 + constZmove };
            SpotsFinal[24] = new int[] { 3100 + constXmove, 56, 7534 + constZmove };
            SpotsFinal[25] = new int[] { 10936 + constXmove, 54, 6865 + constZmove };
            SpotsFinal[26] = new int[] { 6110 + constXmove, 51, 4560 + constZmove };
            SpotsFinal[27] = new int[] { 7880 + constXmove, 53, 9845 + constZmove };
            for (var i = 0; i < 28; i++)
            {
                SpotsVector[i] = new Vector3(new Vector2(Spots[i][0], Spots[i][2]), Spots[i][1]);
                SpotsFinalVector[i] = new Vector3(new Vector2(SpotsFinal[i][0], SpotsFinal[i][2]), SpotsFinal[i][1]);
                SpotsVector2[i] = new Vector2(Spots[i][0], Spots[i][2]);
                SpotsFinalVector2[i] = new Vector2(SpotsFinal[i][0], SpotsFinal[i][2]);
            }
        }
        private static void findClosest()
        {
            double x1 = ObjectManager.Player.Position.X;
            double y1 = ObjectManager.Player.Position.Y;
            double dist = 10000000000;
            for (var i = 0; i < 28; i++)
            {
                if (!(((Spots[i][0] - x1) * (Spots[i][0] - x1)) + ((Spots[i][2] - y1) * (Spots[i][2] - y1)) < dist))
                {
                    continue;
                }
                dist = ((Spots[i][0] - x1) * (Spots[i][0] - x1)) + ((Spots[i][2] - y1) * (Spots[i][2] - y1));
                flashSpot = i;
            }
        }
        public static bool haveSight()
        {
            return (checkForFriendlyPlayer() || checkForFriendlyWard());
        }
        public static bool haveVision()
        {
            return (checkForFriendlyVisionWard());
        }
        public static bool checkForEnemyPlayer()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(Hero => Hero.Team != ObjectManager.Player.Team && NavMesh.IsWallOfGrass(Hero.Position, 1.0f) && (((SpotsFinal[flashSpot][0] - Hero.Position.X) * (SpotsFinal[flashSpot][0] - Hero.Position.X)) + ((SpotsFinal[flashSpot][2] - Hero.Position.Y) * (SpotsFinal[flashSpot][2] - Hero.Position.Y)) < setdist1 * setdist1));
        }

        public static bool checkForEnemyWard()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Any(Object => Object.Team != ObjectManager.Player.Team && NavMesh.IsWallOfGrass(Object.Position, 1.0f) && (((SpotsFinal[flashSpot][0] - Object.Position.X) * (SpotsFinal[flashSpot][0] - Object.Position.X)) + ((SpotsFinal[flashSpot][2] - Object.Position.Y) * (SpotsFinal[flashSpot][2] - Object.Position.Y)) < setdist1 * setdist1));
        }

        public static bool checkForFriendlyPlayer()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(Hero => Hero.Team == ObjectManager.Player.Team && NavMesh.IsWallOfGrass(Hero.Position, 1.0f) && (((SpotsFinal[flashSpot][0] - Hero.Position.X) * (SpotsFinal[flashSpot][0] - Hero.Position.X)) + ((SpotsFinal[flashSpot][2] - Hero.Position.Y) * (SpotsFinal[flashSpot][2] - Hero.Position.Y)) < setdist1 * setdist1));
        }

        public static bool checkForFriendlyWard()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Any(Object => Object.Team == ObjectManager.Player.Team && NavMesh.IsWallOfGrass(Object.Position, 1.0f) && (((SpotsFinal[flashSpot][0] - Object.Position.X) * (SpotsFinal[flashSpot][0] - Object.Position.X)) + ((SpotsFinal[flashSpot][2] - Object.Position.Y) * (SpotsFinal[flashSpot][2] - Object.Position.Y)) < setdist1 * setdist1));
        }

        public static bool checkForFriendlyVisionWard()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Any(Object => Object.Team == ObjectManager.Player.Team && NavMesh.IsWallOfGrass(Object.Position, 1.0f) && Object.Name.ToUpper().Contains("VISION") && (((SpotsFinal[flashSpot][0] - Object.Position.X) * (SpotsFinal[flashSpot][0] - Object.Position.X)) + ((SpotsFinal[flashSpot][2] - Object.Position.Y) * (SpotsFinal[flashSpot][2] - Object.Position.Y)) < setdist1 * setdist1));
        }

        private static void ward(int mode)
        {
            if (CanWard(mode))
            {
                Items.UseItem(GetWardSlot(mode), SpotsFinalVector[flashSpot]);
                lastWardTime = Game.ClockTime;
            }
            else
            {
                Game.PrintChat("Lacking wards");
            }
        }
        public static bool CanWard(int mode)
        {
            var wardSlot = GetWardSlot(mode);
            return wardSlot != -1;
        }
        public static int GetWardSlot(int mode)
        {
            var wardIds = new[] { 0 };
            if (mode == 0)
            {
                wardIds = new[] { 3340, 3350, 3361, 3154, 2045, 2049, 2050, 2044 };
            }
            if (mode == 1)
            {
                wardIds = new[] { 3362, 2043 };
            }

            for (var i = 0; i < wardIds.Count(); i++)
            {
                if (Items.CanUseItem(wardIds[i]))
                {
                    return wardIds[i];
                }
            }
            return -1;
        }
    }
}
