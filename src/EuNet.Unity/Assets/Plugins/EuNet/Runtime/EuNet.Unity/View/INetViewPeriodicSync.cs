using EuNet.Core;

namespace EuNet.Unity
{
    /// <summary>
    /// NetView의 주기적인 동기화를 위한 인터페이스
    /// </summary>
    public interface INetViewPeriodicSync
    {
        /// <summary>
        /// 주기적으로 동기화할때 직렬화
        /// </summary>
        /// <param name="writer">직렬화 데이터를 담을 NetDataWriter</param>
        /// <returns>만약 동기화를 하려면 true, 동기화를 하지 않으려면 false</returns>
        bool OnViewPeriodicSyncSerialize(NetDataWriter writer);

        /// <summary>
        /// 주기적 동기화 역직렬화
        /// </summary>
        /// <param name="reader">받은 동기화데이터. 이 데이터를 기반으로 역직렬화 하면 됨</param>
        void OnViewPeriodicSyncDeserialize(NetDataReader reader);
    }
}
