using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.GameObjects
{
    class NaryTree
    {
        private Node root;

        public NaryTree(BotTerritory root)
        {
            this.root = new Node(root);
        }


    }
}
