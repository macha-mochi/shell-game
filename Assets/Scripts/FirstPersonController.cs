using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float mouseSensitivity = 0.1f;
    private CharacterController controller;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;

    private void OnEnable(){
        moveAction.action.Enable();
        lookAction.action.Enable();
    }
    private void OnDisable(){
        moveAction.action.Disable();
        lookAction.action.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMove();
        HandleLook();
    }

    public void HandleMove(){
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // want W = forward in X direction
        Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;
        controller.Move(dir * moveSpeed * Time.deltaTime);

    }
    public void HandleLook(){
        Vector2 input = lookAction.action.ReadValue<Vector2>() * mouseSensitivity;

        transform.Rotate(Vector3.up * input.x); //only rotate player around Y axis
    }
}


