using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Evaluation;
using WarLight.Shared.AI.JBot.GameObjects;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class RegionTracker
    {
        public static List<GroupedIncome> enemyRegion = new List<GroupedIncome>();

        public static void SetEnemyRegion(List<BonusIDType> list)
        {
            GroupedIncome group = new GroupedIncome();
            group.SetBonuses(list);
            enemyRegion.Add(group);
        }
    }
}
