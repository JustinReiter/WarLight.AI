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
        public static List<TerritoryIDType> _picks = new List<TerritoryIDType>();

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

    }
}
