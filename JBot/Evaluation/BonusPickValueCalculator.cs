using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.JBot.Bot;
using WarLight.Shared.AI.JBot.GameObjects;

namespace WarLight.Shared.AI.JBot.Evaluation
{
    /// <summary>
    /// This class is responsible for finding out which picks to choose based on an algorithm dominantly focusing on FTBs and combos.
    /// </summary>
    /// <remarks>
    /// This class is responsible for finding out which picks to choose. This happens by giving all Bonuses
    /// values. Furthermore this class is used during picking stage.
    /// </remarks>
    public class BonusPickValueCalculator
    {
        public BotMain BotState;

        public object MainLoop { get; private set; }

        public BonusPickValueCalculator(BotMain state)
        {
            this.BotState = state;
        }

        /// <summary>
        ///     Determines the score for different bonuses based on factors such as efficiency, wastelands, pick-conditions, locations
        /// </summary>
        /// <param name="bonus"></param>
        /// <param name="useNeighborBonusFactor"></param>
        /// <returns></returns>
        public double GetExpansionValue(BotBonus bonus, Boolean useNeighborBonusFactor)
        {
            double expansionValue = GetInefficientWastelandedBonusFactor(bonus);
            Boolean isFirstTurnBonus = IsFirstTurnBonus(bonus);

            if (IsExpansionWorthless(bonus))
            {
                return expansionValue;
            }

            if (bonus.Details.Name.Equals("Caucasus") || bonus.Details.Name.Equals("West China"))
            {
                expansionValue -= 50;
            }

            if (IsManyTurnBonus(bonus))
            {
                expansionValue -= 15;
            }

            if (bonus.Details.Name.Equals("Greenland"))
            {
                foreach (BotTerritory terr in bonus.Territories)
                {
                    if (terr.Details.Name.Equals("Nord") || terr.Details.Name.Equals("Itseqqortoormiit"))
                    {
                        expansionValue -= 10;
                    }
                }
            }

            // Deduct if pick is between all territories (CA and Ant)
            if (bonus.Details.Name.Equals("Central America"))
            {
                foreach (BotTerritory terr in bonus.Territories)
                {
                    if (terr.Details.Name.Equals("Mexico") && terr.Armies.NumArmies == 0)
                    {
                        expansionValue -= 8;
                    }
                }
            } else if (bonus.Details.Name.Equals("Antarctica"))
            {
                foreach (BotTerritory terr in bonus.Territories)
                {
                    if (terr.Details.Name.Equals("South Pole") && terr.Armies.NumArmies == 0)
                    {
                        expansionValue -= 8;
                    }
                }
            }

            // Add modifier to prioritize smaller bonus amounts (+20 for 3er, +15 for 4er, +10 for 5er)
            expansionValue += (7 - bonus.Amount) * 5;
            return expansionValue;
        }


        private bool IsExpansionWorthless(BotBonus bonus)
        {
            if (bonus.Amount <= 0)
            {
                return true;
            }

            if (bonus.ContainsOpponentPresence())
            {
                return true;
            }

            if (bonus.IsOwnedByMyself() && BotState.NumberOfTurns != -1)
            {
                return true;
            }

            return false;
        }

        private double GetInefficientWastelandedBonusFactor(BotBonus bonus)
        {
            // Checks bonus for inefficient and wastelanded territories
            double value = 0.0;
            if (IsWastelandedBonus(bonus))
            {
                value -= 100;
            }

            return IsInefficientBonus(bonus) ? value - 50 : value;
        }

        public Boolean IsFirstTurnBonus(BotBonus bonus)
        {
            Boolean isFirstTurnBonus = false;

            if (bonus.Amount != 3 || IsInefficientBonus(bonus) || IsWastelandedBonus(bonus))
            {
                return isFirstTurnBonus;
            }

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
                                if (!ContainsTerritory(bonus, adjTerr) && adjTerr.Armies.NumArmies == 0 && !IsInefficientBonus(adjTerr.Bonuses[0]) && IsWastelandedBonus(adjTerr.Bonuses[0]))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                } else
                {
                    ArrayList coveredTerritories = new ArrayList();

                    foreach (var adjTerr in terr.Neighbors)
                    {
                        if (ContainsTerritory(bonus, adjTerr))
                        {
                            coveredTerritories.Add(adjTerr);
                        }
                    }

                    foreach (var bonusTerr in bonus.Territories)
                    {
                        if (!coveredTerritories.Contains(bonusTerr) && bonusTerr.Armies.NumArmies != 0)
                        {
                            foreach (var adjTerr in bonusTerr.Neighbors)
                            {
                                if (adjTerr.Armies.NumArmies == 0 && !IsInefficientBonus(adjTerr.Bonuses[0]) && !IsWastelandedBonus(adjTerr.Bonuses[0]))
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

        public Boolean IsInefficientBonus(BotBonus bonus)
        {
            return bonus.Territories.Count != bonus.Amount + 1;
        }

        public Boolean IsWastelandedBonus(BotBonus bonus)
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


        private int GetCounteredTerritories(BotBonus bonus, PlayerIDType playerID)
        {
            var outvar = 0;
            foreach (var territory in bonus.Territories)
            {
                if (territory.GetOpponentNeighbors().Count > 0 && playerID == BotState.Me.ID)
                    outvar++;
                else if (territory.GetOwnedNeighbors().Count > 0 && BotState.IsOpponent(playerID))
                    outvar++;
            }
            return outvar;
        }

        public bool IsComboBonus(BotBonus bonus, BotMap map)
        {
            if (bonus.Amount > 4 || IsInefficientBonus(bonus) || IsWastelandedBonus(bonus))
            {
                return false;
            }

            ComboBonuses temp = new ComboBonuses(bonus, map);
            return temp.isCombo;
        }

        private Boolean IsManyTurnBonus(BotBonus bonus, ComboBonuses optionalCombo = null)
        {
            IDictionary<BotTerritory, bool> seenTerritories = new Dictionary<BotTerritory, bool>();
            BotTerritory pick = null;
            foreach (BotTerritory terr in bonus.Territories)
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
                if (!ContainsTerritory(bonus, adjTerr))
                {
                    continue;
                }
                seenTerritories[adjTerr] = true;
                foreach (BotTerritory adjSecondTerr in adjTerr.Neighbors)
                {
                    if (!ContainsTerritory(bonus, adjSecondTerr))
                    {
                        continue;
                    }
                    seenTerritories[adjSecondTerr] = true;
                }
            }

            if (seenTerritories.Count == bonus.Territories.Count)
            {
                return false;
            } else if (optionalCombo != null)
            {
                List<BotTerritory> unseenTerritories = Except(bonus, seenTerritories.Keys.ToList<BotTerritory>());
                foreach (BotTerritory terr in optionalCombo.adjacentPickTerritories)
                {
                    if (terr.Bonuses[0] == bonus)
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

        private List<BotTerritory> Except (BotBonus bonus, List<BotTerritory> seenTerritories)
        {
            List<BotTerritory> UnseenTerritories = new List<BotTerritory>(bonus.Territories);
            foreach (BotTerritory terr in seenTerritories)
            {
                UnseenTerritories.Remove(terr);
            }
            return UnseenTerritories;
        }
    }
}
