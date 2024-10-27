namespace BelowZeroMultiplayerCommon
{
    public enum TokenHandoverPolicy
    {
        Destroy = 0,
        YieldTokenToOtherClients = 1,
        AllowServerToYieldToken = 2,
        NeverYieldToken = 3,
    }
}
