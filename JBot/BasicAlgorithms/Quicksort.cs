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
            if (to - from < 2 || list == null)
            {
                return;
            }
            int pointer = from;
            for (int i = from + 1; i < to; i++)
            {
                if (vF(list[i]) > vF(list[from]))
                {
                    Swap(i, ++pointer, ref list);
                }
            }
            Swap(from, pointer, ref list);
            QuicksortList(ref list, from, pointer, vF);
            QuicksortList(ref list, pointer + 1, to, vF);
        }

        public static void QuicksortPath(ref PathVector vector, int from, int to)
        {
            if (to - from < 2 || vector == null)
            {
                return;
            }
            int pointer = from;
            for (int i = from + 1; i < to; i++)
            {
                if (vector.nodes[from].minDistance > vector.nodes[i].minDistance)
                {
                    Swap(++pointer, i, ref vector.nodes);
                }
            }

            Swap(pointer, from, ref vector.nodes);
            QuicksortPath(ref vector, from, pointer);
            QuicksortPath(ref vector, pointer + 1, to);
        }

        public static void QuicksortFastestPath(ref List<PathNode> nodes, int from, int to)
        {
            if (to - from < 2 || nodes == null)
            {
                return;
            }

            int pointer = from;
            for (int i = from + 1; i < to; i++)
            {
                if (nodes[pointer].GetSteps() < nodes[i].GetSteps())
                {
                    Swap(++pointer, i, ref nodes);
                }
            }

            Swap(from, pointer, ref nodes);
            QuicksortFastestPath(ref nodes, from, pointer);
            QuicksortFastestPath(ref nodes, pointer + 1, to);

        }

        private static void Swap<T>(int one, int two, ref List<T> list)
        {
            T temp = list[one];
            list[one] = list[two];
            list[two] = temp;
        }
    }
}
