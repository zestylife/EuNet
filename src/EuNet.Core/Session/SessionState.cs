namespace EuNet.Core
{
    /// <summary>
    /// 채널 상태
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// 초기화 되었지만 아직 사용되지 않음
        /// </summary>
        Initialized = 0,

        /// <summary>
        /// 접속이 완료됨
        /// </summary>
        Connected = 1,

        /// <summary>
        /// 접속이 해제됨
        /// </summary>
        Closed = 2
    }
}
