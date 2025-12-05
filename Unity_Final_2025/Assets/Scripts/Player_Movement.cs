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

    [SerializeField]
    private bool isPlayer2 = false;


    [SerializeField]
    private float pushForce = 5.0f;


    private Animator animator;
    private Rigidbody rigidBody = null;
    private Player_Input input = null;
    private InputAction moveAction = null;
    private InputAction jabs = null;
    private InputAction Block = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing!");
        }
    }

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            Debug.LogError("Rigidbody component is missing!");
        }

        input = new Player_Input();

        if (isPlayer2) // Player 2 controls
        {
            moveAction = input.Player2.Move;
            jabs = input.Player2.Jabs;
            Block = input.Player2.Block;
        }
        else // Player 1 controls
        {
            moveAction = input.Player.Move;
            jabs = input.Player.Jabs;
            Block = input.Player.Block;
        }
    }

    private void OnEnable()
    {
        input.Enable();

        if (moveAction != null)
        {
            moveAction.Enable();
        }
        else
        {
            Debug.LogWarning("Move action is not assigned!");
        }

        if (jabs != null)
        {
            jabs.Enable();
            jabs.performed += JabsPerformend;
        }
        else
        {
            Debug.LogWarning("Jabs action is not assigned!");
        }

        if (Block != null)
        {
            Block.Enable();
            Block.performed += BlockPerformed;
            Block.canceled += BlockCanceled; 
        }
    }

    private void OnDisable()
    {
        input.Disable();

        if (moveAction != null)
        {
            moveAction.Disable();
        }

        if (jabs != null)
        {
            jabs.Disable();
        }

        if (Block != null)
        {
            Block.Disable();
            Block.performed -= BlockPerformed;
            Block.canceled -= BlockCanceled; 
        }
    }

    private void Update()
    {
        if (moveAction == null || rigidBody == null)
        {
            Debug.LogWarning("Move action or Rigidbody is not assigned!");
            return;
        }

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
    }

    private void JabsPerformend(InputAction.CallbackContext context)
    {
        // Determine which jab to trigger based on the input
        if (context.control.name == "q" || context.control.name == "rightButton") // Left Jab
        {
            animator.SetTrigger("LJab");
            Debug.Log("Left Punch Thrown!");
        }
        else if (context.control.name == "e" || context.control.name == "leftButton") // Right Jab
        {
            animator.SetTrigger("RJab");
            Debug.Log("Right Punch Thrown!");
        }
    }

    private bool isBlocking = false; 
    private void BlockPerformed(InputAction.CallbackContext context)
    {
        isBlocking = true;
        animator.SetBool("IsBlocking", true); 
        Debug.Log("Player started blocking.");
    }

    private void BlockCanceled(InputAction.CallbackContext context)
    {
        
        isBlocking = false;
        animator.SetBool("IsBlocking", false); 
        Debug.Log("Player stopped blocking.");
    }

    public void OnHit(Vector3 hitSourcePosition, float force)
    {

        // Direction from the source to the player -> push player away from the source
        Vector3 direction = transform.position - hitSourcePosition;
        direction.y = 0f; // keep the push horizontal; remove this line if you want vertical component
        if (direction == Vector3.zero)
        {
            direction = transform.forward; // fallback if positions coincide
        }
        direction.Normalize();
        rigidBody.AddForce(direction * force, ForceMode.Impulse);
    }

    // React when something tagged "hurtbox" hits this player.
    // Works whether the hurtbox is a trigger or a non-trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("HurtBox"))
        {
            return;
        }

        // Prefer attached rigidbody center if available, otherwise use collider closest point
        Vector3 sourcePos;
        if (other.attachedRigidbody != null)
        {
            sourcePos = other.attachedRigidbody.worldCenterOfMass;
        }
        else
        {
            sourcePos = other.ClosestPoint(transform.position);
        }
        OnHit(sourcePos, pushForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        if (!other.CompareTag("HurtBox"))
        {
            return;
        }
        Vector3 sourcePos;
        if (collision.contacts != null && collision.contacts.Length > 0)
        {
            sourcePos = collision.contacts[0].point;
        }
        else
        {
            if (other.attachedRigidbody != null)
            {
                sourcePos = other.attachedRigidbody.worldCenterOfMass;
            }
            else
            {
                sourcePos = other.transform.position;
            }
        }
        OnHit(sourcePos, pushForce);
    }
}