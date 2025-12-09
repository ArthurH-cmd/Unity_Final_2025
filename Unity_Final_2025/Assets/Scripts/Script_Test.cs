using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Script_Test : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float rotationSpeed = 10.0f;
    [SerializeField]
    public Transform target;
   
    [SerializeField] // health
    private float MaxPlayerHealth = 100.0f;
    private float currentPlayerHealth;

    [SerializeField] // shelid
    private float MaxGuardHealth = 150.0f;
    private float currentGuardHealth;

    [SerializeField]
    private bool isPlayer2 = false;

    
    
    [SerializeField]
    private float heavyPushForce = 100.0f;
    [SerializeField]
    private float jabPunchForce = 50.0f;

    private float pushForce = 50.0f;

    [SerializeField] 
    private AudioSource musicBox;

    [SerializeField]
    private float shieldRegenRate = 10.0f; 
    [SerializeField] 
    private float shieldMaxIdleTime = 5.0f; // full regen 
    [SerializeField] 
    private float shieldBreakCooldown = 10.0f; // disabled block time when shield breaks

    private float shieldIdleTimer = 0f;
    private bool shieldBroken = false;
    private Animator animator;
    private Rigidbody rigidBody = null;
    private Player_Input input = null;
    private InputAction moveAction = null;
    private InputAction jabs = null;
    private InputAction Block = null;
    private InputAction Heavy = null;

    private bool wutPunch = false; // true is lightPunch , False is heavy

    private void Start()
    {
        currentPlayerHealth = MaxPlayerHealth;
        currentGuardHealth = MaxGuardHealth;

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
            Heavy = input.Player2.Heavy;
        }
        else // Player 1 controls
        {
            moveAction = input.Player.Move;
            jabs = input.Player.Jabs;
            Block = input.Player.Block;
            Heavy = input.Player.Heavy;
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

        if (Heavy != null) 
        { 
            Heavy.Enable();
            Heavy.performed += HeavyPunchPerformed;

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
        if (Heavy != null)
        {
            Heavy.Disable();
            Heavy.performed -= HeavyPunchPerformed;

        }
    }

    private void Update()
    {
        if (moveAction == null || rigidBody == null)
        {
            Debug.LogWarning("Move action or Rigidbody is not assigned!");
            return;
        }

        HandleShieldRegen();//Call shield regen handler every frame

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
        if (isPlayer2)
        {
            if (context.control.name == "rightButton") // Left Jab
            {
                wutPunch = true;
                animator.SetTrigger("LJab");
                Debug.Log("Left Punch Thrown!");

            }
            else if (context.control.name == "leftButton") // Right Jab
            {
                wutPunch = true;
                animator.SetTrigger("RJab");
                Debug.Log("Right Punch Thrown!");

            }
        }

        else
        {
            if (context.control.name == "q" ) // Left Jab
            {
                wutPunch = true; // jab
                animator.SetTrigger("LJab");
                Debug.Log("Left Punch Thrown!");

            }
            else if (context.control.name == "e") // Right Jab
            {
                wutPunch = true;
                animator.SetTrigger("RJab");
                Debug.Log("Right Punch Thrown!");

            }
        }
    }

    private void HeavyPunchPerformed(InputAction.CallbackContext context) 
    {
        if (isPlayer2)
        {
            if (context.control.name == "h")
            {
                wutPunch = false; // heavy
                animator.SetTrigger("RHeavy");
                Debug.Log("Right heavy Thrown!");
            }
        }

        else 
        {
            if (context.control.name == "g")
            {
                wutPunch = false;
                animator.SetTrigger("RHeavy");
                Debug.Log("Right heavy Thrown!");
            }
        }

        wutPunch = false;
    
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
        musicBox.Play();


        if (isBlocking) // block logic
        {
            if (wutPunch == true) // jabs
            {
                pushForce = jabPunchForce / 2;
                currentGuardHealth -= 10;
                Debug.Log($"{currentGuardHealth} left");
            }
            else // heavy
            {
                pushForce = heavyPushForce / 2;
                currentGuardHealth -= 30;
                Debug.Log($"{currentGuardHealth} left");
            }
            if (currentGuardHealth <= 0 && !shieldBroken)
            {
                StartCoroutine(BreakShield());
            }
        }
        else
        {
            if (wutPunch == true) // jabs
            {
                pushForce = jabPunchForce;
                currentPlayerHealth -= 10;
                Debug.Log($"Player Lost Health. {currentPlayerHealth} left");

            }
            else // heavy
            {
                pushForce = heavyPushForce;
                currentPlayerHealth -= 30;
                Debug.Log($"Player Lost Health. {currentPlayerHealth} left");
            }
        }
        ShieldHealthColor(); // change shield color based on health

        if (currentPlayerHealth < 0) // dies in spanish
        {
            moveAction.Disable();
            // add a gameover
        }

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
    private void ShieldHealthColor()
    {
        if (currentGuardHealth <= MaxGuardHealth * 0.3f)
        {
            // Change shield color to red
        }
        else if (currentGuardHealth <= MaxGuardHealth * 0.6f)
        {
            // Change shield color to yellow
        }
        else
        {
            // Change shield color to blue
        }
    }
    // The IEnumerator to handles shield break working as a cooldown, similar to a virtual class and Enum class
    private IEnumerator BreakShield()
    {
        shieldBroken = true;
        isBlocking = false;
        animator.SetBool("IsBlocking", false);

        Block.Disable(); // block is locked!
        Debug.Log($"Shield Broke! Blocking disabled for {shieldBreakCooldown} seconds!");

        yield return new WaitForSeconds(shieldBreakCooldown); // wait for cooldown using yield that when the condition is met it continues in the function
                                                              // the waitforseconds is a function that is part of the IEnumerator and base of C#

        Block.Enable();
        shieldBroken = false;
        currentGuardHealth = MaxGuardHealth; // full restore on break end
        Debug.Log("Shield Ready again!");
    }

    private void HandleShieldRegen()
    {
        if (isBlocking)
        {
            shieldIdleTimer = 0f; // reset because shield is being used
            return;
        }

        shieldIdleTimer += Time.deltaTime;

        // Passive regen (10 hp per second)
        if (currentGuardHealth < MaxGuardHealth && !shieldBroken)
        {
            currentGuardHealth += shieldRegenRate * Time.deltaTime;
        }
        // Full regen after being idle 5 seconds
        if (shieldIdleTimer >= shieldMaxIdleTime && !shieldBroken)
        {
            currentGuardHealth = MaxGuardHealth;
        }
        currentGuardHealth = Mathf.Clamp(currentGuardHealth, 0, MaxGuardHealth); // Mathf.Clamp to keep health within bounds because of the ranges given in the function
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