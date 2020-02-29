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
        public double GetExpansionValue(BotBonus bonus)
        {
            double expansionValue = getInitialExpansionValue(bonus);
            Boolean isFirstTurnBonus = IsFirstTurnBonus(bonus);
            expansionValue += addBonusCoverageFactor(bonus);

            return expansionValue;
        }

        private double getInitialExpansionValue(BotBonus bonus)
        {
            double expansionValue = 0;
            expansionValue = GetInefficientWastelandedBonusFactor(bonus);
            if (IsExpansionWorthless(bonus))
            {
                return expansionValue;
            }
            expansionValue += assignStandardizedValue(bonus); // Assigning standardized values

            if (IsManyTurnBonus(bonus))
            {
                expansionValue -= 20;
            }
            return expansionValue;
        }

        private double addBonusCoverageFactor(BotBonus bonus)
        {
            double coverageBonus = 0;
            List<BotBonus> CoveredNeighours = getNonWastelandedNeighbourBonuses(bonus);
            for (int i = 0; i < CoveredNeighours.Count; i++){
                if (getInitialExpansionValue(CoveredNeighours[i]) > 0)
                {
                    coverageBonus += getInitialExpansionValue(CoveredNeighours[i])*0.2;
                }
            }
            return coverageBonus;
        }

        private List <BotBonus> getNonWastelandedNeighbourBonuses(BotBonus bonus) //List of bonuses without wastelands, that are maximum 1 distance away from the bonus
        {

            List<BotBonus> CoveredNeighours = new List<BotBonus>();

            foreach (BotTerritory terr in bonus.Territories)
            {
                foreach (BotTerritory adjTerr in terr.Neighbors)
                {
                    foreach (BotTerritory secondAdjTerr in adjTerr.Neighbors)
                    {                       
                        if (!CoveredNeighours.Contains(secondAdjTerr.Bonuses[0])){
                            CoveredNeighours.Add(secondAdjTerr.Bonuses[0]);
                        }
                    }
                }
            }

            CoveredNeighours.Remove(bonus);
            return CoveredNeighours;
        }

        private double assignStandardizedValue(BotBonus bonus)
        {
            double expansionValue = 0;
            switch (bonus.Details.Name)
            {
                case "Antarctica":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {

                        if (terr.Armies.NumArmies == 0)
                        {

                            switch (terr.Details.Name)
                            {
                                case "Siple": expansionValue += 50; break;
                                case "South Pole": expansionValue += 50; break;
                                case "Scott": expansionValue += 50; break;
                                case "Novolazarevskaya": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Australia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Tasmania": expansionValue += 50; break;
                                case "New Zealand": expansionValue += 50; break;
                                case "New Southwales": expansionValue += 50; break;
                                case "South Australia": expansionValue += 50; break;
                                case "Western Australia": expansionValue += 50; break;
                                case "Queensland": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Canada":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Yukon": expansionValue += 50; break;
                                case "British Columbia": expansionValue += 50; break;
                                case "Northwest Territories": expansionValue += 50; break;
                                case "Alberta": expansionValue += 50; break;
                                case "Nunavut": expansionValue += 50; break;
                                case "Ontario": expansionValue += 50; break;
                                case "Quebec": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Caucasus":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Tadschikistan": expansionValue += 50; break;
                                case "Kyrgrzstan": expansionValue += 50; break;
                                case "Eastern Kazakhstan": expansionValue += 50; break;
                                case "Turkmenistan": expansionValue += 50; break;
                                case "Western Kazakhszan": expansionValue += 50; break;
                                case "Georgia": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Central America":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Baja": expansionValue += 50; break;
                                case "Mexico": expansionValue += 50; break;
                                case "Panama": expansionValue += 50; break;
                                case "Cuba": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Central Russia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Tomsk": expansionValue += 50; break;
                                case "Omsk": expansionValue += 50; break;
                                case "Tura": expansionValue += 50; break;
                                case "Jessej": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "East Africa":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Kenya": expansionValue += 50; break;
                                case "Somalia": expansionValue += 50; break;
                                case "Ethiopia": expansionValue += 50; break;
                                case "Sudan": expansionValue += 50; break;
                                case "Egypt": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "East China":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Taiwan": expansionValue += 50; break;
                                case "Shanghai": expansionValue += 50; break;
                                case "Jiangxi": expansionValue += 50; break;
                                case "Hong Kong": expansionValue += 50; break;
                                case "Beijing": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "East Russia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Kamchatka": expansionValue += 50; break;
                                case "Mys Lopatk": expansionValue += 50; break;
                                case "Khabarovsk": expansionValue += 50; break;
                                case "Eastern Siberia": expansionValue += 50; break;
                                case "Chita": expansionValue += 50; break;
                                case "Western Siberia": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "East US":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Midwest": expansionValue += 50; break;
                                case "Gulf Coast": expansionValue += 50; break;
                                case "Great Lakes": expansionValue += 50; break;
                                case "Atlantic Northeast": expansionValue += 50; break;
                                case "Tennessee": expansionValue += 50; break;
                                case "Florida": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Europe":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Ukraine": expansionValue += 50; break;
                                case "Poland": expansionValue += 50; break;
                                case "Germany": expansionValue += 50; break;
                                case "Italy": expansionValue += 50; break;
                                case "France": expansionValue += 50; break;
                                case "Spain": expansionValue += 50; break;
                                case "United Kingdom": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Greenland":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Danmark Havn": expansionValue += 50; break;
                                case "Iceland": expansionValue += 50; break;
                                case "Itseqqortoormiit": expansionValue += 50; break;
                                case "Nord": expansionValue += 50; break;
                                case "Nuuk": expansionValue += 50; break;
                                case "Qaanaaq": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Indonesia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Solomon Islands": expansionValue += 50; break;
                                case "Papua New Guinea": expansionValue += 50; break;
                                case "Philippines": expansionValue += 50; break;
                                case "Borneo": expansionValue += 50; break;
                                case "Malaysia": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Middle East":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Afghanistan": expansionValue += 50; break;
                                case "Iran": expansionValue += 50; break;
                                case "Saudi Arabia": expansionValue += 50; break;
                                case "Iraq": expansionValue += 50; break;
                                case "Israel": expansionValue += 50; break;
                                case "Turkey": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "North Africa":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Niger": expansionValue += 50; break;
                                case "Mali": expansionValue += 50; break;
                                case "Mauritania": expansionValue += 50; break;
                                case "Algeria": expansionValue += 50; break;
                                case "Libya": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "South Africa":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Madagascar": expansionValue += 50; break;
                                case "South Africa": expansionValue += 50; break;
                                case "Namibia": expansionValue += 50; break;
                                case "Botswana": expansionValue += 50; break;
                                case "Angola": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "South America":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Colombia": expansionValue += 50; break;
                                case "Venezuela": expansionValue += 50; break;
                                case "Brazil": expansionValue += 50; break;
                                case "Bolivia": expansionValue += 50; break;
                                case "Argentina": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "Southeast Asia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                            {
                                switch (terr.Details.Name)
                                {
                                    case "Thailand": expansionValue += 50; break;
                                    case "Myanmar": expansionValue += 50; break;
                                    case "India": expansionValue += 50; break;
                                    case "Pakistan": expansionValue += 50; break;
                                    default: break;
                                }
                            }
                    }

                    break;

                case "West Africa":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Congo": expansionValue += 50; break;
                                case "Cameroon": expansionValue += 50; break;
                                case "Chad": expansionValue += 50; break;
                                case "Nigeria": expansionValue += 50; break;
                                case "Ghana": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "West China":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Tibet": expansionValue += 50; break;
                                case "Xinjiang": expansionValue += 50; break;
                                case "Qinghai": expansionValue += 50; break;
                                case "Shaanxi": expansionValue += 50; break;
                                case "Mongolia": expansionValue += 50; break;
                                case "Inner Mongolia": expansionValue += 50; break;
                                case "Heilongjiang": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "West Russia":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Vorkuta": expansionValue += 50; break;
                                case "Ufa": expansionValue += 50; break;
                                case "Arkhangelsk": expansionValue += 50; break;
                                case "Moscow": expansionValue += 50; break;
                                case "Murmansk": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }

                    break;

                case "West US":

                    expansionValue += 45;

                    foreach (BotTerritory terr in bonus.Territories)
                    {
                        if (terr.Armies.NumArmies == 0)
                        {
                            switch (terr.Details.Name)
                            {
                                case "Pacific Northwest": expansionValue += 50; break;
                                case "California": expansionValue += 50; break;
                                case "Great Basin": expansionValue += 50; break;
                                case "Southwest": expansionValue += 50; break;
                                case "Rocky Mountains": expansionValue += 50; break;
                                case "Texas": expansionValue += 50; break;
                                case "Great Plains": expansionValue += 50; break;
                                default: break;
                            }
                        }
                    }
                    
                    break;

                default:
                    break;
            }
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
                value -= 1000;
            }

            return IsInefficientBonus(bonus) ? value - 25 : value;
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
                                if (!ContainsTerritory(bonus, adjTerr) && adjTerr.Armies.NumArmies == 0 && !IsInefficientBonus(adjTerr.Bonuses[0]) && !IsWastelandedBonus(adjTerr.Bonuses[0]))
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
