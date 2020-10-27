using EuNet.Core;
using EuNet.Unity;
using System.Collections.Generic;
using UnityEngine;

[ExecutionOrder(-10)]
public class ActorManager : SceneSingleton<ActorManager> , INetViewHandler
{
    private List<Actor> _actorList = new List<Actor>();
    private NetView _view;

    public Actor ControlActor { get; set; }

    protected override void Awake()
    {
        base.Awake();

        _view = GetComponent<NetView>();
    }

    private void Update()
    {
        float elapsedTime = Time.deltaTime;

        foreach (var actor in _actorList)
            actor.OnUpdate(elapsedTime);
    }

    public void Add(Actor actor)
    {
        _actorList.Add(actor);
    }

    public void Remove(Actor actor)
    {
        _actorList.Remove(actor);

        if (actor == ControlActor)
            ControlActor = null;
    }

    public void OnViewInstantiate(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void OnViewDestroy(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void OnViewMessage(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }
}
