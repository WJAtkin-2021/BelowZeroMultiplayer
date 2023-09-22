namespace BelowZeroMultiplayerCommon
{
    public enum TokenExchangePolicy
    {
        None = 0,
        AutomaticHandover = 1,
        OnClientRequest = 2,
        OnClientRequestOrAuto = 3,
        DoNotYield = 4,
    }
}
