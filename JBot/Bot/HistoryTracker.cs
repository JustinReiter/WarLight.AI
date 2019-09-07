﻿using System.Linq;

namespace WarLight.Shared.AI.JBot.Bot
{
    /// <summary>
    /// HistoryTracker is responsible for storing the whole history of what has happened on the board so far and our thoughts about it.
    /// </summary>
    public class HistoryTracker
    {
        public HistoryTracker(BotMain state)
        {
            this.BotState = state;
            this.DeploymentHistory = new DeploymentHistory(state);
        }

        private BotMain BotState;

        public DeploymentHistory DeploymentHistory;

        public int GetOpponentDeployment(PlayerIDType opponentID)
        {
            return BotState.PrevTurn.Where(o => o.PlayerID == opponentID).OfType<GameOrderDeploy>().Sum(o => o.NumArmies);
        }

        public void ReadPlayerDeployment()
        {
            DeploymentHistory.Update(BotState.Me.ID, BotState.PrevTurn.Where(o => o.PlayerID == BotState.Me.ID).OfType<GameOrderDeploy>().Sum(o => o.NumArmies));
        }

        public void ReadOpponentDeployment()
        {
            foreach(var opponent in BotState.Opponents)
            {

                if (BotState.NumberOfTurns > 0)
                    DeploymentHistory.Update(opponent.ID, GetOpponentDeployment(opponent.ID));
                else
                    DeploymentHistory.Update(opponent.ID, 5);
            }
        }

        public int OpponentDeployment(PlayerIDType opponentID)
        {
            return DeploymentHistory.GetOpponentDeployment(opponentID);
        }
    }
}
