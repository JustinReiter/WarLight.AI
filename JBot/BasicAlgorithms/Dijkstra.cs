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

        public static PathNode ShortestPath(BotMap map, BotTerritory source, BotTerritory destination)
        {
            List<PathNode> seenTerr = new List<PathNode>();
            PathVector unseenTerr = new PathVector();

            foreach (BotTerritory terr in map.Territories.Values)
            {
                if (terr.ID == source.ID)
                {
                    unseenTerr.Insert(0, new PathNode(terr));
                    unseenTerr.nodes[0].minDistance = 0;
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
                    int numArmies = map.Territories[terrId].Armies.NumArmies;
                    if (numArmies == 0)
                    {
                        numArmies = 4;
                    } else if (numArmies == 10)
                    {
                        numArmies = 20;
                    }
                    if (unseenTerr.Contains(terrId) && unseenTerr.GetNode(terrId).minDistance > (pointer.minDistance + numArmies))
                    {
                        PathNode temp = unseenTerr.GetNode(terrId);
                        temp.minDistance = pointer.minDistance + numArmies;
                        temp.minPath = new List<TerritoryIDType>(pointer.minPath);
                        temp.minPath.Add(pointer.territory);
                    }
                }
                seenTerr.Add(unseenTerr.Remove(pointer.territory));
                Quicksort.QuicksortPath(ref unseenTerr, 0, unseenTerr.nodes.Count);
            }

            return seenTerr.Last();
        }

        public static PathNode ShortestPath(BotMap map, List<BotTerritory> sources, BotTerritory destination)
        {
            PathNode bestPath = ShortestPath(map, sources[0], destination);
            foreach (BotTerritory source in sources)
            {
                PathNode temp = ShortestPath(map, source, destination);
                bestPath = bestPath.minDistance < temp.minDistance ? bestPath : temp;
            }
            return bestPath;
        }
    }
}
