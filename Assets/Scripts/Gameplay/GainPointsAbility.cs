public class GainPointsAbility : ICardAbility
{
    int value;

    public GainPointsAbility(int value)
    {
        this.value = value;
    }

    public void Resolve(PlayerState owner, PlayerState opponent, CardInstance card)
    {
        owner.Score += value;
    }
}
