/*
 * Warning: This file has been generated automatically.
 */

using LeagueSharp;
using UltimateCarry.Champions;

namespace UltimateCarry
{
    public class PluginLoader
    {
        public static bool loaded;

        public PluginLoader()
        {
            if (!loaded)
            {
                switch (ObjectManager.Player.ChampionName.ToLower())
                {
                    case "ahri":
                        new Ahri();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry" + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "Amumu":
                        new Amumu();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "azir":
                        new Azir();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "caitlyn":
                        new Caitlyn();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "elise":
                        new Elise();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "ezreal":
                        new Ezreal();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "gangplank":
                        new Gangplank();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "gnar":
                        new Gnar();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "jax":
                        new Jax();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "khazix":
                        new Khazix();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "lucian":
                        new Lucian();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "lulu":
                        new Lulu();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "missfortune":
                        new MissFortune();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "morgana":
                        new Morgana();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "riven":
                        new Riven();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "swain":
                        new Swain();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "teemo":
                        new Teemo();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "thresh":
                        new Thresh();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "tristana":
                        new Tristana();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "twistedfate":
                        new TwistedFate();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "xerath":
                        new Xerath();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "zed":
                        new Zed();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    case "zyra":
                        new Zyra();
                        loaded = true;
                        Game.PrintChat(
                            "<font color = \"#FFB6C1\">UltimateCarry " + ObjectManager.Player.ChampionName +
                            " Loaded!</font>");
                        break;
                    default:
                        Game.PrintChat("UltimateCarry => {0} Not Supported!", ObjectManager.Player.ChampionName);
                        break;
                }
            }
        }
    }
}