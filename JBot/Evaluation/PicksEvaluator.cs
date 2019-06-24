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

            var map = BotMap.FromStanding(BotState, BotState.DistributionStanding);
            var weights = pickableTerritories.ToDictionary(o => o, terrID =>
            {

                map.Territories[terrID].OwnerPlayerID = BotState.Me.ID;
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

            // Check for FTBs
            List<ComboBonuses> firstTurnBonusList = new List<ComboBonuses>();
            foreach (KeyValuePair<TerritoryIDType, double> bonus in weights)
            {
                if (BotState.BonusPickValueCalculator.IsFirstTurnBonus(map.Territories[bonus.Key].Bonuses[0]))
                {
                    ComboBonuses newCombo = new ComboBonuses(map.Territories[bonus.Key].Bonuses[0], map);
                    firstTurnBonusList.Add(newCombo);
                }
            }
            ReorderFirstTurnComboPicks(ref firstTurnBonusList);

            // Check for combos


            List<TerritoryIDType> picks = weights.OrderByDescending(o => o.Value).Take(maxPicks).Select(o => o.Key).Distinct().ToList();
            //StatefulFogRemover.PickedTerritories = picks;

            return picks;
        }


        private void ReorderFirstTurnComboPicks(ref List<ComboBonuses> list)
        {
            IDictionary<ComboBonuses, bool> iterated = new Dictionary<ComboBonuses, bool>;
            for (int i = 0; i < list.Count; i++)
            {
                if (iterated.ContainsKey(list[i]))
                {
                    continue;
                }
                if (list[i].adjacentPickTerritories.Count > 2)
                {
                    ComboBonuses temp = list[i];
                    list.Remove(temp);
                    list.Add(temp);
                    i--;
                }
            }
        }
    }
}
