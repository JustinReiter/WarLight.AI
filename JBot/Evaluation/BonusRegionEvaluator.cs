using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Evaluation
{
    public class BonusRegionEvaluator
    {
        public BotMain BotState;
        public BonusRegionEvaluator(BotMain state)
        {
            this.BotState = state;
        }
    }
}
