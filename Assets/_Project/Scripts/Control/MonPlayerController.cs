using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class MonPlayerController : MonoBehaviour
{
    [Header("Controle")]
    public bool canControl = false;

    [Header("Movimento")]
    public float moveSpeed = 3f;
    public bool clampX = true;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Pulo")]
    public float jumpForce = 4f;
    public float gravity = -18f;
    public bool useInitialYAsGround = true;
    public float groundY = 0f;

    [Header("Ataque")]
    public Health target;
    public int damage = 10;
    public float attackRange = 1.4f;
    public float attackCooldown = 0.8f;

    [Header("Repel bônus")]
    public float baseBonusRepelDistance = 0.35f;

    [Header("Morfologia")]
    public MorphPointType pointTypeOnHit = MorphPointType.Attack;
    public int pointsOnHit = 1;

    [Header("Forma dominante")]
    public MonFormGameplayStats formStats;

    private float nextAttackTime;
    private float verticalVelocity;
    private bool isGrounded;

    private Health selfHealth;
    private CombatantStateMachine stateMachine;
    private MorphPointBank morphPointBank;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        stateMachine = GetComponent<CombatantStateMachine>();
        morphPointBank = GetComponent<MorphPointBank>();

        if (formStats == null)
        {
            formStats = GetComponent<MonFormGameplayStats>();
        }
    }

    private void Start()
    {
        if (useInitialYAsGround)
        {
            groundY = transform.position.y;
        }

        isGrounded = true;
    }

    private void Update()
    {
        if (!canControl) return;

        if (selfHealth != null && selfHealth.IsDead)
        {
            SetState(CombatState.Dead);
            return;
        }

        if (stateMachine != null && stateMachine.IsLocked)
        {
            return;
        }

        HandleMovementAndJump();
        HandleAttack();
    }

    private void HandleMovementAndJump()
    {
        if (Keyboard.current == null) return;

        float horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput += 1f;
        }

        Vector3 position = transform.position;

        float finalMoveSpeed = moveSpeed * GetMoveSpeedMultiplier();

        position.x += horizontalInput * finalMoveSpeed * Time.deltaTime;

        bool jumpPressed =
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            verticalVelocity = jumpForce * GetJumpMultiplier();
            isGrounded = false;

            Debug.Log(
                $"{gameObject.name} pulou com força {verticalVelocity}. " +
                $"Forma dominante: {GetCurrentFormName()}"
            );
        }

        verticalVelocity += gravity * Time.deltaTime;
        position.y += verticalVelocity * Time.deltaTime;

        if (position.y <= groundY)
        {
            position.y = groundY;
            verticalVelocity = 0f;
            isGrounded = true;
        }

        if (clampX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        transform.position = position;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            SetState(CombatState.Moving);
        }
        else
        {
            SetState(CombatState.Idle);
        }
    }

    private void HandleAttack()
    {
        if (Keyboard.current == null) return;
        if (Time.time < nextAttackTime) return;
        if (target == null) return;
        if (target.IsDead) return;

        bool attackPressed =
            Keyboard.current.jKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame;

        if (!attackPressed) return;

        float distance = Mathf.Abs(target.transform.position.x - transform.position.x);

        if (distance > attackRange)
        {
            Debug.Log($"{gameObject.name} atacou, mas o alvo estava fora do alcance.");
            return;
        }

        SetState(CombatState.Attacking, 0.12f);

        int finalDamage = GetFinalDamage();

        target.TakeDamage(finalDamage, transform);

        ApplyBonusRepelToTarget();

        Debug.Log(
            $"Ataque manual do Mon acertou com dano final {finalDamage}. " +
            $"Forma dominante: {GetCurrentFormName()}"
        );

        AddMorphPointsOnHit();

        nextAttackTime = Time.time + attackCooldown;
    }

    private int GetFinalDamage()
    {
        float multiplier = formStats != null ? formStats.DamageMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    private float GetMoveSpeedMultiplier()
    {
        return formStats != null ? formStats.MoveSpeedMultiplier : 1f;
    }

    private float GetJumpMultiplier()
    {
        return formStats != null ? formStats.JumpMultiplier : 1f;
    }

    private float GetRepelMultiplier()
    {
        return formStats != null ? formStats.RepelMultiplier : 1f;
    }

    private string GetCurrentFormName()
    {
        if (formStats == null || !formStats.HasAnyPoints) return "Normal";
        return formStats.CurrentDominantType.ToString();
    }

    private void ApplyBonusRepelToTarget()
    {
        if (target == null) return;

        float repelMultiplier = GetRepelMultiplier();

        if (repelMultiplier <= 1f) return;

        float directionX = Mathf.Sign(target.transform.position.x - transform.position.x);
        float bonusRepelDistance = baseBonusRepelDistance * (repelMultiplier - 1f);

        target.transform.position += new Vector3(directionX * bonusRepelDistance, 0f, 0f);

        Debug.Log($"{gameObject.name} aplicou repel bônus x{repelMultiplier}.");
    }

    private void AddMorphPointsOnHit()
    {
        if (morphPointBank == null) return;
        if (pointsOnHit <= 0) return;

        morphPointBank.AddPoints(pointTypeOnHit, pointsOnHit);
    }

    private void SetState(CombatState state)
    {
        if (stateMachine == null) return;

        stateMachine.SetState(state);
    }

    private void SetState(CombatState state, float lockDuration)
    {
        if (stateMachine == null) return;

        stateMachine.SetState(state, lockDuration);
    }
}