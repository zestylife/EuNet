using Common;
using EuNet.Core;
using EuNet.Unity;
using System.Threading.Tasks;
using UnityEngine;

public class Actor : MonoBehaviour , INetViewHandler, INetSerializable , INetViewPeriodicSync , IActorViewRpc
{
    [SerializeField] private Renderer _renderer;

    private NetView _view;
    public  NetView View => _view;
    private ActorViewRpc _actorRpc;
    private CharacterController _moveController;
    private Vector3 _moveDirection;
    private Vector3? _netSyncPosition;
    private float _moveSpeed = 10f;

    private void Awake()
    {
        _view = GetComponent<NetView>();
        _actorRpc = new ActorViewRpc(_view);
        _moveController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        ActorManager.Instance.Add(this);
    }

    private void OnDestroy()
    {
        ActorManager.Instance?.Remove(this);
    }

    private void Update()
    {
        var moveDelta = _moveDirection * _moveSpeed * Time.deltaTime;

        if (_netSyncPosition.HasValue)
        {
            // 네트워크 위치와 동기화를 하자
            _netSyncPosition += moveDelta;

            var dist = _netSyncPosition.Value - transform.localPosition;
            moveDelta = dist * Mathf.Min(Time.deltaTime * 10f, 1f);
        }

        _moveController.Move(moveDelta);
    }

    public void SetMoveDirection(float x, float y)
    {
        _actorRpc
            .ToOthers(DeliveryMethod.Unreliable)
            .OnSetMoveDirection(x, y);

        OnSetMoveDirection(x, y);
    }

    public Task OnSetMoveDirection(float x, float y)
    {
        _moveDirection = new Vector3(x, 0f, y).normalized;
        return Task.CompletedTask;
    }

    public void OnViewInstantiate(NetDataReader reader)
    {
        _renderer.material.color = reader.ReadColor();
    }

    public void OnViewDestroy(NetDataReader reader)
    {
        
    }

    public void OnViewMessage(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public bool OnViewPeriodicSyncSerialize(NetDataWriter writer)
    {
        writer.Write(_moveDirection);
        writer.Write(transform.localPosition);
        return true;
    }

    public void OnViewPeriodicSyncDeserialize(NetDataReader reader)
    {
        _moveDirection = reader.ReadVector3();
        _netSyncPosition = reader.ReadVector3();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Write(_renderer.material.color);
    }

    public void Deserialize(NetDataReader reader)
    {
        _renderer.material.color = reader.ReadColor();
    }
}
