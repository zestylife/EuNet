namespace EuNet.Unity
{
    /// <summary>
    /// P2P 에서 사용되는 내부 프로토콜
    /// </summary>
    public enum NetProtocol : ushort
    {
        /// <summary>
        /// 게임오브젝트 생성
        /// </summary>
        P2pInstantiate,

        /// <summary>
        /// 게임오브젝트 파과
        /// </summary>
        P2pDestroy,

        /// <summary>
        /// 메시지
        /// </summary>
        P2pMessage,

        /// <summary>
        /// 복구 요청
        /// </summary>
        P2pRequestRecovery,

        /// <summary>
        /// 복구 응답
        /// </summary>
        P2pResponseRecovery,

        /// <summary>
        /// 위치 회전 동기화
        /// </summary>
        P2pPosRotSync,

        /// <summary>
        /// 주기적인 동기화
        /// </summary>
        P2pPeriodicSync,
    }
}
