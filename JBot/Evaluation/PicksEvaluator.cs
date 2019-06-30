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
            foreach (KeyValuePair<TerritoryIDType, double> pick in weights)
            {
                // Checks for FTB
                if (BotState.BonusPickValueCalculator.IsFirstTurnBonus(map.Territories[pick.Key].Bonuses[0]))
                {
                    ComboBonuses newCombo = new ComboBonuses(map.Territories[pick.Key].Bonuses[0], map);
                    firstTurnBonusList.Add(newCombo);
                }
            }

            // Check for combos
            foreach (KeyValuePair<TerritoryIDType, double> pick in weights)
            {
                if (BotState.BonusPickValueCalculator.IsComboBonus(map.Territories[pick.Key].Bonuses[0]))
                {

                }
            }

            List<TerritoryIDType> picks = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();
            //StatefulFogRemover.PickedTerritories = picks;

            AILog.Log("DEBUG", "BEFORE");
            foreach (var terr in picks)
            {
                AILog.Log("DEBUG", map.Territories[terr].Details.Name);
            }

            for (int j = 0; j < firstTurnBonusList.Count; j++)
            {
                if (firstTurnBonusList[j].isFTB)
                {
                    for (int i = firstTurnBonusList[0].adjacentPickTerritories.Count - 1; i >= 0; i--)
                    {
                        BotTerritory temp = firstTurnBonusList[0].adjacentPickTerritories[i];
                        picks.Remove(temp.ID);
                        picks.Insert(0, temp.ID);
                    }
                }
            }

            AILog.Log("DEBUG", "BEFORE");
            foreach (var terr in picks)
            {
                AILog.Log("DEBUG", map.Territories[terr].Details.Name);
            }
            return picks;
        }
    }
}
