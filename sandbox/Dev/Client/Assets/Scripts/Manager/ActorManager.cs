using EuNet.Unity;
using System.Collections.Generic;

public class ActorManager : SceneSingleton<ActorManager>
{
    private List<Actor> _actorList = new List<Actor>();
    public List<Actor> ActorList => _actorList;

    public bool Add(Actor actor)
    {
        if (_actorList.Contains(actor))
            return false;

        _actorList.Add(actor);
        return true;
    }

    public bool Remove(Actor actor)
    {
        return _actorList.Remove(actor);
    }
}
