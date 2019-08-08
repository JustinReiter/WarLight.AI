using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class DeploymentTracker
    {
        private static List<Dictionary<PlayerIDType, int>> deploysByTurn = new List<Dictionary<PlayerIDType, int>>();
        



        public static int GetDeploys(PlayerIDType playerId, BotMain bot)
        {
            if (GetTurns() <= bot.NumberOfTurns)
            {
                return deploysByTurn[bot.NumberOfTurns][playerId];
            }
            throw new IndexOutOfRangeException();
        }

        public static void SetDeploys(PlayerIDType playerId, int deploys, BotMain bot)
        {
            if (GetTurns() < bot.NumberOfTurns)
            {
                deploysByTurn.Add(new Dictionary<PlayerIDType, int>());
            }
            deploysByTurn[bot.NumberOfTurns][playerId] = deploys;
        }


        private static int GetTurns()
        {
            return deploysByTurn.Count;
        }

    }
}
