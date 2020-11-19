namespace EuNet.Core
{
    /// <summary>
    /// 전송 타겟. P2P 전용
    /// </summary>
    public enum DeliveryTarget
    {
        /// <summary>
        /// 나를 포함한 전체 인원을 호출한다
        /// </summary>
        All,

        /// <summary>
        /// 나를 제외한 나머지 인원에 호출한다
        /// </summary>
        Others,

        /// <summary>
        /// 마스터만 호출한다
        /// </summary>
        Master,

        /// <summary>
        /// 특정 세션 타겟. 세션 아이디를 입력하여 보낸다.
        /// </summary>
        Target,
    }
}
