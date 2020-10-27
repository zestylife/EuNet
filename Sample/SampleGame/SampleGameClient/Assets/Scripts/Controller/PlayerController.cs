using EuNet.Core;
using EuNet.Unity;
using SampleGameCommon;
using System.Threading.Tasks;
using UnityEngine;

public partial class PlayerController : BaseController
{
    private CharacterController _characterController;
    private Vector3 _moveDir;
    private float _moveSpeed = 10f;
    private Animator _animator;

    private static int movementHash = Animator.StringToHash("Movement");

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        _characterController.enableOverlapRecovery = true;

        _animator = GetComponent<Animator>();

        _playerRpc = new PlayerViewRpc(_view, DeliveryMethod.ReliableOrdered);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDie()
    {
        base.OnDie();
    }

    public override void OnUpdate(float elapsedTime)
    {
        base.OnUpdate(elapsedTime);

        if (_moveDir != Vector3.zero)
        {
            _characterController.Move(_moveDir * elapsedTime * _moveSpeed);
            
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation(_moveDir), elapsedTime * 10f);
        }

        _animator.SetFloat(movementHash, _moveDir.sqrMagnitude);
    }

    public void SetMoveDir(Vector3 dir)
    {
        _moveDir = dir.normalized;

        NetPool.DataWriterPool.Use((NetDataWriter writer) =>
        {
            writer.Write(Protocol.Move);
            writer.Write(transform.localPosition);
            writer.Write(_moveDir);

            _view.SendMessage(writer, DeliveryTarget.Others, DeliveryMethod.Unreliable);
        });
    }

    public void Attack()
    {
        _playerRpc.OnAttack();
        OnAttack();

        /*
        NetPool.DataWriterPool.Use((NetDataWriter writer) =>
        {
            writer.Write(Protocol.Attack);

            _view.SendMessage(writer, DeliveryTarget.All, DeliveryMethod.ReliableOrdered);
        });
        */
    }

    public override bool OnViewPeriodicSyncSerialize(NetDataWriter writer)
    {
        base.OnViewPeriodicSyncSerialize(writer);

        writer.Write(transform.localPosition);
        writer.Write(_moveDir);
        
        return true;
    }

    public override void OnViewPeriodicSyncDeserialize(NetDataReader reader)
    {
        base.OnViewPeriodicSyncDeserialize(reader);

        var pos = reader.ReadVector3();
        _moveDir = reader.ReadVector3();

        if (_moveDir == Vector3.zero ||
            (transform.localPosition - pos).sqrMagnitude >= 100f)
            transform.localPosition = pos;
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, pos, 0.5f);
        }
    }

    public override void OnViewInstantiate(NetDataReader reader)
    {
        base.OnViewInstantiate(reader);
    }

    public override void OnViewDestroy(NetDataReader reader)
    {
        base.OnViewDestroy(reader);
    }

    public override void OnViewMessage(NetDataReader reader)
    {
        base.OnViewMessage(reader);

        var protocol = reader.GetProtocol();
        switch(protocol)
        {
            case Protocol.Attack:
                {
                    _animator.SetTrigger("Attack");
                }
                break;
            case Protocol.Move:
                {
                    transform.localPosition = reader.ReadVector3();
                    _moveDir = reader.ReadVector3();
                }
                break;
        }
    }
}
