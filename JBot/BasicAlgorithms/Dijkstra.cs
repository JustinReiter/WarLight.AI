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

        public static PathNode DijkstrasAlgorithm(BotMap map, BotTerritory source, List<BotTerritory> destinations)
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

            while (DoesVectorHaveAllDestinations(unseenTerr, destinations))
            {
                PathNode pointer = unseenTerr.nodes[0];
                foreach (TerritoryIDType terrId in pointer.adjacent)
                {
                    int numArmies = map.Territories[terrId].Armies.NumArmies;
                    if (numArmies == 0)
                    {
                        numArmies = 4;
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

        private static bool DoesVectorHaveAllDestinations(PathVector vector, List<BotTerritory> territories)
        {
            foreach (BotTerritory terr in territories)
            {
                if (!vector.Contains(terr.ID))
                {
                    return false;
                }
            }
            return true;
        }

        public static PathNode ShortestPath(BotMap map, dynamic source, dynamic destination)
        {
            List<BotTerritory> sourceTerrList = new List<BotTerritory>();
            List<BotTerritory> destTerrList = new List<BotTerritory>();

            // Convert source object into list of source territories (Accepted input types: BotTerritory/BotBonus/List<BotTerritory)
            if (source is BotTerritory)
            {
                sourceTerrList.Add(source);
            } else if (source is BotBonus)
            {
                sourceTerrList.AddRange(source.Territories);
            } else if (source is List<BotTerritory>)
            {
                sourceTerrList.AddRange(source);
            } else
            {
                throw new Exception("Unexpected source type thrown to Dijkstra's shortest path");
            }

            // Convert destination object into list of destination territories (Accepted input types: BotTerritory/BotBonus/List<BotTerritory)
            if (destination is BotTerritory)
            {
                destTerrList.Add(destination);
            }
            else if (destination is BotBonus)
            {
                destTerrList.AddRange(destination.Territories);
            }
            else if (destination is List<BotTerritory>)
            {
                destTerrList.AddRange(destination);
            }
            else
            {
                throw new Exception("Unexpected destination type thrown to Dijkstra's shortest path");
            }

            // Iterate through each source territory to find shortest path
            PathNode bestPath = DijkstrasAlgorithm(map, sourceTerrList[0], destTerrList);
            foreach (BotTerritory sourceTerr in sourceTerrList)
            {
                PathNode temp = DijkstrasAlgorithm(map, sourceTerr, destTerrList);
                bestPath = bestPath.minDistance < temp.minDistance ? bestPath : temp;
            }
            return bestPath;
        }
    }
}
