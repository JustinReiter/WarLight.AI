using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.BasicAlgorithms;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Evaluation
{
    public class BonusRegionEvaluator
    {
        public BotMain BotState;
        public BonusRegionEvaluator(BotMain state)
        {
            this.BotState = state;
        }

        public double GetRegionExpansionValue()
        {

        }

        public double GetRegionSafetyValue()
        {

        }

        public int GetAdjacentMultiBorders(BotBonus bonus)
        {
            int multiBorders = 0;
            List<TerritoryIDType> uniqueTerr = new List<TerritoryIDType>();
            foreach (BotTerritory terr in bonus.Territories)
            {
                foreach (BotTerritory adjTerr in terr.Neighbors)
                {
                    if (adjTerr.ID == terr.ID && !uniqueTerr.Contains(adjTerr.ID))
                    {
                        uniqueTerr.Add(adjTerr.ID);
                    } else
                    {
                        multiBorders++;
                    }
                }
            }
            return multiBorders;
        }

        public int GetAdjacentBonuses(BotBonus bonus)
        {
            List<BonusIDType> uniqueBonuses = new List<BonusIDType>();
            foreach (BotTerritory terr in bonus.Territories)
            {
                foreach (BotTerritory adjTerr in terr.Neighbors)
                {
                    if (adjTerr.Bonuses[0].ID == terr.Bonuses[0].ID && !uniqueBonuses.Contains(adjTerr.Bonuses[0].ID))
                    {
                        uniqueBonuses.Add(adjTerr.Bonuses[0].ID);
                    }
                }
            }
            return uniqueBonuses.Count;
        }

        public bool IsWasteland(BotBonus bonus)
        {
            foreach (BotTerritory terr in bonus.Territories)
            {
                if (terr.Armies.NumArmies > 4)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInefficient(BotBonus bonus)
        {
            return bonus.Territories.Count > bonus.Amount + 1 ? true : false;
        }

        /// <summary>
        /// Returns true if the enemy picks are either (one-off) adjacent, or area "close" path-wise.
        /// </summary>
        /// <param name="node">Node with Source/Dest as enemy picks</param>
        /// <returns></returns>
        // TODO Refine arbitrary numbers
        public bool IsEnemyMainRegion(PathNode node)
        {
            int count = GetEnemyMainRegion(node).Count;
            return count > 0 && (count < 4 || node.minPath.Count < 6);
        }

        public List<BonusIDType> GetEnemyMainRegion(PathNode node)
        {
            List<BonusIDType> uniqueBonuses = new List<BonusIDType>();
            foreach (TerritoryIDType terrId in node.minPath)
            {
                BotBonus bonus = BotState.VisibleMap.Territories[terrId].Bonuses[0];
                if (!uniqueBonuses.Contains(bonus.ID) || !IsWasteland(bonus) || !IsInefficient(bonus))
                {
                    uniqueBonuses.Add(bonus.ID);
                }
                else if (IsWasteland(bonus) || IsInefficient(bonus))
                {
                    uniqueBonuses.Clear();
                    return uniqueBonuses;
                }
            }
            return uniqueBonuses;
        }
    }
}
