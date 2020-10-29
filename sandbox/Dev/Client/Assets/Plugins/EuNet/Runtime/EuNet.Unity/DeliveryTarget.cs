namespace EuNet.Unity
{
    public enum DeliveryTarget
    {
        //! 나를 포함한 전체 인원을 호출한다
        All,

        // 나를 제외한 나머지 인원에 호출한다
        Others,

        //! 마스터만 호출한다
        Master,
        
        //! 특정 세션 타겟
        Target,
    }
}
