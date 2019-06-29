﻿using System;
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
        public List<BotTerritory> adjacentPickTerritories = new List<BotTerritory>();
        public BotBonus mainBonus;
        public Boolean isCombo;
        public Boolean isCounterable;
        public Boolean isFTB;
        public Boolean isEfficient;

        public ComboBonuses(BotBonus mainBonus, BotMap map)
        {
            this.mainBonus = mainBonus;
            adjacentPickTerritories.Add(GetMainBonusPick(mainBonus));
            PopulateAdjacentPickList(map);
            ReorderEfficientPicks();
            isFTB = IsFirstTurnBonus(mainBonus);
            isCounterable = adjacentPickTerritories.Count > 2 ? true : false;
            isCombo = (isFTB || adjacentPickTerritories.Count > 1) && !IsManyTurnBonus();
            isEfficient = !IsInefficientBonus(mainBonus) && IsEfficientCombo();
        }

        private void ReorderEfficientPicks()
        {
            int pointer = 1;

            IDictionary<BotTerritory, bool> iterated = new Dictionary<BotTerritory, bool>();
            while (pointer < adjacentPickTerritories.Count)
            {
                if (iterated.ContainsKey(adjacentPickTerritories[pointer]) || !IsInefficientBonus(adjacentPickTerritories[pointer].Bonuses[0]))
                {
                    pointer++;
                } else
                {
                    iterated.Add(adjacentPickTerritories[pointer], true);
                    Swap(pointer);
                }
            }
        }

        private void Swap(int pointer)
        {
            BotTerritory terr = adjacentPickTerritories[pointer];
            adjacentPickTerritories.Remove(terr);
            adjacentPickTerritories.Add(terr);
        }

        private void PopulateAdjacentPickList(BotMap map)
        {
            foreach (var terr in mainBonus.Territories)
            {
                foreach (var adjTerr in terr.Neighbors)
                {
                    if (!ContainsTerritory(mainBonus, adjTerr) && adjTerr.Armies.NumArmies == 0)
                    {
                        adjacentPickTerritories.Add(adjTerr);
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

            if (bonus.Amount != 3 || IsInefficientBonus(bonus) || IsWastelandedBonus(bonus))
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

                if (NumberOfBonusTerrNeighbours(terr) == 3)
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

                                    if (!pickTerritories.ContainsKey(adjTerr.ID))
                                    {
                                        pickTerritories.Add(adjTerr.ID, 0);
                                    }
                                    pickTerritories[adjTerr.ID]++;
                                }
                            }
                        }
                    }

                }
                else
                {
                    ArrayList coveredTerritories = new ArrayList(terr.Neighbors);

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

        private int NumberOfBonusTerrNeighbours(BotTerritory terr)
        {
            int value = 0;
            foreach (var adjTerr in terr.Neighbors)
            {
                if (ContainsTerritory(terr.Bonuses[0], adjTerr))
                {
                    value++;
                }
            }
            return value;
        }

        private Boolean IsInefficientBonus(BotBonus bonus)
        {
            return bonus.Territories.Count != bonus.Amount + 1;
        }

        private Boolean IsWastelandedBonus(BotBonus bonus)
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
                externalPickEfficient = !IsInefficientBonus(val.Key) ? true : externalPickEfficient;
            }
            return externalPickEfficient;
        }

        private Boolean IsManyTurnBonus()
        {
            IDictionary<BotTerritory, bool> seenTerritories = new Dictionary<BotTerritory, bool>();
            BotTerritory pick = null;
            foreach (BotTerritory terr in mainBonus.Territories)
            {
                if (terr.Armies.NumArmies == 0)
                {
                    seenTerritories[terr] = true;
                    pick = terr;
                }
            }

            if (pick == null)
            {
                return true;
            }

            foreach (BotTerritory adjTerr in pick.Neighbors)
            {
                if (!ContainsTerritory(mainBonus, adjTerr))
                {
                    continue;
                }
                seenTerritories[adjTerr] = true;
                foreach (BotTerritory adjSecondTerr in adjTerr.Neighbors)
                {
                    if (!ContainsTerritory(mainBonus, adjSecondTerr))
                    {
                        continue;
                    }
                    seenTerritories[adjSecondTerr] = true;
                }
            }

            if (seenTerritories.Count == mainBonus.Territories.Count)
            {
                return false;
            }
            else if (adjacentPickTerritories.Count > 1)
            {
                List<BotTerritory> unseenTerritories = Except(mainBonus, seenTerritories.Keys.ToList<BotTerritory>());
                foreach (BotTerritory terr in adjacentPickTerritories)
                {
                    if (terr.Bonuses[0] == mainBonus)
                    {
                        continue;
                    }
                    foreach (BotTerritory adjTerr in terr.Neighbors)
                    {
                        if (unseenTerritories.Contains(adjTerr))
                        {
                            unseenTerritories.Remove(adjTerr);
                            if (unseenTerritories.Count == 0)
                            {
                                return false;
                            }
                        }
                        foreach (BotTerritory adjSecondTerr in adjTerr.Neighbors)
                        {
                            if (unseenTerritories.Contains(adjSecondTerr))
                            {
                                unseenTerritories.Remove(adjSecondTerr);
                                if (unseenTerritories.Count == 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private List<BotTerritory> Except(BotBonus bonus, List<BotTerritory> seenTerritories)
        {
            List<BotTerritory> UnseenTerritories = new List<BotTerritory>(bonus.Territories);
            foreach (BotTerritory terr in seenTerritories)
            {
                UnseenTerritories.Remove(terr);
            }
            return UnseenTerritories;
        }

        private Boolean ContainsTerritory(BotTerritory territory)
        {
            foreach (var terr in mainBonus.Territories)
            {
                if (terr.Equals(territory))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
