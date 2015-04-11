﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
    internal class EnemyInfo
    {
        public int LastPinged;
        public int LastSeen;
        public Obj_AI_Hero Player;

        public EnemyInfo(Obj_AI_Hero player)
        {
            Player = player;
        }
    }

    internal class Helper
    {
        public List<EnemyInfo> EnemyInfo = new List<EnemyInfo>();
        public IEnumerable<Obj_AI_Hero> EnemyTeam;
        public IEnumerable<Obj_AI_Hero> OwnTeam;

        public Helper()
        {
            var champions = ObjectManager.Get<Obj_AI_Hero>().ToList();

            OwnTeam = champions.Where(x => x.IsAlly);
            EnemyTeam = champions.Where(x => x.IsEnemy);

            EnemyInfo = EnemyTeam.Select(x => new EnemyInfo(x)).ToList();

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            var time = Environment.TickCount;

            foreach (var enemyInfo in EnemyInfo.Where(x => x.Player.IsVisible))
            {
                enemyInfo.LastSeen = time;
            }
        }

        public EnemyInfo GetPlayerInfo(Obj_AI_Hero enemy)
        {
            return Program.Helper.EnemyInfo.Find(x => x.Player.NetworkId == enemy.NetworkId);
        }

        public float GetTargetHealth(EnemyInfo playerInfo, int additionalTime)
        {
            if (playerInfo.Player.IsVisible)
            {
                return playerInfo.Player.Health;
            }

            var predictedhealth = playerInfo.Player.Health +
                                  playerInfo.Player.HPRegenRate *
                                  ((Environment.TickCount - playerInfo.LastSeen + additionalTime) / 1000f);

            return predictedhealth > playerInfo.Player.MaxHealth ? playerInfo.Player.MaxHealth : predictedhealth;
        }

        public static T GetSafeMenuItem<T>(MenuItem item)
        {
            if (item != null)
            {
                return item.GetValue<T>();
            }

            return default(T);
        }
    }
}