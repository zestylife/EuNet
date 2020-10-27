using EuNet.Core;
using EuNet.Unity;
using UnityEngine;

public class BaseController : MonoBehaviour , INetViewHandler , INetViewPeriodicSync
{
    protected NetView _view { get; private set; }

    protected virtual void Awake()
    {
        _view = GetComponent<NetView>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void OnDestroy()
    {
        
    }

    public virtual void OnDie()
    {

    }

    public virtual void OnUpdate(float elapsedTime)
    {
        
    }

    public virtual void OnViewInstantiate(NetDataReader reader)
    {
        
    }

    public virtual void OnViewDestroy(NetDataReader reader)
    {
        
    }

    public virtual void OnViewMessage(NetDataReader reader)
    {
        
    }

    public virtual bool OnViewPeriodicSyncSerialize(NetDataWriter writer)
    {
        return false;
    }

    public virtual void OnViewPeriodicSyncDeserialize(NetDataReader reader)
    {
        
    }
}
