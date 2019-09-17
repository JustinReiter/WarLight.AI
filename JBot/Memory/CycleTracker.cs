using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class CycleTracker
    {
        public static bool hasFoundCycle = false;
        public static bool isOddTurnCycle;

        public static bool IsCycleFound()
        {
            return hasFoundCycle;
        }

        public static void SetCycle(bool isOdd)
        {
            isOddTurnCycle = isOdd;
            hasFoundCycle = true;
        }

        /// <summary>
        /// Determines the cycle order by looking at given picks. Possible choices are:
        /// FIRST TURN PRIORITY
            //126
            //136
            //234
            //235
            //236

            //SECOND TURN PRIORITY
            //135
            //145

            //INDETERMINABLE PRIORITY
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
            } else if (chosenPicks[0] == givenPicks[0] && (chosenPicks[2] == givenPicks[1] || chosenPicks[3] == givenPicks[1])) {
                SetCycle(false);
            }
        }
    }
}
