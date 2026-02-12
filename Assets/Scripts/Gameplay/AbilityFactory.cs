public static class AbilityFactory
{
    public static ICardAbility Create(AbilityData data)
    {
        switch (data.type)
        {
            case "GainPoints":
                return new GainPointsAbility(data.value);
            case "StealPoints":
                return new StealPointsAbility(data.value);
            case "DoublePower":
                return new DoublePowerAbility();
            default:
                return null;
        }
    }
}
