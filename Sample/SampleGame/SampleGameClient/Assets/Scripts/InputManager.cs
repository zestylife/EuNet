using EuNet.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecutionOrder(-11)]
public class InputManager : SceneSingleton<ActorManager>
{
    private Camera _mainCamera;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;
    }

    private void Start()
    {
        
    }

    

    private void LateUpdate()
    {
        var controlActor = ActorManager.Instance.ControlActor;
        if (controlActor != null)
        {
            _mainCamera.transform.localPosition = controlActor.transform.localPosition + new Vector3(0.2f, 8f, -7f);
            _mainCamera.transform.localRotation = Quaternion.Euler(50f, 0f, 0f);
        }
    }

    private void OnMovement(InputValue value)
    {
        Vector2 inputMovement = value.Get<Vector2>();

        var controlActor = ActorManager.Instance.ControlActor;
        if (controlActor != null)
        {
            var playerController = controlActor.Controller as PlayerController;
            if (playerController != null)
            {
                playerController.SetMoveDir(new Vector3(inputMovement.x, 0f, inputMovement.y));
            }
        }
    }

    private void OnAttack(InputValue value)
    {
        var controlActor = ActorManager.Instance.ControlActor;
        if (controlActor != null)
        {
            var playerController = controlActor.Controller as PlayerController;
            if (playerController != null)
            {
                playerController.Attack();
            }
        }
    }
}
