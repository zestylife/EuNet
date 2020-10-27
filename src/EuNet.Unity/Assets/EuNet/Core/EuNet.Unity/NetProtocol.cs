namespace EuNet.Unity
{
    public enum NetProtocol : ushort
    {
        P2pInstantiate,
        P2pDestroy,
        P2pMessage,
        P2pRequestRecovery,
        P2pResponseRecovery,
        P2pPosRotSync,
        P2pPeriodicSync,
    }
}
