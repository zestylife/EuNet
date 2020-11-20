using EuNet.Core;

namespace EuNet.Unity
{
    /// <summary>
    /// NetView의 이벤트를 받기 위한 인터페이스.
    /// </summary>
    public interface INetViewHandler
    {
        /// <summary>
        /// 네트워크로 게임오브젝트가 생성됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        void OnViewInstantiate(NetDataReader reader);

        /// <summary>
        /// 네트워크로 게임오브젝트가 파괴됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        void OnViewDestroy(NetDataReader reader);

        /// <summary>
        /// 네트워크로 메시지가 도착함
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        void OnViewMessage(NetDataReader reader);
    }
}
