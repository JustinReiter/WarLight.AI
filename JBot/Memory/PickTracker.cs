using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Memory
{
    static class PickTracker
    {
        private static List<TerritoryIDType> _picks = new List<TerritoryIDType>();
        private static List<TerritoryIDType> _chosenPicks = new List<TerritoryIDType>();
        private static List<TerritoryIDType> _enemyPicks = new List<TerritoryIDType>();

        public static TerritoryIDType GetPick(int pickSlot)
        {
            return _picks[pickSlot];
        }

        public static List<TerritoryIDType> GetPickList()
        {
            return _picks;
        }

        public static void SetPickList(List<TerritoryIDType> picks)
        {
            _picks = picks;
        }

        public static void SetPickList(List<BotTerritory> picks)
        {
            foreach (BotTerritory terr in picks)
            {
                _picks.Add(terr.ID);
            }
        }

        public static List<TerritoryIDType> GetChosenPickList()
        {
            return _chosenPicks;
        }

        public static void SetChosenPickList(List<TerritoryIDType> chosenPicks)
        {
            _chosenPicks = chosenPicks;
        }

        public static List<TerritoryIDType> GetEnemyPickList()
        {
            return _enemyPicks;
        }

        public static void SetEnemyPickList(List<TerritoryIDType> chosenPicks)
        {
            _chosenPicks = chosenPicks;
        }

        public  static int GetEnemyListCount()
        {
            return _enemyPicks.Count;
        }

        public static void SetConfirmedPicks(BotMain bot)
        {
            SetChosenPickList(bot.GetPicks());
            SetEnemyPickList(Except(_picks, _chosenPicks));
            Memory.CycleTracker.SetCyclePicks(_picks, _chosenPicks);
        }

        private static List<TerritoryIDType> Except(List<TerritoryIDType> main, List<TerritoryIDType> secondary)
        {
            List<TerritoryIDType> list = new List<TerritoryIDType>();
            foreach (TerritoryIDType terr in secondary)
            {
                if (!main.Contains(terr))
                {
                    list.Add(terr);
                }
            }
            return list;
        }


    }
}
