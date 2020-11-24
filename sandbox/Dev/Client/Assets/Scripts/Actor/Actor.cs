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
    private SyncVector3 _netPosition;
    private float _moveSpeed = 10f;

    public Vector3 MoveVelocity
    {
        get
        {
            return _moveDirection * _moveSpeed;
        }
    }

    private void Awake()
    {
        _view = GetComponent<NetView>();
        _actorRpc = new ActorViewRpc(_view);
        _moveController = GetComponent<CharacterController>();
        _netPosition = transform.position;
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
        if(_view.IsMine())
        {
            var moveDelta = MoveVelocity * Time.deltaTime;
            _moveController.Move(moveDelta);
        }
        else
        {
            _netPosition.Update(Time.deltaTime);
            _moveController.Move(_netPosition - transform.position);
        }
    }

    public void SetMoveDirection(float x, float y)
    {
        _actorRpc
            .ToOthers(DeliveryMethod.Unreliable)
            .OnSetMoveDirection(x, y, transform.position);

        OnSetMoveDirection(x, y, transform.position);
    }

    public Task OnSetMoveDirection(float moveX, float moveY, Vector3 position)
    {
        _moveDirection = new Vector3(moveX, 0f, moveY).normalized;
        _netPosition.Set(transform.position, position, MoveVelocity);
        return Task.CompletedTask;
    }

    public Task<Color> OnTest(Vector3 position, Quaternion rotation)
    {
        throw new System.NotImplementedException();
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
        writer.Write(transform.position);
        return true;
    }

    public void OnViewPeriodicSyncDeserialize(NetDataReader reader)
    {
        _moveDirection = reader.ReadVector3();
        var position = reader.ReadVector3();
        _netPosition.Set(transform.position, position, MoveVelocity);
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
