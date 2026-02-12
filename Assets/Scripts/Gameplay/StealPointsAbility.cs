using UnityEngine;

public class StealPointsAbility : ICardAbility
{
    int value;

    public StealPointsAbility(int value)
    {
        this.value = value;
    }

    public void Resolve(PlayerState owner, PlayerState opponent, CardInstance card)
    {
        int pointsToSteal = Mathf.Min(value, opponent.Score);
        opponent.Score -= pointsToSteal;
        owner.Score += pointsToSteal;
    }
}
