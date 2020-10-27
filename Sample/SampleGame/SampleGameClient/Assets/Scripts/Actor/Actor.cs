using EuNet.Core;
using EuNet.Unity;
using UnityEngine;

[RequireComponent(typeof(NetView))]
public class Actor : MonoBehaviour , INetViewHandler
{
    [Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to eachother")]
    public int affiliation;
    [Tooltip("Represents point where other actors will aim when they attack this actor")]
    public Transform aimPoint;

    private BaseController _controller;
    public BaseController Controller => _controller;

    private NetView _view;
    public NetView View => _view;

    private float _maxHp;
    public float MaxHp
    {
        get { return _maxHp; }
        set
        {
            if (_maxHp <= 0f)
                return;

            _maxHp = value;
            _hp = Mathf.Min(_maxHp, _hp);
        }
    }

    private float _hp;
    public float Hp
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if(_hp <= 0f)
                Die();
        }
    }

    private void Awake()
    {
        _controller = GetComponent<BaseController>();
        _view = GetComponent<NetView>();
    }

    private void Start()
    {
        ActorManager.Instance?.Add(this);
    }

    protected void OnDestroy()
    {
        ActorManager.Instance?.Remove(this);
    }

    public void OnUpdate(float elapsedTime)
    {
        _controller.OnUpdate(elapsedTime);
    }

    public void Die()
    {
        _controller.OnDie();
    }

    public void OnViewInstantiate(NetDataReader reader)
    {
        
    }

    public void OnViewDestroy(NetDataReader reader)
    {
        
    }

    public void OnViewMessage(NetDataReader reader)
    {
        
    }
}
