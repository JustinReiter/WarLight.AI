using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.JBot.BasicAlgorithms
{
    class PathVector
    {
        public List<PathNode> nodes;

        public PathVector()
        {
            nodes = new List<PathNode>();
        }

        public void Add(PathNode node)
        {
            nodes.Add(node);
        }

        public void Insert(int index, PathNode node)
        {
            nodes.Insert(index, node);
        }

        public PathNode Remove(TerritoryIDType terrId)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].territory == terrId)
                {
                    PathNode removedNode = nodes[i];
                    nodes.RemoveAt(i);
                    return removedNode;
                }
            }
            return null;
        }

        public PathNode GetNode(TerritoryIDType terrId)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].territory == terrId)
                {
                    return nodes[i];
                }
            }
            return null;
        }

        public bool Contains(TerritoryIDType terrId)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].territory == terrId)
                {
                    return true;
                }
            }
            return false;
        }

        public PathNode Last()
        {
            return nodes.Last();
        }
    }
}
