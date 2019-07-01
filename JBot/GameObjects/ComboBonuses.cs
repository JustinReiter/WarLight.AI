using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;
using WarLight.Shared.AI.JBot.Evaluation;

namespace WarLight.Shared.AI.JBot.GameObjects
{

    class ComboBonuses
    {
        public List<BotTerritory> adjacentPickTerritories = new List<BotTerritory>();
        public BotBonus mainBonus;
        public BotTerritory mainPick;
        public Boolean isCombo;
        public Boolean isCounterable;
        public Boolean isFTB;
        public List<BotTerritory> supportFTBPick = new List<BotTerritory>();
        public List<BotTerritory> supportComboPick = new List<BotTerritory>();
        public Boolean isEfficient;

        public ComboBonuses(BotBonus mainBonus, BotMap map)
        {
            this.mainBonus = mainBonus;
            mainPick = GetMainBonusPick(mainBonus);
            adjacentPickTerritories.Add(mainPick);
            PopulateAdjacentPickList(map);
            isFTB = IsFirstTurnBonus(mainBonus);
            isCounterable = adjacentPickTerritories.Count > 2 ? true : false;
            isCombo = (isFTB || adjacentPickTerritories.Count > 1) && !IsManyTurnBonus();
            isEfficient = !IsInefficientBonus(mainBonus) && IsEfficientCombo();
            ReorderPicks();
        }

        private void ReorderPicks()
        {
            BotTerritory[] picks = new BotTerritory[adjacentPickTerritories.Count - 1];
            Array.Copy(adjacentPickTerritories.ToArray(), 1, picks, 0, picks.Length);


            if (isFTB)
            {
                ReorderSupportFTBs();
                if (supportFTBPick.Count != 0)
                {
                    for (int i = 0; i < supportFTBPick.Count; i++)
                    {
                        picks[i] = supportFTBPick[i];
                    }
                }
            } else if (isCombo)
            {
                if (supportComboPick.Count != 0)
                {
                    for (int i = 0; i < supportComboPick.Count; i++)
                    {
                        picks[i] = supportComboPick[i];
                    }
                }
            }

            List<BotTerritory> list = new List<BotTerritory>(picks);
            list.Insert(0, mainPick);

            for (int i = 0; i < adjacentPickTerritories.Count; i++)
            {
                if (!list.Contains(adjacentPickTerritories[i]))
                {
                    list.Add(adjacentPickTerritories[i]);
                }
            }

            adjacentPickTerritories = list;
        }

        private void ReorderSupportFTBs()
        {
            
        }

        private void ReorderSupportCombos()
        {

        }

        private void Swap(int pointer, List<BotTerritory> list)
        {
            BotTerritory terr = adjacentPickTerritories[pointer];
            list.Remove(terr);
            list.Add(terr);
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
            if (bonus.Amount != 3 || IsInefficientBonus(bonus) || IsWastelandedBonus(bonus))
            {
                return false;
            }

            IDictionary<BotTerritory, int> pickTerritories = new Dictionary<BotTerritory, int>();
            pickTerritories.Add(adjacentPickTerritories[0], NumberOfBonusTerrNeighbours(adjacentPickTerritories[0], mainBonus));
            if (NumberOfBonusTerrNeighbours(mainPick, mainBonus) == 3)
            {
                foreach (var adjBonusTerr in mainPick.Neighbors)
                {

                    if (ContainsTerritory(bonus, adjBonusTerr))
                    {
                        foreach (var adjTerr in adjBonusTerr.Neighbors)
                        {
                            if (!ContainsTerritory(bonus, adjTerr) && adjTerr.Armies.NumArmies == 0)
                            {
                                if (!pickTerritories.ContainsKey(adjTerr))
                                {
                                    pickTerritories.Add(adjTerr, 0);
                                    supportFTBPick.Add(adjTerr);
                                }
                                pickTerritories[adjTerr]++;
                            }
                        }
                    }
                }

            }
            else
            {
                ArrayList uncoveredTerritories = new ArrayList(Except(mainBonus, mainPick.Neighbors));
                uncoveredTerritories.Remove(mainPick);

                foreach (BotTerritory terr in uncoveredTerritories)
                {
                    foreach (BotTerritory adjTerr in terr.Neighbors)
                    {
                        if (adjTerr.Armies.NumArmies == 0)
                        {
                            if (!pickTerritories.ContainsKey(adjTerr))
                            {
                                pickTerritories.Add(adjTerr, 0);
                            }
                            pickTerritories[adjTerr]++;
                        }
                    }
                }

                foreach (KeyValuePair<BotTerritory, int> pair in pickTerritories)
                {
                    if (pair.Value == uncoveredTerritories.Count)
                    {
                        supportFTBPick.Add(pair.Key);
                    }
                }

            }
            return supportFTBPick.Count > 0;
        }

