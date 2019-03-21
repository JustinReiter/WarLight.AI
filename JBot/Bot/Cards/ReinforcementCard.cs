namespace WarLight.Shared.AI.JBot.Bot.Cards
{
    public class ReinforcementCard : Card
    {
        public int Armies { get; private set; }


        public ReinforcementCard(CardTypes cardType, CardInstanceIDType cardInstanceId, int armies) : base(cardType, cardInstanceId)
        {
            Armies = armies;
        }

    }
}
