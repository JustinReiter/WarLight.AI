﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.BasicAlgorithms
{
    class PathNode
    {

        public TerritoryIDType territory;
        public List<TerritoryIDType> minPath;
        public int minDistance;
        public List<TerritoryIDType> adjacent;

        public PathNode(BotTerritory terr)
        {
            territory = terr.ID;
            minPath = new List<TerritoryIDType>();
            minDistance = int.MaxValue;
            SetAdjacentTerrId(terr);
        }

        private void SetAdjacentTerrId(BotTerritory terr)
        {
            foreach (BotTerritory adjTerr in terr.Neighbors)
            {
                adjacent.Add(adjTerr.ID);
            }
        }
    }
}
