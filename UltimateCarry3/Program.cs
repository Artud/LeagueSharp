using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
    internal class Program
    {
        public const int LocalVersion = 81; //for update
        public const String Version = "3.0.*";
        public static Champion Champion;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Champions.Azir.Orbwalking.Orbwalker Azirwalker;
        public static Helper Helper;
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //AutoUpdater.InitializeUpdater();
            Chat.Print("Ultimate Carry Version 3");
            Helper = new Helper();

            Menu = new Menu("UltimateCarry", "UltimateCarry_" + ObjectManager.Player.ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);
            if (ObjectManager.Player.ChampionName == "Azir")
            {
                var orbwalking = Menu.AddSubMenu(new Menu("AzirWalking", "Orbwalking"));
                Azirwalker = new Champions.Azir.Orbwalking.Orbwalker(orbwalking);
                Menu.Item("FarmDelay").SetValue(new Slider(125, 100, 200));
            }
            else
            {
                var orbwalking = Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(orbwalking);
                Menu.Item("FarmDelay").SetValue(new Slider(0, 0, 200));
            }
            var bushRevealer = new AutoBushRevealer();
            //var overlay = new Overlay();

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                new PluginLoader();
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                //Champion = new Champion(); //Champ not supported
            }

            Menu.AddToMainMenu();
            Chat.Print("You have the latest version.");
            Chat.Print("Ultimate Carry loaded! (If something is not working please report it)");
        }
    }
}