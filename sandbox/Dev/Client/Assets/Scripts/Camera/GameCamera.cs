using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private const float MoveSmooth = 10.0f;

    void LateUpdate()
    {
        var actor = GameManager.Instance.ControlActor;
        if(actor != null)
        {
            Vector3 resultPos = actor.transform.position + (new Vector3(0f, 12f, -6f));

            transform.position = Vector3.Lerp(transform.position, resultPos, Time.deltaTime * MoveSmooth);
            transform.localRotation = Quaternion.Euler(60.0f, 0.0f, 0.0f);
        }
    }
}
