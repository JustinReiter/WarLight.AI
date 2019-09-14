using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.BasicAlgorithms;

namespace WarLight.Shared.AI.JBot.Memory
{
    public static class PathTracker
    {
        public static List<PathNode> _paths = new List<PathNode>();
        public static List<PathNode> _enemyVicinity = new List<PathNode>();

        public static void AddPath(PathNode node)
        {
            _paths.Add(node);
            Sort(ref _paths);
        }

        public static void SetPaths(List<PathNode> paths)
        {
            _paths = new List<PathNode>(paths);
            Sort(ref _paths);
        }

        public static void AddEnemyVicinity(PathNode enemyVicinity)
        {
            _enemyVicinity.Add(enemyVicinity);
            Sort(ref _enemyVicinity);
        }

        public static void SetEnemyVicinity(List<PathNode> enemyVicinity)
        {
            _enemyVicinity = new List<PathNode>(enemyVicinity);
            Sort(ref _enemyVicinity);
        }

        public static void Sort(ref List<PathNode> list)
        {
            Quicksort.QuicksortFastestPath(ref list, 0, list.Count);
        }
    }
}
