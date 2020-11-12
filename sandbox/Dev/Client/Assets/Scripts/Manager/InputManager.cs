using EuNet.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecutionOrder(-20)]
public class InputManager : MonoBehaviour
{
    private void Update()
    {
        ActorInput();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void ActorInput()
    {
        var actor = GameManager.Instance.ControlActor;
        if (actor == null)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        //actor.SetMoveDirection(x, y);
    }
}
