using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.JBot.Bot;
using WarLight.Shared.AI.JBot.GameObjects;

namespace WarLight.Shared.AI.JBot.Evaluation
{
    public class PicksEvaluator
    {
        public BotMain BotState;
        public PicksEvaluator(BotMain state)
        {
            this.BotState = state;
        }

        public List<TerritoryIDType> GetPicks()
        {
            if (BotState.Map.IsScenarioDistribution(BotState.Settings.DistributionModeID))
            {
                var us = BotState.Map.GetTerritoriesForScenario(BotState.Settings.DistributionModeID, BotState.Me.ScenarioID);
                us.RandomizeOrder();
                return us;
            }


            int maxPicks = BotState.Settings.LimitDistributionTerritories == 0 ? BotState.Map.Territories.Count : (BotState.Settings.LimitDistributionTerritories * BotState.Players.Count(o => o.Value.State == GamePlayerState.Playing));

            var pickableTerritories = BotState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();
            bool isAntWastelanded = false;
            TerritoryIDType ausID = (TerritoryIDType) 0;
            var map = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            var weights = pickableTerritories.ToDictionary(o => o, terrID =>
            {
                // Check if Ant is wastelanded for Aus score modifier
                map.Territories[terrID].OwnerPlayerID = BotState.Me.ID;
                if (map.Territories[terrID].Bonuses[0].Details.Name.Equals("Antarctica") && BotState.BonusPickValueCalculator.IsWastelandedBonus(map.Territories[terrID].Bonuses[0]))
                {
                    isAntWastelanded = true;
                } else if (map.Territories[terrID].Bonuses[0].Details.Name.Equals("Australia"))
                {
                    ausID = terrID;
                }

                // Only find value if bonus has significiant amount
                if(map.Territories[terrID].Bonuses.Count > 0)
                {
                    BotBonus bonus = map.Territories[terrID].Bonuses[0];
                    bonus.SetMyExpansionValueHeuristic();
                    double r = bonus.ExpansionValue;
                    return r;
                }
                else
                {
                    return 0;
                }
            });

            // Check for if pick is in Aus
            if (!isAntWastelanded && weights.ContainsKey(ausID))
            {
                weights[ausID] -= 25;
            }

            // Check for FTBs and assign value for Aus based on Ant Wasteland boolean
            List<ComboBonuses> firstTurnBonusList = new List<ComboBonuses>();
            List<ComboBonuses> comboList = new List<ComboBonuses>();
            foreach (KeyValuePair<TerritoryIDType, double> pick in weights)
            {
                // Checks for FTB and combos
                if (BotState.BonusPickValueCalculator.IsFirstTurnBonus(map.Territories[pick.Key].Bonuses[0]))
                {
                    ComboBonuses newCombo = new ComboBonuses(map.Territories[pick.Key].Bonuses[0], map);
                    firstTurnBonusList.Add(newCombo);
                } else if (BotState.BonusPickValueCalculator.IsComboBonus(map.Territories[pick.Key].Bonuses[0], map))
                {
                    ComboBonuses newCombo = new ComboBonuses(map.Territories[pick.Key].Bonuses[0], map);
                    comboList.Add(newCombo);
                }
            }



            AILog.Log("\nDEBUG", "BEFORE LIST");
            foreach (ComboBonuses ftb in firstTurnBonusList)
            {
                AILog.Log("DEBUG", ftb.mainPick.Details.Name);
                for (int i = 1; i < ftb.adjacentPickTerritories.Count; i++)
                {
                    AILog.Log("\tDEBUG", ftb.adjacentPickTerritories[i].Details.Name);
                }
            }

            AILog.Log("\nDEBUG", "MIDDLE");

            foreach (ComboBonuses combo in comboList)
            {
                AILog.Log("DEBUG", combo.mainPick.Details.Name);
                for (int i = 1; i < combo.adjacentPickTerritories.Count; i++)
                {
                    AILog.Log("\tDEBUG", combo.adjacentPickTerritories[i].Details.Name);
                }
            }
            AILog.Log("\nDEBUG", "AFTER LIST");



            List<TerritoryIDType> picks = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();
            //StatefulFogRemover.PickedTerritories = picks;

            AILog.Log("\nDEBUG", "BEFORE");
            foreach (var terr in picks)
            {
                AILog.Log("DEBUG", map.Territories[terr].Details.Name);
            }

            ReorderPicksByCombos(firstTurnBonusList, comboList, ref picks);

            AILog.Log("DEBUG", "AFTER");
            foreach (var terr in picks)
            {
                AILog.Log("DEBUG", map.Territories[terr].Details.Name);
            }
            return picks;
        }

