namespace EuNet.Client
{
    /// <summary>
    /// P2P 연결상태
    /// </summary>
    public enum P2pConnectState : byte
    {
        /// <summary>
        /// 홀펀칭 중
        /// </summary>
        HolePunching,

        /// <summary>
        /// 연결되지 않음.
        /// 홀펀칭 실패 시 이 상태로 되며 릴레이 서버를 통해서 통신한다.
        /// 주기적으로 다시 홀펀칭할수도 있다.
        /// </summary>
        NotConnected,

        /// <summary>
        /// 단방향으로 직접 연결되었음.
        /// 내가 보낸 메시지가 상대편에만 잘 도착함.
        /// </summary>
        Connected,

        /// <summary>
        /// 양방향으로 직접 연결되었음.
        /// 상대방과 나 모두 직접 전송 가능.
        /// </summary>
        BothConnected,
    }
}