        private int NumberOfBonusTerrNeighbours(BotTerritory terr, BotBonus bonus)
        {
            int value = 0;
            foreach (var adjTerr in terr.Neighbors)
            {
                if (ContainsTerritory(bonus, adjTerr))
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

        private void PopulateComboSupportPicks()
        {
            if (isFTB)
            {
                return;
            }

            IDictionary<BotTerritory, bool> seenTerritories = new Dictionary<BotTerritory, bool>();
            BotTerritory pick = mainPick;

            foreach (BotTerritory terr in pick.Neighbors)
            {
                if (ContainsTerritory(mainBonus, terr))
                {
                    seenTerritories[terr] = true;

                    foreach (BotTerritory adjTerr in terr.Neighbors)
                    {
                        if (ContainsTerritory(mainBonus, adjTerr))
                        {
                            seenTerritories[adjTerr] = true;
                        }
                    }
                }
            }

            if (Except(mainBonus, seenTerritories.Keys.ToList<BotTerritory>()).Count == 0)
            {
                supportComboPick = new List<BotTerritory>(adjacentPickTerritories);
                supportComboPick.RemoveAt(0);
            }

            foreach (BotTerritory externalPick in adjacentPickTerritories)
            {
                if (externalPick == mainPick)
                {
                    continue;
                }
                IDictionary<BotTerritory, bool> seenTerritoriesCopy = new Dictionary<BotTerritory, bool>();
                foreach (KeyValuePair<BotTerritory, bool> pair in seenTerritories)
                {
                    seenTerritoriesCopy.Add(pair.Key, pair.Value);
                }

                foreach(BotTerritory adjTerr in externalPick.Neighbors)
                {
                    if (ContainsTerritory(mainBonus, adjTerr))
                    {
                        seenTerritoriesCopy.Add(adjTerr, true);
                    }

                    foreach (BotTerritory adjSecondTerr in adjTerr.Neighbors)
                    {
                        if (ContainsTerritory(mainBonus, adjSecondTerr))
                        {
                            seenTerritoriesCopy.Add(adjSecondTerr, true);
                        }
                    }
                }

                if (Except(mainBonus, seenTerritoriesCopy.Keys.ToList<BotTerritory>()).Count == 0)
                {
                    supportComboPick.Add(externalPick);
                }

            }
        }

        private Boolean IsManyTurnBonus()
        {
            IDictionary<BotTerritory, bool> seenTerritories = new Dictionary<BotTerritory, bool>();
            BotTerritory pick = mainPick;

            if (mainPick == null)
            {
                return true;
            }

            foreach (BotTerritory adjTerr in mainPick.Neighbors)
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
                    List<BotTerritory>unseenTerritoriesCopy = new List<BotTerritory>(unseenTerritories);
                    if (terr.Bonuses[0] == mainBonus)
                    {
                        continue;
                    }
                    foreach (BotTerritory adjTerr in terr.Neighbors)
                    {
                        if (unseenTerritoriesCopy.Contains(adjTerr))
                        {
                            unseenTerritoriesCopy.Remove(adjTerr);
                            if (unseenTerritoriesCopy.Count == 0)
                            {
                                return false;
                            }
                        }
                        foreach (BotTerritory adjSecondTerr in adjTerr.Neighbors)
                        {
                            if (unseenTerritoriesCopy.Contains(adjSecondTerr))
                            {
                                unseenTerritoriesCopy.Remove(adjSecondTerr);
                                if (unseenTerritoriesCopy.Count == 0)
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