        // Check Tree Diagram for reference of reordering
        private void ReorderPicksByCombos(List<ComboBonuses> ftb, List<ComboBonuses> combos, ref List<TerritoryIDType> picks)
        {
            // Determine amount of useful FTBs and Combos
            int usableFTB = 0;
            int usableCombo = 0;
            for (int i = 0; i < ftb.Count; i++)
            {
                usableFTB += ftb[i].isFTB ? 1 : 0;
            }
            for (int i = 0; i < combos.Count; i++)
            {
                usableCombo += combos[i].isCombo ? 1 : 0;
            }

            // Case of no combos/FTBs
            if (usableFTB == 0 && usableCombo == 0)
            {
                return;
            }

            // Paths of tree where there is at least 1 FTB ELSE 0 usable FTBs
            if (usableFTB != 0)
            {
                int counterableFTBCount = 0;
                foreach (ComboBonuses combo in ftb)
                {
                    counterableFTBCount += combo.isCounterable ? 1 : 0;
                }

                // Path of trees where there are non-counterable FTBS, place 1/2 | 3/4
                for (int i = usableFTB - counterableFTBCount - 1; i >= 0; i--)
                {
                    for (int j = ftb[i].adjacentPickTerritories.Count - 1; j >= 0; j--)
                    {
                        TerritoryIDType temp = ftb[i].adjacentPickTerritories[j].ID;
                        picks.Remove(temp);
                        picks.Insert(0, temp);
                    }
                }

                // Case of 1/2 or 2/3 FTBs counterable
                if (counterableFTBCount + 1 == usableFTB && usableFTB != 1)
                {
                    for (int i = usableFTB - counterableFTBCount; i < usableFTB; i++)
                    {
                        for (int j = 2; j >= 0; j--)
                        {
                            TerritoryIDType temp = ftb[i].adjacentPickTerritories[j].ID;
                            picks.Remove(temp);
                            picks.Insert(3 + i, temp);
                        }
                    }
                }

                // If 1/1 or 2/2 or 3/3 Counterable FTBS and 0 combos ELSEIF  +1 combos
                if (counterableFTBCount == usableFTB && usableCombo == 0)
                {
                    // If 1/1 Counterable FTBs ELSE 2/2 | 3/3 counterable FTBs
                    if (counterableFTBCount == 1)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            TerritoryIDType temp = ftb[0].adjacentPickTerritories[i].ID;
                            picks.Remove(temp);
                            picks.Insert(3, temp);
                        }
                    } else
                    {
                        for (int i = 1; i >= 0; i--)
                        {
                            TerritoryIDType temp = ftb[0].adjacentPickTerritories[i].ID;
                            picks.Remove(temp);
                            picks.Insert(0, temp);
                        }
                        for (int i = 2; i >= 0; i--)
                        {
                            TerritoryIDType temp = ftb[1].adjacentPickTerritories[i].ID;
                            picks.Remove(temp);
                            picks.Insert(3, temp);
                        }
                    }
                } else if (counterableFTBCount == usableFTB)
                {
                    for (int i = 1; i >= 0; i--)
                    {
                        TerritoryIDType temp = combos[0].adjacentPickTerritories[i].ID;
                        picks.Remove(temp);
                        picks.Insert(0, temp);
                    }

                    if (counterableFTBCount == 1)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            TerritoryIDType temp = ftb[0].adjacentPickTerritories[i].ID;
                            picks.Remove(temp);
                            picks.Insert(3, temp);
                        }
                    } else
                    {
                        for (int i = 0; i < counterableFTBCount - 1; i++)
                        {
                            for (int j = 1; j >= 0; j--)
                            {
                                TerritoryIDType temp = ftb[i].adjacentPickTerritories[j].ID;
                                picks.Remove(temp);
                                picks.Insert(2 + i * 2, temp);
                            }
                        }
                    }
                }
            } else
            {
                // Case of no FTBs and at least 1 combo
                for (int i = combos[0].adjacentPickTerritories.Count - 1; i >= 0; i--)
                {
                    TerritoryIDType temp = combos[0].adjacentPickTerritories[i].ID;
                    picks.Remove(temp);
                    picks.Insert(0, temp);
                }

                if (usableCombo > 1)
                {
                    for (int i = combos[1].adjacentPickTerritories.Count- 1; i >= 0; i--)
                    {
                        TerritoryIDType temp = combos[1].adjacentPickTerritories[i].ID;
                        picks.Remove(temp);
                        picks.Insert(3, temp);
                    }
                }
            }

            TrimExcessPicks(ref picks);
        }

        private void TrimExcessPicks(ref List<TerritoryIDType> picks)
        {
            if (picks.Count > 6)
            {
                picks.RemoveRange(6, picks.Count - 6);
            }
        }
    }
}
