using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.GameObjects
{

    class ComboBonuses
    {
        public ArrayList adjacentPickTerritories = new ArrayList();
        public BotBonus mainBonus;
        public Boolean isCounterable;
        public Boolean isFTB;
        public Boolean isEfficient;

        public ComboBonuses(BotBonus mainBonus, BotMap map)
        {
            this.mainBonus = mainBonus;
            adjacentPickTerritories.Add(GetMainBonusPick(mainBonus));
            PopulateAdjacentPickList(map);
            isFTB = IsFirstTurnBonus(mainBonus);
            isCounterable = adjacentPickTerritories.Count > 2 ? true : false;
            isEfficient = !IsInefficientTerritory(mainBonus) && IsEfficientCombo();

        }

        private void PopulateAdjacentPickList(BotMap map)
        {
            foreach (var terr in mainBonus.Territories)
            {
                foreach (var adjTerr in terr.Neighbors)
                {
                    if (!ContainsTerritory(mainBonus, adjTerr) && adjTerr.Armies.NumArmies == 0)
                    {
                        adjacentPickTerritories.Add(adjTerr.Bonuses[0]);
                    }
                }
            }
        }

        private BotTerritory GetMainBonusPick(BotBonus bonus)
        {
            BotTerritory territory = null;
            foreach (var terr in bonus.Territories)
            {
                if (terr.Armies.NumArmies == 0)
                {
                    territory = terr;
                }
            }
            return territory;
        }

        private Boolean getCounterable()
        {
            return adjacentPickTerritories.Count > 2 ? true : false;
        }

        private Boolean ContainsTerritory(BotBonus bonus, BotTerritory territory)
        {
            foreach (var terr in bonus.Territories)
            {
                if (terr.Equals(territory))
                {
                    return true;
                }
            }
            return false;
        }

        public Boolean IsFirstTurnBonus(BotBonus bonus)
        {
            Boolean isFirstTurnBonus = false;

            if (bonus.Amount != 3 || IsInefficientTerritory(bonus) || IsWastelandedTerritory(bonus))
            {
                return isFirstTurnBonus;
            }

            IDictionary<TerritoryIDType, int> pickTerritories = new Dictionary<TerritoryIDType, int>();
            foreach (var terr in bonus.Territories)
            {
                if (terr.Armies.NumArmies != 0)
                {
                    continue;
                }

                if (terr.Neighbors.Count == 3)
                {
                    foreach (var adjBonusTerr in terr.Neighbors)
                    {

                        if (ContainsTerritory(bonus, adjBonusTerr))
                        {
                            foreach (var adjTerr in adjBonusTerr.Neighbors)
                            {
                                if (!ContainsTerritory(bonus, adjTerr) && adjTerr.Armies.NumArmies == 0)
                                {
                                    isFirstTurnBonus = true;
                                    pickTerritories[adjTerr.ID]++;
                                }
                            }

                            isFirstTurnBonus = true;
                        }
                    }

                }
                else
                {
                    ArrayList coveredTerritories = new ArrayList();

                    foreach (var adjTerr in terr.Neighbors)
                    {
                        coveredTerritories.Add(adjTerr);
                    }

                    foreach (var bonusTerr in bonus.Territories)
                    {
                        if (!coveredTerritories.Contains(bonusTerr))
                        {
                            foreach (var adjTerr in bonusTerr.Neighbors)
                            {
                                if (adjTerr.Armies.NumArmies == 0)
                                {
                                    goto CONTINUELOOP;
                                }
                            }
                            return isFirstTurnBonus;
                        }
                    CONTINUELOOP:;
                    }
                    isFirstTurnBonus = true;
                }
            }
            return isFirstTurnBonus;
        }

        private Boolean IsInefficientTerritory(BotBonus bonus)
        {
            return bonus.Territories.Count != bonus.Amount + 1;
        }

        private Boolean IsWastelandedTerritory(BotBonus bonus)
        {
            foreach (var terr in bonus.Territories)
            {
                if (terr.Armies.NumArmies > 2)
                {
                    return true;
                }
            }
            return false;
        }

        private Boolean IsEfficientCombo()
        {
            IDictionary<BotBonus, bool> bonuses = new Dictionary<BotBonus, bool>();
            foreach (BotTerritory picks in adjacentPickTerritories)
            {
                bonuses[picks.Bonuses[0]] = true;
            }
            bool externalPickEfficient = false;
            foreach (KeyValuePair<BotBonus, bool> val in bonuses)
            {
                externalPickEfficient = !IsInefficientTerritory(val.Key) ? true : externalPickEfficient;
            }
            return externalPickEfficient;
        }
    }
}
