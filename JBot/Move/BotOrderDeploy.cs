using WarLight.Shared.AI.JBot.Bot;

namespace WarLight.Shared.AI.JBot.Move
{
    /// <summary>This Move is used in the first part of each round.</summary>
    /// <remarks>
    /// This Move is used in the first part of each round. It represents what Territory
    /// is increased with how many armies.
    /// </remarks>
    public class BotOrderDeploy : BotOrder
    {
        public BotTerritory Territory;

        public int Armies;

        public BotOrderDeploy(PlayerIDType playerID, BotTerritory territory, int armies)
        {
            this.PlayerID = playerID;
            this.Territory = territory;
            this.Armies = armies;
        }

        public override TurnPhase OccursInPhase
        {
            get { return TurnPhase.Deploys; }
        }


    }
}
