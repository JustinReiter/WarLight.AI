using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class BonusTracker
    {

        private static List<BotBonus> takenBonuses = new List<BotBonus>();
        private static List<BotBonus> immediateLostBonuses = new List<BotBonus>();
        private static List<BotBonus> immediateTakenBonuses = new List<BotBonus>();
        private static int armyTurnDifference = 0;
        private static int armySum = 0;

        public static void AddTakenBonus(BotBonus bonus)
        {
            if (bonus.IsOwnedByMyself())
            {
                takenBonuses.Add(bonus);
                armySum += bonus.Amount;
            }
        }

        public static void RemoveTakenBonus(BotBonus bonus)
        {
            if (!bonus.IsOwnedByMyself())
            {
                takenBonuses.Remove(bonus);
                armySum -= bonus.Amount;
            }
        }
        
        public static List<BotBonus> GetTakenBonuses()
        {
            return takenBonuses;
        }

        public static int GetTakenBonusesCount()
        {
            return takenBonuses.Count;
        }

        public static int GetTakenBonusesArmies()
        {
            int sum = 0;
            foreach (BotBonus bonus in takenBonuses)
            {
                sum += bonus.Amount;
            }
            return sum;
        }

        public static int GetArmyTurnDifference()
        {
            return armyTurnDifference;
        }

        public static List<BotBonus> GetImmediateLostBonuses()
        {
            return immediateLostBonuses;
        }

        public static List<BotBonus> GetImmediateTakenBonuses()
        {
            return immediateTakenBonuses;
        }

        public static void UpdateTakenBonuses(BotMain bot)
        {
            armyTurnDifference = 0;
            immediateLostBonuses.Clear();
            immediateTakenBonuses.Clear();
            foreach (BotBonus bonus in takenBonuses)
            {
                if (!bonus.IsOwnedByMyself())
                {
                    RemoveTakenBonus(bonus);
                    armyTurnDifference -= bonus.Amount;
                    immediateLostBonuses.Add(bonus);
                }
            }

            foreach (BotBonus bonus in bot.VisibleMap.Bonuses.Values)
            {
                if (bonus.IsOwnedByMyself())
                {
                    AddTakenBonus(bonus);
                    armyTurnDifference += bonus.Amount;
                    immediateTakenBonuses.Add(bonus);
                }
            }
        }


    }
}
