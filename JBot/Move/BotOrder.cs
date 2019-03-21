namespace WarLight.Shared.AI.JBot.Move
{
    public abstract class BotOrder
    {
        public PlayerIDType PlayerID;

        public abstract TurnPhase OccursInPhase { get; }

        public override string ToString()
        {
            return "BotOrder";
        }

    }
}
