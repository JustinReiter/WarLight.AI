using System.Collections.Generic;
using System.Linq;
using WarLight.Shared.AI.JBot.Bot;
using WarLight.Shared.AI.JBot.Move;

namespace WarLight.Shared.AI.JBot.Evaluation
{
    public class LastVisibleMapUpdater
    {
        public BotMain BotState;
        public LastVisibleMapUpdater(BotMain state)
        {
            this.BotState = state;
        }
        public void StoreOpponentDeployment()
        {
            var lastVisibleMap = BotState.LastVisibleMapX;
            foreach (GamePlayer opponent in BotState.Opponents)
            {
                List<GameOrderDeploy> opponentDeployments = BotState.PrevTurn.Where(o => o.PlayerID == opponent.ID).OfType<GameOrderDeploy>().ToList();
                foreach (GameOrderDeploy opponentDeployment in opponentDeployments)
                {
                    BotTerritory lwmTerritory = lastVisibleMap.Territories[opponentDeployment.DeployOn];
                    if (lwmTerritory.IsVisible)
                    {
                        MovesCommitter.CommittPlaceArmiesMove(new BotOrderDeploy(lwmTerritory.OwnerPlayerID, lwmTerritory, opponentDeployment.NumArmies));
                    }
                }
            }
        }
    }
}
