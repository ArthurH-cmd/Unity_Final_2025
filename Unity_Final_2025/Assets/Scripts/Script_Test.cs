using NUnit.Framework;
using System.Collections;
using Unity.Mathematics;
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
    public float heavyPushForce = 100.0f;
    [SerializeField]
    public float jabPunchForce = 50.0f;

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
    private ParticleSystem sheild_particals;
    private Rigidbody rigidBody = null;
    private Player_Input input = null;
    private InputAction moveAction = null;
    private InputAction jabs = null;
    private InputAction Block = null;
    private InputAction Heavy = null;

    public bool wutPunch = false; // true is lightPunch , False is heavy

    public Spectators crowd;

    public float PlayerHealthMax
    {
        get { return MaxPlayerHealth; }
    }

    public float PlayerHealthCurrent
    {
        get { return currentPlayerHealth; }
    }

    private void Start()
    {
        currentPlayerHealth = MaxPlayerHealth;
        currentGuardHealth = MaxGuardHealth;

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing!");
        }
        sheild_particals = GetComponentInChildren<ParticleSystem>();

        if (sheild_particals == null)
        {
            Debug.LogError("ParticleSystem component is missing!");
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

        currentPlayerHealth = MaxPlayerHealth;
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

    private void randomJab()
    {
        int randomJab = UnityEngine.Random.Range(0, 2);

        if (randomJab == 1)
        {
            wutPunch = true;
            animator.SetTrigger("RJab");
            Debug.Log("Right Punch Thrown!");

        }
        else
        {
            wutPunch = true; // jab
            animator.SetTrigger("LJab");
            Debug.Log("Left Punch Thrown!");

        }
    }

    private void randomHeavy()
    {
        int randomJab = UnityEngine.Random.Range(0, 2);

        if (randomJab == 1)
        {
            wutPunch = false;
            animator.SetTrigger("RHeavy");

        }
        else // add L-Heavy here
        {
            wutPunch = false;
            animator.SetTrigger("RHeavy");

        }
    }


    private void JabsPerformend(InputAction.CallbackContext context)
    {
        if (isPlayer2)
        {

            if (context.control.name == "leftButton") // Right Jab
            {
                randomJab();

            }
        }

        else
        {
            if (context.control.name == "e") // Right Jab
            {
                randomJab();

            }
        }
    }

    private void HeavyPunchPerformed(InputAction.CallbackContext context)
    {
        if (isPlayer2)
        {
            if (context.control.name == "rightButton")
            {
                randomHeavy();
            }
        }

        else
        {
            if (context.control.name == "q")
            {
                randomHeavy();
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

    public void OnHit(Vector3 hitSourcePosition, Script_Test attacker)
    {
        musicBox.Play();

        crowd?.OnHitOccurred(hitSourcePosition, !attacker.wutPunch); // crowd reacts to the hit

        // Use ATTACKER's wutPunch, not yours!
        int damage = attacker.wutPunch ? 10 : 30;
        float pushForce = attacker.wutPunch ? attacker.jabPunchForce : attacker.heavyPushForce;

        if (isBlocking && !shieldBroken)
        {
            currentGuardHealth -= damage;
            ShieldHealthColor();
            sheild_particals.Play();
            StartCoroutine(DisableColliderBriefly());

            Debug.Log($"Blocked {(attacker.wutPunch ? "JAB" : "HEAVY")}: {currentGuardHealth} shield left");

            if (currentGuardHealth <= 0)
            {
                StartCoroutine(BreakShield());
            }
        }
        else
        {
            currentPlayerHealth -= damage;
            Debug.Log($"Hit by {(attacker.wutPunch ? "JAB" : "HEAVY")}: {currentPlayerHealth} health left");
            StartCoroutine(DisableColliderBriefly());

            if (currentPlayerHealth <= 0)
            {
                crowd.OnPlayerDefeated();
                moveAction.Disable();
                jabs.Disable();
                Heavy.Disable();
                Debug.Log("Player defeated!");

                attacker.jabs.Disable();
                attacker.Heavy.Disable();
            }
        }

        Vector3 pushDirection = (transform.position - hitSourcePosition).normalized;
        pushDirection.y = 0f;

        if (pushDirection == Vector3.zero)
            pushDirection = transform.forward;

        rigidBody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    private IEnumerator DisableColliderBriefly()
    {
        Collider col = GetComponent<Collider>();
        col.enabled = false;
        yield return new WaitForSeconds(0.5f);
        col.enabled = true;
    }

    private void ShieldHealthColor()
    {
        var main = sheild_particals.main;

        float healthPercent = currentGuardHealth / MaxGuardHealth;

        Color newColor = Color.Lerp(Color.darkRed, Color.blue, healthPercent); // you can switch the colors here (it goes from red to blue as health goes from 0 to max)

        main.startColor = newColor;
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
        // Get who punched you
        Script_Test attacker = other.GetComponentInParent<Script_Test>();
        if (attacker == null || attacker == this) // Don't hit yourself
        {
            return;
        }

        Vector3 sourcePos;
        if (other.attachedRigidbody != null) 
        {
            sourcePos = other.attachedRigidbody.worldCenterOfMass;
        }
        else
        {
            sourcePos = other.ClosestPoint(transform.position);
        }
        OnHit(sourcePos, attacker);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("HurtBox"))
        {
            Script_Test attacker = collision.collider.GetComponentInParent<Script_Test>();

            if (attacker != null && attacker != this)
            {
                Vector3 sourcePos;

                if (collision.contacts.Length > 0)
                {
                    sourcePos = collision.contacts[0].point;
                }
                else
                {
                    sourcePos = collision.collider.transform.position;
                }

                OnHit(sourcePos, attacker);
            }
        }
    }

}