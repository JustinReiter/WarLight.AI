using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.BasicAlgorithms
{
    static class Dijkstra
    {

        public static PathNode ShortestPath(BotMain bot, BotTerritory source, BotTerritory destination)
        {
            List<PathNode> seenTerr = new List<PathNode>();
            PathVector unseenTerr = new PathVector();

            foreach (BotTerritory terr in bot.VisibleMap.Territories.Values)
            {
                if (terr == source)
                {
                    unseenTerr.Insert(0, new PathNode(terr));
                }
                else
                {
                    unseenTerr.Add(new PathNode(terr));
                }
            }

            while (unseenTerr.Contains(destination.ID))
            {
                PathNode pointer = unseenTerr.nodes[0];
                foreach (TerritoryIDType terrId in pointer.adjacent)
                {
                    if (unseenTerr.Contains(terrId) && unseenTerr.GetNode(terrId).minDistance > (pointer.minDistance + bot.VisibleMap.Territories[terrId].Armies.NumArmies))
                    {
                        PathNode temp = unseenTerr.GetNode(terrId);
                        temp.minDistance = pointer.minDistance + bot.VisibleMap.Territories[terrId].Armies.NumArmies;
                        temp.minPath = pointer.minPath;
                        temp.minPath.Add(pointer.territory);
                    }
                }
                seenTerr.Add(unseenTerr.Remove(pointer.territory));
                Quicksort.QuicksortPath(ref unseenTerr, 0, unseenTerr.nodes.Count);
            }

            return seenTerr.Last();
        }

        public static PathNode ShortestPath(BotMain bot, List<BotTerritory> sources, BotTerritory destination)
        {
            PathNode bestPath = ShortestPath(bot, sources[0], destination);
            foreach (BotTerritory source in sources)
            {
                PathNode temp = ShortestPath(bot, source, destination);
                bestPath = bestPath.minDistance < temp.minDistance ? bestPath : temp;
            }
            return bestPath;
        }
    }
}
