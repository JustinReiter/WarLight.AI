using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.BasicAlgorithms
{
    static class Quicksort
    {

        public delegate int ValueFunction(BotTerritory list);

        public static void QuicksortList(ref List<BotTerritory> list, int from, int to, ValueFunction vF)
        {
            if (to - from < 2)
            {

            }
            int pointer = from;
            for (int i = from + 1; i < to; i++)
            {
                if (vF(list[i]) > vF(list[from]))
                {
                    Swap(i, ++pointer, ref list);
                }
            }
            Swap(0, pointer, ref list);
            QuicksortList(ref list, from, pointer, vF);
            QuicksortList(ref list, pointer + 1, to, vF);
        }


        private static void Swap(int one, int two, ref List<BotTerritory> list)
        {
            BotTerritory temp = list[one];
            list[one] = list[two];
            list[two] = temp;
        }
    }
}
