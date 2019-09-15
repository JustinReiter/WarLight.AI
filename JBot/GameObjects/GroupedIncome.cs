using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.BasicAlgorithms;

namespace WarLight.Shared.AI.JBot.GameObjects
{
    class GroupedIncome
    {

        List<BonusIDType> _bonuses;
        List<PathNode> _paths;
        int _income;


        public GroupedIncome()
        {
            _bonuses = new List<BonusIDType>();
            _paths = new List<PathNode>();
            _income = 0;
        }

        public void AddBonus(BonusIDType bonus, int armies)
        {
            _bonuses.Add(bonus);
            _income += armies;
        }

        public List<BonusIDType> GetBonuses()
        {
            return _bonuses;
        }

        public void AddPath(PathNode path)
        {
            _paths.Add(path);
            Memory.PathTracker.Sort(ref _paths);
        }

        public List<PathNode> GetPaths()
        {
            return _paths;
        }
    }
}
