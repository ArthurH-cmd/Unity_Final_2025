using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float rotationSpeed = 1.0f;
    [SerializeField]
    public Transform target;

    private Animator animator;
    private Rigidbody rigidBody = null;
    private Player_Input input = null;
    private InputAction moveAction = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        input = new Player_Input();
        moveAction = input.Player.Move;
    }

    private void OnEnable()
    {
        input.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
        moveAction.Disable();
    }
    private void Update()
    {
        // Move Input
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        Vector3 fwd = rigidBody.transform.forward;
        Vector3 right = rigidBody.transform.right;
        fwd.y = 0.0f;
        right.y = 0.0f;
        fwd.Normalize();
        right.Normalize();

        Vector3 moveVelocity = (fwd * moveInput.y * moveSpeed) + (right * moveInput.x * moveSpeed);
        moveVelocity.y = rigidBody.linearVelocity.y;

        rigidBody.linearVelocity = moveVelocity;
        rigidBody.angularVelocity = Vector3.zero;

        // Look at target 
        if (target != null)
        {
            Vector3 dirToTarget = target.position - transform.position;
            dirToTarget.y = 0f; // keep level rotation

            if (dirToTarget != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }


        // playerInputs
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            animator.SetTrigger("RJab");
            Debug.Log("Right Punch Thrown!");
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("LJab");
            Debug.Log("Right Punch Thrown!");
        }


    }


}
