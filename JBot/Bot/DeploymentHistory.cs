﻿using System.Collections.Generic;

namespace WarLight.Shared.AI.JBot.Bot
{
    public class DeploymentHistory
    {
        public BotMain BotState;
        public DeploymentHistory(BotMain state)
        {
            BotState = state;
        }


        private Dictionary<PlayerIDType, int> OpponentDeployments = new Dictionary<PlayerIDType, int>();

        public virtual int GetOpponentDeployment(PlayerIDType opponentID)
        {
            if (BotState.NumberOfTurns < 1)
                return 0;

            return OpponentDeployments[opponentID];
        }
        
        public virtual void Update(PlayerIDType opponentID, int opponentDeployment)
        {
            AILog.Log("DeploymentHistory", "VisibleDeployment for " + opponentID + ": " + opponentDeployment);
            OpponentDeployments[opponentID] = opponentDeployment;
            Memory.DeploymentTracker.SetDeploys(opponentID, opponentDeployment, BotState.NumberOfTurns - 1);
        }
        
    }
}
