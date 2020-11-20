namespace EuNet.Unity
{
    /// <summary>
    /// 주기적 동기화 타입
    /// </summary>
    public enum PeriodicSyncType
    {
        /// <summary>
        /// 주기적인 동기화를 사용하지 않음
        /// </summary>
        None = 0,

        /// <summary>
        /// 언제나 주기적으로 동기화함
        /// </summary>
        Always = 1,

        /// <summary>
        /// 값이 변경되었을때만 주기적을 동기화함
        /// </summary>
        Changed = 2
    }
}