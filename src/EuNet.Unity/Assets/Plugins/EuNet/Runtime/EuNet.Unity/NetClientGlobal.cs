using EuNet.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// 글로벌 NetClient 관리자. 반드시 전역적으로 1개를 생성하셔야 합니다.
    /// 한개의 NetClientP2p를 생성하고 사용하면 이곳에 자동 등록되어 관리됨.
    /// </summary>
    [ExecutionOrder(-10000)]
    [AddComponentMenu("EuNet/NetClientGlobal")]
    public class NetClientGlobal : Singleton<NetClientGlobal>
    {
        [Header("Mode")]
        [SerializeField] private bool _isLocalMode = false;

        /// <summary>
        /// 테스트를 위한 로컬모드 여부.
        /// </summary>
        public bool IsLocalMode => _isLocalMode;

        [Header("Synchronization")]
        [SerializeField] private float _precisionForVectorSqrtSync = 0.0001f;
        public float PrecisionForVectorSqrtSync => _precisionForVectorSqrtSync;

        [SerializeField] private float _precisionForQuaternionSync = 0.1f;
        public float PrecisionForQuaternionSync => _precisionForQuaternionSync;

        [SerializeField] private float _limitForPositionSqrtSync = 100f;
        public float LimitForPositionSqrtSync => _limitForPositionSqrtSync;

        [SerializeField] private float _limitForRotationSync = 90f;
        public float LimitForRotationSync => _limitForRotationSync;

        [SerializeField] private float _defaultSyncTime = 0.1f;
        public float DefaultSyncTime => _defaultSyncTime;

        private NetClientP2p _client;

        /// <summary>
        /// P2P로 연결된 클라이언트
        /// </summary>
        public NetClientP2p Client => _client;

        /// <summary>
        /// 현재 세션아이디
        /// </summary>
        public ushort SessionId => _client.SessionId;

        /// <summary>
        /// TCP 채널
        /// </summary>
        public TcpChannel TcpChannel => _client.TcpChannel;

        /// <summary>
        /// UDP 채널
        /// </summary>
        public UdpChannel UdpChannel => _client.UdpChannel;

        /// <summary>
        /// NetView 들을 관리하는 관리자
        /// </summary>
        public NetViews Views => _client.Views;

        /// <summary>
        /// 현재 세션상태
        /// </summary>
        public SessionState State => _client.State;

        /// <summary>
        /// P2P 클라이언트를 등록함. NetClientP2pBehaviour를 사용하면 자동으로 등록됨
        /// </summary>
        /// <param name="client">등록할 NetClientP2p</param>
        public void SetClient(NetClientP2p client)
        {
            if (client != null && _client != null)
            {
                Debug.LogError("[NetClientP2p] should have only one instance.");
                return;
            }

            _client = client;
        }

        /// <summary>
        /// NetView 를 등록
        /// </summary>
        /// <param name="view">등록할 NetView</param>
        /// <returns>성공여부</returns>
        public bool RegisterView(NetView view)
        {
            return Client.RegisterView(view);
        }

        /// <summary>
        /// NetView 의 등록을 해제
        /// </summary>
        /// <param name="view">등록해제할 NetView</param>
        /// <returns>성공여부</returns>
        public bool UnregisterView(NetView view)
        {
            if (Client == null)
                return false;

            return Client.UnregisterView(view);
        }

        /// <summary>
        /// 소유자가 있는 ViewId 를 생성함. 예를들어 플레이어 캐릭터.
        /// ViewId는 고유해야하며 다른 P2P멤버들과 동일한 객체에 동일한 ViewId를 가지고 동기화함.
        /// </summary>
        /// <returns>생성된 ViewId</returns>
        public int GenerateViewId()
        {
            return Client.GenerateViewId();
        }

        /// <summary>
        /// 소유자가 없는 SceneViewId를 생성함. 예를들어 몬스터나 미니언. 마스터만 호출할 수 있음.
        /// ViewId는 고유해야하며 다른 P2P멤버들과 동일한 객체에 동일한 ViewId를 가지고 동기화함.
        /// </summary>
        /// <returns>생성된 ViewId. 0보다 작으면 실패</returns>
        public int GenerateSceneViewId()
        {
            return Client.GenerateSceneViewId();
        }

        /// <summary>
        /// ViewId를 삭제함. 해당 NetView는 더이상 동기화나 통신이 불가함.
        /// </summary>
        /// <param name="viewId">제거할 ViewId</param>
        public void RemoveViewId(int viewId)
        {
            Client?.RemoveViewId(viewId);
        }

        /// <summary>
        /// 나 자산이 마스터인지 여부
        /// </summary>
        /// <returns>마스터 여부</returns>
        public bool MasterIsMine()
        {
            if (IsLocalMode == true)
                return true;

            return Client.MasterIsMine();
        }

        /// <summary>
        /// 네트워크를 통해 모든 유저에게 게임오브젝트를 생성한다. (소유자가 있음. 예를들어 플레이어 캐릭터)
        /// </summary>
        /// <param name="name">생성할 Resources 프리팹 이름</param>
        /// <param name="pos">위치</param>
        /// <param name="rot">회전</param>
        /// <param name="writer">생성시 전달할 추가 데이터</param>
        /// <returns>생성된 게임오브젝트</returns>
        public GameObject Instantiate(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Client.Instantiate(name, pos, rot, writer);
        }

        /// <summary>
        /// 네트워크를 통해 모든 유저에게 게임오브젝트를 생성한다. (소유자가 없음. 예를들어 몬스터, 미니언)
        /// </summary>
        /// <param name="name">생성할 Resources 프리팹 이름</param>
        /// <param name="pos">위치</param>
        /// <param name="rot">회전</param>
        /// <param name="writer">생성시 전달할 추가 데이터</param>
        /// <returns>생성된 게임오브젝트</returns>
        public GameObject InstantiateSceneObject(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Client.InstantiateSceneObject(name, pos, rot, writer);
        }

        /// <summary>
        /// 네트워크를 통해 모든 유저에게 게임오브젝트를 제거함
        /// </summary>
        /// <param name="viewId">제거할 오브젝트에 속한 ViewId</param>
        /// <param name="writer">제거시 전달할 추가 데이터</param>
        public void Destroy(int viewId, NetDataWriter writer = null)
        {
            Client.Destroy(viewId, writer);
        }

        /// <summary>
        /// P2P 메시지를 다른유저들이 같은 ViewId를 가진 NetView에게 전달함.
        /// </summary>
        /// <param name="view">메시지를 전달할 NetView</param>
        /// <param name="writer">메시지 데이터</param>
        /// <param name="deliveryTarget">전송 타겟</param>
        /// <param name="deliveryMethod">전송 방식</param>
        public void SendP2pMessage(INetView view, NetDataWriter writer, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod)
        {
            if (IsLocalMode == true)
                return;

            Client.SendP2pMessage(view, writer, deliveryTarget, deliveryMethod);
        }

        /// <summary>
        /// P2P 그룹의 모든 유저에게 데이터를 전송함
        /// </summary>
        /// <param name="dataWriter">전송할 데이터</param>
        /// <param name="deliveryMethod">전송 방식</param>
        public void SendAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            if (IsLocalMode == true)
                return;

            Client.SendAll(writer, deliveryMethod);
        }

        /// <summary>
        /// 마스터에게 복구를 요청함. 현재 NetView 게임오브젝트를 생성하고 상태를 복구함
        /// </summary>
        public Task RequestRecovery()
        {
            if (IsLocalMode == true)
                return Task.CompletedTask;

            return Client.RequestRecovery();
        }
    }
}