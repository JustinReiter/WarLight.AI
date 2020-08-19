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

        public static  void InitializeMap(BotMap map)
        {
            knownMap = map.GetMapCopy();
        }

        public static void UpdateMap(BotMap map)
        {
            foreach (var territory in map.Territories)
            {
                if (territory.Value.IsVisible)
                {
                    knownMap.Territories[territory.Key].OwnerPlayerID = territory.Value.OwnerPlayerID;
                    knownMap.Territories[territory.Key].Armies = territory.Value.Armies;
                }
            }
        }

        public static int GuessCurrentIncome(int minIncome, int deploysFound, PlayerIDType opponentId)
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
    }
}
