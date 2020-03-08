using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class CycleTracker
    {
        private static bool hasFoundCycle = false;
        private static bool isOddTurnCycle;

        public static bool IsCycleFound()
        {
            return hasFoundCycle;
        }

        public static void SetCycle(bool isOdd)
        {
            if (!hasFoundCycle)
            {
                isOddTurnCycle = isOdd;
                hasFoundCycle = true;
            }
        }

        /// <summary>
        /// Determines the cycle order by looking at given picks. Possible choices are:
        /// Odd TURN PRIORITY
            //126
            //136
            //234
            //235
            //236

        /// Even TURN PRIORITY
            //135
            //145

        /// INDETERMINABLE PRIORITY
            //123
            //124
            //125
            //134
        /// </summary>
        /// <param name="chosenPicks">6 picks chosen</param>
        /// <param name="givenPicks">3 picks given</param>

        public static void SetCyclePicks(List<TerritoryIDType> chosenPicks, List<TerritoryIDType> givenPicks)
        {
            if (chosenPicks[0] != givenPicks[0] || (chosenPicks[0] == givenPicks[0] && chosenPicks[5] == givenPicks[2]))
            {
                SetCycle(true);
                AILog.Log("Cycle", "Able to determine cycle order through picks");
                AILog.Log("Cycle", "\tOddTurnPriority: " + isOddTurnCycle);
            } else if (chosenPicks[0] == givenPicks[0] && (chosenPicks[2] == givenPicks[1] || chosenPicks[3] == givenPicks[1])) {
                SetCycle(false);
                AILog.Log("Cycle", "Able to determine cycle order through picks");
                AILog.Log("Cycle", "\tOddTurnPriority: " + isOddTurnCycle);
            }
            AILog.Log("Cycle", "Unable to determine cycle order through picks");
        }

        /// <summary>
        /// Sets the cycle order if not found already. Looks at deploys and attacks
        /// </summary>
        /// <param name="deployments"></param>
        /// <param name="attackTransfers"></param>
        /// <param name="meID"></param>
        /// <param name="numberOfTurns"></param>
        public static void SetCycleOrders(List<GameOrderDeploy> deployments, List<GameOrderAttackTransfer> attackTransfers, PlayerIDType meID, int numberOfTurns)
        {
            // TODO: incorporate checks for cards
            if (deployments[0].PlayerID != meID || attackTransfers[0].PlayerID != meID)
            {
                Memory.CycleTracker.SetCycle(numberOfTurns % 2 == 1);
                AILog.Log("Cycle", "Turn " + numberOfTurns + ": Able to determine cycle order through first order");
                AILog.Log("Cycle", "Turn " + numberOfTurns + ":\tOddTurnPriority: " + isOddTurnCycle);
            } else if (ParseListForOrder(deployments, meID, numberOfTurns)) {
                AILog.Log("Cycle", "Turn " + numberOfTurns + ": Able to determine cycle order through deployments");
                AILog.Log("Cycle", "Turn " + numberOfTurns + ":\tOddTurnPriority: " + isOddTurnCycle);
            } else if (ParseListForOrder(attackTransfers, meID, numberOfTurns))
            {
                AILog.Log("Cycle", "Turn " + numberOfTurns + ": Able to determine cycle order through attacks/transfers");
                AILog.Log("Cycle", "Turn " + numberOfTurns + ":\tOddTurnPriority: " + isOddTurnCycle);
            } else
            {
                AILog.Log("Cycle", "Turn " + numberOfTurns + ": Unable to determine cycle order");
            }
        }

        private static bool ParseListForOrder(dynamic list, PlayerIDType me, int numberOfTurns)
        {
            int enemyIndex = -1;
            bool canFindOrderCycle = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PlayerID != me && enemyIndex == -1)
                {
                    enemyIndex = i;
                    break;
                }
                else if (enemyIndex != -1 && list[i].PlayerID == me)
                {
                    canFindOrderCycle = true;
                }
            }

            if (canFindOrderCycle)
            {
                SetCycle(enemyIndex % 2 == 0 && numberOfTurns % 2 == 1);
                return true;
            }

            return false;
        }
    }
}
