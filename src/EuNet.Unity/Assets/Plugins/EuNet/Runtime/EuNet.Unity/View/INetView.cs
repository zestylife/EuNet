using EuNet.Core;

namespace EuNet.Unity
{
    /// <summary>
    /// 게임오브젝트를 동기화하기 위한 넷뷰 인터페이스
    /// 다른 유저들과 동일한 ViewId로 동기화하거나 메시지를 주고 받음.
    /// </summary>
    public interface INetView
    {
        /// <summary>
        /// 고유한 ViewId. 다른 유저들과 동일한 ViewId로 동기화하거나 메시지를 주고 받음.
        /// </summary>
        int ViewId { get; }

        /// <summary>
        /// 네트워크로 게임오브젝트가 생성됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        void OnNetInstantiate(NetDataReader reader);

        /// <summary>
        /// 네트워크로 게임오브젝트가 파괴됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        void OnNetDestroy(NetDataReader reader);

        /// <summary>
        /// P2P 유저에게 데이터 전송
        /// </summary>
        /// <param name="dataWriter">전송할 데이터</param>
        /// <param name="deliveryTarget">전송 타겟</param>
        /// <param name="deliveryMethod">전송 방식</param>
        void SendMessage(NetDataWriter dataWriter, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod);

        /// <summary>
        /// 메시지가 도착함
        /// </summary>
        /// <param name="reader">전달된 메시지 데이터</param>
        void OnNetMessage(NetDataReader reader);

        /// <summary>
        /// 동기화 데이터가 도착함
        /// </summary>
        /// <param name="procorol">프로토콜</param>
        /// <param name="reader">전달된 데이터</param>
        void OnNetSync(NetProtocol procorol, NetDataReader reader);
    }
}
