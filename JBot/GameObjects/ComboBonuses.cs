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
            adjacentPickTerritories.Add(mainBonus);
            populateAdjacentPickList(map);
            isFTB = mainBonus.Amount == 3 ? true : false;
            isCounterable = adjacentPickTerritories.Count > 2 ? true : false;

        }

        private void populateAdjacentPickList(BotMap map)
        {
            foreach (var terr in mainBonus.Territories)
            {
                foreach (var adjTerr in terr.Neighbors)
                {
                    if (!containsTerritory(mainBonus, adjTerr) && adjTerr.Armies.NumArmies == 0)
                    {
                        adjacentPickTerritories.Add(adjTerr.Bonuses[0]);
                    }
                }
            }
        }

        private Boolean getCounterable()
        {
            return adjacentPickTerritories.Count > 2 ? true : false;
        }

        private Boolean containsTerritory(BotBonus bonus, BotTerritory territory)
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
    }
}
