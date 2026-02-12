public class DoublePowerAbility : ICardAbility
{
    public void Resolve(PlayerState owner, PlayerState opponent, CardInstance card)
    {
        // Double only THIS card's power
        int extraPower = card.GetBasePower();
        owner.Score += extraPower;
    }
}
