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

        public static int GetDeploys(PlayerIDType playerId, int turn)
        {
            if (GetTurns() > turn)
            {
                return deploysByTurn[turn][playerId];
            }
            throw new IndexOutOfRangeException();
        }

        public static void SetDeploys(PlayerIDType playerId, int deploys, int turn)
        {
            if (GetTurns() < turn)
            {
                deploysByTurn.Add(new Dictionary<PlayerIDType, int>());
            }
            deploysByTurn[turn][playerId] = deploys;
        }

        public static void AddTurn(int turn)
        {
            while (GetTurns() <= turn)
            {
                deploysByTurn.Add(new Dictionary<PlayerIDType, int>());
            }
        }

        public static int GetTurns()
        {
            return deploysByTurn.Count;
        }
    }
}
