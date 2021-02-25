using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.JBot.BasicAlgorithms;
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

        /// <summary>
        /// Obtains the picks that will be used for the game. Will determine an initial score that will be used to order to picks initially, followed by finding FTBs and combos
        /// that will be interspersed based on cases outlined in tree diagram
        /// </summary>
        /// <returns>List of territory IDs of the final picks to be sent</returns>
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
            var map = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            var weights = pickableTerritories.ToDictionary(o => o, terrID =>
            {
                
                // Only find value if bonus has significiant amount
                if(map.Territories[terrID].Bonuses.Count > 0)
                {
                    BotBonus bonus = map.Territories[terrID].Bonuses[0];
                    bonus.SetMyPickValueHeuristic(); //first evaluation of bonuses, using their standardized value, wastelands and how fast they expand
                    double r = bonus.ExpansionValue;
                    return r;
                }
                else
                {
                    return 0;
                }
            });

          

            TerritoryIDType[] terrIDs = weights.Keys.OrderByDescending(a => weights[a]).ToArray();

            foreach (TerritoryIDType terrID in terrIDs)
            {
                AILog.Log("Pick Values", map.Territories[terrID].Bonuses[0] + ": " + weights[terrID]);
            }

            List<TerritoryIDType> picks = weights.Keys.OrderByDescending(a => weights[a]).ToList();

            Memory.PickTracker.SetPickList(picks);
            BotMap storedMap = BotState.VisibleMap.GetMapCopy();
            Memory.PickTracker.pickMap = storedMap;

            //Pick 3 und 4 based on coverage ändern

            double min_coverage = 6;
            int current_pick_to_check = 2;

            while (current_pick_to_check < picks.Count)
            {
                PathNode dist_1 = Dijkstra.ShortestPath(map, map.Territories[picks[0]], map.Territories[picks[1]]);
                double distance_1 = dist_1.minPath.Count;
                PathNode dist_2 = Dijkstra.ShortestPath(map, map.Territories[picks[0]], map.Territories[picks[current_pick_to_check]]);
                double distance_2 = dist_2.minPath.Count;
                PathNode dist_3 = Dijkstra.ShortestPath(map, map.Territories[picks[1]], map.Territories[picks[current_pick_to_check]]);
                double distance_3 = dist_3.minPath.Count;

                double actual_coverage = (distance_1 + distance_2 + distance_3) / 3;

                if (actual_coverage < min_coverage)
                {
                    current_pick_to_check++;
                }
                else
                {
                    TerritoryIDType swap = picks[2];
                    picks[2] = picks[current_pick_to_check];
                    picks[current_pick_to_check] = swap;
                    break;
                }
            }

            double min_coverage2 = 5;
            int current_pick_to_check2 = 3;

            while (current_pick_to_check < picks.Count)
            {
                PathNode dist_1 = Dijkstra.ShortestPath(map, map.Territories[picks[0]], map.Territories[picks[1]]);
                double distance_1 = dist_1.minPath.Count;
                PathNode dist_2 = Dijkstra.ShortestPath(map, map.Territories[picks[0]], map.Territories[picks[2]]);
                double distance_2 = dist_2.minPath.Count;
                PathNode dist_3 = Dijkstra.ShortestPath(map, map.Territories[picks[0]], map.Territories[picks[current_pick_to_check2]]);
                double distance_3 = dist_3.minPath.Count;
                PathNode dist_4 = Dijkstra.ShortestPath(map, map.Territories[picks[1]], map.Territories[picks[2]]);
                double distance_4 = dist_4.minPath.Count;
                PathNode dist_5 = Dijkstra.ShortestPath(map, map.Territories[picks[1]], map.Territories[picks[current_pick_to_check2]]);
                double distance_5 = dist_5.minPath.Count;
                PathNode dist_6 = Dijkstra.ShortestPath(map, map.Territories[picks[2]], map.Territories[picks[current_pick_to_check2]]);
                double distance_6 = dist_6.minPath.Count;

                double actual_coverage = (distance_1 + distance_2 + distance_3 + distance_4 + distance_5 + distance_6) / 6;

                if (actual_coverage < min_coverage2)
                {
                    current_pick_to_check++;
                }
                else
                {
                    TerritoryIDType swap = picks[3];
                    picks[3] = picks[current_pick_to_check2];
                    picks[current_pick_to_check2] = swap;
                    break;
                }
            }


            //List<BotTerritory> test = BasicAlgorithms.DistanceCalculator.GetShortestPathToTerritories(map, map.Territories[picks[0]]), map.Territories, emptyList);

            //Konter für 4 oder 5 picken als 6

            foreach (BotTerritory terri in map.Territories[picks[4]].Bonuses[0].Territories)
            {
                foreach (BotTerritory terri_neigh in terri.Neighbors)
                {
                    if (picks.Contains(terri_neigh.ID) && !picks.GetRange(0,5).Contains(terri_neigh.ID))
                    {
                        picks[5] = terri_neigh.ID;
                        goto PickFound;
                        
                    }
                }
            }

            foreach (BotTerritory terri in map.Territories[picks[3]].Bonuses[0].Territories)
            {
                foreach (BotTerritory terri_neigh in terri.Neighbors)
                {
                    if (picks.Contains(terri_neigh.ID) && !picks.GetRange(0, 5).Contains(terri_neigh.ID))
                    {
                        picks[5] = terri_neigh.ID;
                        goto PickFound;

                    }
                }
            }

        PickFound:

            picks = picks.GetRange(0, 6);
            return picks;
            }
            /*
            
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

            ReorderCombosByNumberOfTerritories(ref firstTurnBonusList);
            ReorderCombosByNumberOfTerritories(ref comboList);


            AILog.Log("Picks", "Number of FTBs: " + firstTurnBonusList.Count);
            AILog.Log("Picks", "Number of Combos: " + comboList.Count);
            AILog.Log("Picks", "FTBs found:");
            foreach (ComboBonuses ftb in firstTurnBonusList)
            {
                AILog.Log("Picks", "\tBonus: " + ftb.mainBonus.Details.Name);
                AILog.Log("Picks", "\t\t" + ftb.mainPick.Details.Name);
                for (int i = 0; i < ftb.adjacentPickTerritories.Count; i++)
                {
                    if (ftb.adjacentPickTerritories[i].Details.Name.Equals(ftb.mainPick.Details.Name))
                    {
                        continue;
                    }
                    AILog.Log("Picks", "\t\t\t" + ftb.adjacentPickTerritories[i].Details.Name);
                }
            }
            AILog.Log("Picks", "End of FTB list");
            AILog.Log("Picks", "Combos found:");

            foreach (ComboBonuses combo in comboList)
            {
                AILog.Log("Picks", "\tBonus: " + combo.mainBonus.Details.Name);
                AILog.Log("Picks", "\t\t" + combo.mainPick.Details.Name);
                for (int i = 0; i < combo.adjacentPickTerritories.Count; i++)
                {
                    if (combo.adjacentPickTerritories[i].Details.Name.Equals(combo.mainPick.Details.Name))
                    {
                        continue;
                    }
                    AILog.Log("Picks", "\t\t\t" + (combo.adjacentPickTerritories[i].Details.Name.Equals(combo.mainPick.Details.Name) ? combo.adjacentPickTerritories[++i].Details.Name : combo.adjacentPickTerritories[i].Details.Name));
                }
            }
            AILog.Log("Picks", "End of combo list");



            List<TerritoryIDType> picks = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();
            //StatefulFogRemover.PickedTerritories = picks;

            AILog.Log("Picks", "Before final reshuffle:");
            foreach (var terr in picks)
            {
                AILog.Log("Picks", "\t" + map.Territories[terr].Bonuses[0].Details.Name + ", " + map.Territories[terr].Details.Name);
            }

            ReorderPicksByCombos(firstTurnBonusList, comboList, ref picks);

            AILog.Log("Picks", "After final reshuffle:");
            foreach (var terr in picks)
            {
                AILog.Log("Picks", "\t" + map.Territories[terr].Bonuses[0].Details.Name + ", " + map.Territories[terr].Details.Name);
            }

            Memory.PickTracker.SetPickList(picks);
            BotMap storedMap = BotState.VisibleMap.GetMapCopy();
            Memory.PickTracker.pickMap = storedMap;
            return picks;
        }

        */

            /// <summary>
            /// Prioritizes combos or FTBs that contain less picks adjacent to the bonus. Also places Sub Combos at bottom of list with exception of counterable combos
            /// </summary>
            /// <param name="list"></param>
            private void ReorderCombosByNumberOfTerritories(ref List<ComboBonuses> list)
        {
            List<TerritoryIDType> seenPickIDs = new List<TerritoryIDType>();
            for (int i = 0; i < list.Count; i++)
            {
                if (seenPickIDs.Contains(list[i].mainPick.ID))
                {
                    continue;
                } else
                {
                    seenPickIDs.Add(list[i].mainPick.ID);
                    if (list[i].adjacentPickTerritories.Count < 3 && list[i].isEfficient)
                    {
                        ComboBonuses temp = list[i];
                        list.Remove(temp);
                        list.Insert(0, temp);
                    }
                }
            }

            if (list.Count > 0 && !list[0].isFTB)
            {
                List<TerritoryIDType> hasSeen = new List<TerritoryIDType>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (!hasSeen.Contains(list[i].mainPick.Details.ID) && (list[i].mainBonus.Amount == 3 || GetSmallestBonus(list[i].isFTB ? list[i].supportFTBPick : list[i].supportComboPick) == 5))
                    {
                        ComboBonuses temp = list[i];
                        list.RemoveAt(i);
                        list.Add(temp);
                        hasSeen.Add(list[i].mainPick.Details.ID);
                    }
                }

                hasSeen = new List<TerritoryIDType>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (!hasSeen.Contains(list[i].mainPick.Details.ID) && list[i].adjacentPickTerritories.Count > 2)
                    {
                        ComboBonuses temp = list[i];
                        list.RemoveAt(i);
                        list.Add(temp);
                        hasSeen.Add(list[i].mainPick.Details.ID);
                    }
                }
            }
        }

        private int GetSmallestBonus(List<BotTerritory> list)
        {
            int smallestBonus = 10;
            foreach (BotTerritory terr in list)
            {
                smallestBonus = terr.Bonuses[0].Amount < smallestBonus ? terr.Bonuses[0].Amount : smallestBonus;
            }
            return smallestBonus;
        }

        /// <summary>
        /// Final reshuffling of picks to organize according to situations of number of (counterable) FTBs and counters. 
        /// Check tree diagram for ordering of picks by case
        /// </summary>
        /// <param name="ftb">List of FTBs</param>
        /// <param name="combos">List of combos</param>
        /// <param name="picks">List of current picks</param>
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
                    for (int i = 1; i < usableFTB; i++)
                    {
                        for (int j = 2; j >= 0; j--)
                        {
                            TerritoryIDType temp = ftb[i].adjacentPickTerritories[j].ID;
                            picks.Remove(temp);
                            picks.Insert(3 + i - 1, temp);
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
                TerritoryIDType temp = combos[0].adjacentPickTerritories[0].ID;
                picks.Remove(temp);
                picks.Insert(0, temp);
                temp = combos[0].adjacentPickTerritories[1].ID;
                picks.Remove(temp);
                picks.Insert(2, temp);

                if (usableCombo > 1)
                {
                    for (int i = 1; i >= 0; i--)
                    {
                        temp = combos[1].adjacentPickTerritories[i].ID;
                        picks.Remove(temp);
                        picks.Insert(3, temp);
                    }
                }
            }

            TrimExcessPicks(ref picks);
        }

        /// <summary>
        /// Trims the number of final picks in the list to correspond to the maximum number of allowable picks. Due to the nature of the shuffling of picks, picks after 6 have
        /// no guarantee for relation to any combos and instead should follow scoring
        /// </summary>
        /// <param name="picks"></param>
        private void TrimExcessPicks(ref List<TerritoryIDType> picks)
        {
            int picksAllowed = BotState.Players.Count * BotState.Settings.LimitDistributionTerritories;
            if (picks.Count > picksAllowed)
            {
                picks.RemoveRange(picksAllowed, picks.Count - picksAllowed);
            }
        }
    }
}
