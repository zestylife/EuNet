namespace EuNet.Server
{
    /// <summary>
    /// 서버 상태
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// 상태 없음
        /// </summary>
        None = 0,

        /// <summary>
        /// 시작중
        /// </summary>
        Starting = 1,

        /// <summary>
        /// 시작됨
        /// </summary>
        Started = 2,

        /// <summary>
        /// 정지중
        /// </summary>
        Stopping = 3,

        /// <summary>
        /// 정지됨
        /// </summary>
        Stopped = 4
    }
}
