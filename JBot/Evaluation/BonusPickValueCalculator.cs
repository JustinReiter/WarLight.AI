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
        private readonly double TerritoryMultiplicator = 0.9;
        private readonly double NeutralKillsMultiplicator = 1.0;
        private readonly double NeutralsMultiplicator = 0.5;

        public BotMain BotState;

        public object MainLoop { get; private set; }

        public BonusPickValueCalculator(BotMain state)
        {
            this.BotState = state;
        }

        /// <summary>Classifies the Bonus according to the intel from the temporaryMap.
        /// </summary>
        /// <remarks>
        /// Classifies the Bonus according to the intel from the temporaryMap. However the results of the classification aren't written to the temporary map but to the visible map.
        /// </remarks>
        /// <param name="temporaryMap"></param>
        public void ClassifyBonuses(BotMap temporaryMap, BotMap mapToWriteIn)
        {
            foreach (var bonus in temporaryMap.Bonuses.Values)
            {
                bonus.SetMyExpansionValueHeuristic();

                // Categorize the expansion values. Possible values are 0 = rubbish and 1 = good
                var toMuchNeutrals = false;
                var neutralArmies = bonus.NeutralArmies.DefensePower;
                if (neutralArmies > 28)
                    toMuchNeutrals = true;
                else if (neutralArmies >= 4 * (bonus.Amount + 2))
                    toMuchNeutrals = true;

                if (bonus.IsOwnedByMyself() || bonus.Amount == 0 || bonus.ContainsOpponentPresence() || bonus.ContainsTeammatePresence() || toMuchNeutrals)
                    mapToWriteIn.Bonuses[bonus.ID].ExpansionValueCategory = 0;
                else
                    mapToWriteIn.Bonuses[bonus.ID].ExpansionValueCategory = 1;
            }
        }


        private double GetIncomeCostsRatio(BotBonus bonus)
        {
            var income = (double)bonus.Amount;
            var neutrals = (double)bonus.NeutralArmies.DefensePower;
            neutrals += bonus.Territories.Count(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution) * BotState.Settings.InitialNeutralsInDistribution;

            int neutralKills = 0;
            foreach (BotTerritory territory in bonus.NeutralTerritories.Where(o => o.Armies.Fogged == false))
            {
                neutralKills += (int)Math.Round(territory.Armies.DefensePower * BotState.Settings.DefenseKillRate);
            }

            int territories = bonus.Territories.Count;

            double adjustedTerritoryFactor = territories * TerritoryMultiplicator;
            double adjustedNeutralKillsFactor = neutralKills * NeutralKillsMultiplicator;
            double adjustedNeutralsFactor = neutrals * NeutralsMultiplicator;

            return income * 100000 / (adjustedTerritoryFactor + adjustedNeutralKillsFactor + adjustedNeutralsFactor);
        }

        private double GetNeutralArmiesFactor(int neutralArmies)
        {
            double factor = 0.0;
            factor = neutralArmies * 0.01;
            factor = Math.Min(factor, 0.2);
            return factor;
        }

        private double GetTerritoryFactor(int territories)
        {
            double factor = 0.0;
            factor = territories * 0.01;
            factor = Math.Min(factor, 0.05);
            return factor;
        }

        private double GetImmediatelyCounteredTerritoryFactor(int immediatelyCounteredTerritories)
        {
            double factor = 0.0;
            factor = immediatelyCounteredTerritories * 0.1;
            factor = Math.Min(factor, 0.2);
            return factor;
        }

        private double GetAllCounteredTerritoryFactor(int allCounteredTerritories)
        {
            double factor = 0.0;
            factor = allCounteredTerritories * 0.1;
            factor = Math.Min(factor, 0.2);
            return factor;
        }

        private double GetOpponentInNeighborBonusFactor(int amountNeighborBonuses)
        {
            double factor = 0.0;
            if (amountNeighborBonuses > 0)
            {
                factor = 0.1;
            }
            return factor;
        }

        // positive then bad, negative then good
        private double GetNeighborBonusesFactor(BotBonus bonus)
        {
            if (BotState.NumberOfTurns != -1)
            {
                return 0;
            }
            List<BotBonus> betterSmallerBonuses = new List<BotBonus>();
            List<BotBonus> betterLargerBonuses = new List<BotBonus>();
            double neighborBonusValueToleranceFactor = 1.3;
            double betterBiggerBonusesMultiplicator = 0.001;
            double betterSmallerBonusesMultiplicator = 0.01;

            List<BotBonus> neighborBonuses = bonus.GetNeighborBonuses();
            double ourValue = GetExpansionValue(bonus, false);
            foreach (BotBonus neighborBonus in neighborBonuses)
            {
                if (neighborBonus.Territories.Count(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution) == 0)
                {
                    continue;
                }
                double neighborBonusValue = GetExpansionValue(neighborBonus, false);
                neighborBonusValue = neighborBonusValue * neighborBonusValueToleranceFactor;
                if (neighborBonusValue >= ourValue && bonus.Territories.Count > neighborBonus.Territories.Count)
                {
                    betterSmallerBonuses.Add(bonus);
                }
                else if (neighborBonusValue >= ourValue && bonus.Territories.Count < neighborBonus.Territories.Count)
                {
                    betterLargerBonuses.Add(bonus);
                }
            }
            int amountBetterLittleSmallerBonuses = 0;
            int amountBetterVerySmallerBonuses = 0;
            foreach (BotBonus smallerNeighbor in betterSmallerBonuses)
            {
                if (smallerNeighbor.Territories.Count <= bonus.Territories.Count / 2)
                {
                    amountBetterVerySmallerBonuses++;
                }
                else
                {
                    amountBetterLittleSmallerBonuses++;
                }
            }
            double adjustedFactor =
                (betterBiggerBonusesMultiplicator * betterLargerBonuses.Count - betterSmallerBonusesMultiplicator * amountBetterLittleSmallerBonuses - betterSmallerBonusesMultiplicator
             * 2 * amountBetterVerySmallerBonuses);
            adjustedFactor = Math.Max(-0.2, Math.Min(0.2, adjustedFactor));

            return -1 * adjustedFactor;
        }

        private double getBorderTerritoriesFactor(BotBonus bonus)
        {
            double pickingStageBorderTerritoryMultiplicator = 0.01;
            double inGameBorderTerritoryMultiplicator = 0.005;
            int amountBorderTerritories = 0;
            foreach (BotTerritory territory in bonus.Territories)
            {
                foreach (BotTerritory neighbor in territory.Neighbors)
                {
                    if (neighbor.Bonuses.Count == 0 || neighbor.Bonuses[0] != bonus)
                    {
                        amountBorderTerritories++;
                        break;
                    }
                }
            }
            double adjustedFactor = 0;
            if (BotState.NumberOfTurns == -1)
            {
                adjustedFactor = pickingStageBorderTerritoryMultiplicator * amountBorderTerritories;
            }
            else
            {
                adjustedFactor = inGameBorderTerritoryMultiplicator * amountBorderTerritories;
            }
            adjustedFactor = Math.Min(0.1, adjustedFactor);
            return adjustedFactor;
        }


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

            // Add modifier to prioritize smaller bonus amounts (+20 for 3er, +15 for 4er, +10 for 5er
            expansionValue += (7 - bonus.Amount) * 5;

            //expansionValue = GetIncomeCostsRatio(bonus);

            //var neutralArmies = bonus.NeutralArmies.DefensePower;
            //double neutralArmiesFactor = GetNeutralArmiesFactor(neutralArmies);

            //int allTerritories = bonus.Territories.Count;
            //double territoryFactor = GetTerritoryFactor(allTerritories);


            //int immediatelyCounteredTerritories = bonus.GetOwnedTerritoriesBorderingOpponent().Count;
            //double immediatelyCounteredTerritoriesFactor = GetImmediatelyCounteredTerritoryFactor(immediatelyCounteredTerritories);

            //var allCounteredTerritories = GetCounteredTerritories(bonus, BotState.Me.ID);
            //double allCounteredTerritoriesFactor = GetAllCounteredTerritoryFactor(allCounteredTerritories);

            //int amountNeighborBonusesWithOpponent = 0;
            //var neighborBonuses = bonus.GetNeighborBonuses();
            //foreach (var neighborBonus in neighborBonuses)
            //{
            //    if (neighborBonus.ContainsOpponentPresence())
            //    {
            //        amountNeighborBonusesWithOpponent++;
            //    }
            //}
            //double opponentNeighborBonusFactor = GetOpponentInNeighborBonusFactor(amountNeighborBonusesWithOpponent);
            //double borderTerritoriesFactor = getBorderTerritoriesFactor(bonus);

            //double completeFactor = neutralArmiesFactor + territoryFactor + immediatelyCounteredTerritoriesFactor + allCounteredTerritoriesFactor + opponentNeighborBonusFactor + borderTerritoriesFactor;
            //if (useNeighborBonusFactor)
            //{
            //    completeFactor += GetNeighborBonusesFactor(bonus);
            //}
            //completeFactor = Math.Min(completeFactor, 0.8);

            //expansionValue = expansionValue - (expansionValue * completeFactor);
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

        public bool IsComboBonus(BotBonus bonus)
        {

            return true;
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
