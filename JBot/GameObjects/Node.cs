using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.GameObjects
{
    class Node
    {
        private int shortestDistance;
        private BotTerritory territory;

        public Node(BotTerritory territory)
        {
            this.shortestDistance = Int32.MaxValue;
            this.territory = territory;
        }
    }
}
