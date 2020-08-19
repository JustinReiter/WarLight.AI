using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class MapTracker
    {
        private static BotMap knownMap;
        private static BotMap likelyMap;
        private static BotMap archivedLikelyMap;

        public static  void InitializeMap(BotMap map, BotMain bot)
        {
            knownMap = map.GetMapCopy();

            foreach (var pick in PickTracker.GetEnemyPickList())
            {
                AILog.Log("Logging enemy pick:", "Number of armies on " +  map.Territories[pick].Details.Name + " : " + map.Territories[pick].Value.Armies);
                knownMap.Territories[pick].OwnerPlayerID = bot.Players.Values.Where(o => o.ID != bot.Me.ID).ToList()[0].ID;
                knownMap.Territories[pick].Armies = new Armies(bot.Settings.InitialPlayerArmiesPerTerritory);
            }

            likelyMap = knownMap.GetMapCopy();
            archivedLikelyMap = likelyMap.GetMapCopy();
        }

        public static void UpdateMap(BotMap map)
        {
            foreach (var territory in map.Territories)
            {
                if (territory.Value.IsVisible)
                {
                    AILog.Log("Logging visible territory:", "Number of armies on " + territory.Value.Details.Name + " : " + territory.Value.Armies);
                    knownMap.Territories[territory.Key].OwnerPlayerID = territory.Value.OwnerPlayerID;
                    knownMap.Territories[territory.Key].Armies = territory.Value.Armies;
                    likelyMap.Territories[territory.Key].OwnerPlayerID = territory.Value.OwnerPlayerID;
                    likelyMap.Territories[territory.Key].Armies = territory.Value.Armies;
                    archivedLikelyMap.Territories[territory.Key].OwnerPlayerID = territory.Value.OwnerPlayerID;
                    archivedLikelyMap.Territories[territory.Key].Armies = territory.Value.Armies;
                }
            }
        }

        public static int GuessKnownCurrentIncome(int minIncome, PlayerIDType opponentId)
        {
            int income = minIncome;
            foreach (var bonus in knownMap.Bonuses.Values)
            {
                bool hasAllTerr = true;
                foreach (var terr in bonus.Territories)
                {
                    if (terr.OwnerPlayerID != opponentId)
                    {
                        hasAllTerr = false;
                        break;
                    }
                }

                if (hasAllTerr)
                {
                    income += bonus.Amount;
                }
            }


            return income;
        }

        public static int GuessLikelyCurrentIncome(int minIncome, int deploysFound, PlayerIDType opponentId)
        {
            int income = minIncome;
            foreach (var bonus in likelyMap.Bonuses.Values)
            {
                bool hasAllTerr = true;
                foreach (var terr in bonus.Territories)
                {
                    if (terr.OwnerPlayerID != opponentId)
                    {
                        hasAllTerr = false;
                        break;
                    }
                }

                if (hasAllTerr)
                {
                    income += bonus.Amount;
                }
            }


            return income;
        }

        public static void ResetLikelyMap(bool toArchivedMap)
        {
            if (toArchivedMap)
            {
                likelyMap = archivedLikelyMap.GetMapCopy();
            } else
            {
                likelyMap = knownMap.GetMapCopy();
            }
        }
    }
}
