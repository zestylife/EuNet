using EuNet.Unity;
using UnityEngine;

public class PatrolSphere : NetView
{
    public GameObject[] WayPoints;
    private int _currentWayPoint;

    void Start()
    {
        _currentWayPoint = 0;
    }

    
    void Update()
    {
        if (IsMine())
            MovePatrol();
    }

    private void MovePatrol()
    {
        var obj = WayPoints[_currentWayPoint];
        var originPos = transform.localPosition;
        var targetPos = obj.transform.position;

        var resultPos = Vector3.MoveTowards(originPos, targetPos, Time.deltaTime * 2);

        transform.localPosition = resultPos;

        if ((resultPos - targetPos).sqrMagnitude <= 0.01)
        {
            _currentWayPoint++;
            if (_currentWayPoint >= WayPoints.Length)
                _currentWayPoint = 0;
        }
    }
}
