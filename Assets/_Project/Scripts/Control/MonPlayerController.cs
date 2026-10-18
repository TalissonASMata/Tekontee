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

    [Header("Morfologia por acerto")]
    public MorphInfluence primaryInfluence = new MorphInfluence(MorphPointType.Attack, 1);
    public MorphInfluence secondaryInfluence = new MorphInfluence(MorphPointType.Defense, 0);
    public MorphInfluence tertiaryInfluence = new MorphInfluence(MorphPointType.Energy, 0);

    [Header("Forma dominante")]
    public MonFormGameplayStats formStats;

    private float nextAttackTime;
    private float verticalVelocity;
    private bool isGrounded;

    private Health selfHealth;
    private CombatantStateMachine stateMachine;
    private MorphPointBank morphPointBank;
    private MoveMorphologyEvaluator moveEvaluator;
    private UniversalMoveSet universalMoves;
    private InputAction heavyAction;
    private InputAction aerialAction;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        stateMachine = GetComponent<CombatantStateMachine>();
        morphPointBank = GetComponent<MorphPointBank>();
        moveEvaluator = GetComponent<MoveMorphologyEvaluator>();
        universalMoves = GetComponent<UniversalMoveSet>();

        if (formStats == null)
        {
            formStats = GetComponent<MonFormGameplayStats>();
        }
    }

    private void OnEnable()
    {
        // Os comandos logicos sao comuns a teclado e gamepad.
        heavyAction = new InputAction("HeavyStrike", InputActionType.Button);
        heavyAction.AddBinding("<Keyboard>/j");
        heavyAction.AddBinding("<Keyboard>/enter");
        heavyAction.AddBinding("<Gamepad>/buttonSouth");

        aerialAction = new InputAction("AerialStrike", InputActionType.Button);
        aerialAction.AddBinding("<Keyboard>/k");
        aerialAction.AddCompositeBinding("OneModifier")
            .With("Modifier", "<Keyboard>/leftShift")
            .With("Binding", "<Keyboard>/j");
        aerialAction.AddCompositeBinding("OneModifier")
            .With("Modifier", "<Gamepad>/rightShoulder")
            .With("Binding", "<Gamepad>/buttonSouth");
        heavyAction.Enable();
        aerialAction.Enable();
    }

    private void OnDisable()
    {
        heavyAction?.Disable();
        aerialAction?.Disable();
        heavyAction?.Dispose();
        aerialAction?.Dispose();
        heavyAction = null;
        aerialAction = null;
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
        float horizontalInput = 0f;

        if (Keyboard.current != null && (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed))
        {
            horizontalInput -= 1f;
        }

        if (Keyboard.current != null && (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed))
        {
            horizontalInput += 1f;
        }

        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();
            horizontalInput = Mathf.Abs(stick) > 0.2f ? stick : Gamepad.current.dpad.left.isPressed ? -1f : Gamepad.current.dpad.right.isPressed ? 1f : horizontalInput;
        }

        Vector3 position = transform.position;

        float finalMoveSpeed = moveSpeed * GetMoveSpeedMultiplier();

        position.x += horizontalInput * finalMoveSpeed * Time.deltaTime;

        bool jumpPressed =
            (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)) ||
            (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

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
        if (Time.time < nextAttackTime || target == null || target.IsDead) return;

        bool chordHeld = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
                         (Gamepad.current != null && Gamepad.current.rightShoulder.isPressed);
        bool aerialPressed = aerialAction != null && aerialAction.WasPressedThisFrame();
        bool heavyPressed = heavyAction != null && heavyAction.WasPressedThisFrame();

        // A combinacao tem prioridade sobre o golpe de um botao.
        // Mesmo no quadro em que ambos os botoes sao pressionados juntos,
        // uma combinacao nao executa dois movimentos.
        UniversalMoveId selectedMove;
        if (aerialPressed || (chordHeld && heavyPressed))
            selectedMove = UniversalMoveId.AerialStrike;
        else if (heavyPressed)
            selectedMove = UniversalMoveId.HeavyStrike;
        else
            return;

        if (universalMoves != null)
        {
            if (!universalMoves.TryExecute(selectedMove, target))
            {
                Debug.Log($"{gameObject.name} tentou {selectedMove}, mas estava fora do alcance ou sem definicao.");
                return;
            }

            SetState(CombatState.Attacking, 0.12f);
            ApplyBonusRepelToTarget();
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // Compatibilidade com a cena anterior caso o novo componente nao esteja instalado.
        float distance = Mathf.Abs(target.transform.position.x - transform.position.x);
        if (distance > attackRange)
        {
            Debug.Log($"{gameObject.name} atacou, mas o alvo estava fora do alcance.");
            return;
        }

        SetState(CombatState.Attacking, 0.12f);
        MoveExecutionQuality quality = EvaluateMoveQuality();
        int finalDamage = GetFinalDamage(quality);
        target.TakeDamage(finalDamage, transform);
        ApplyBonusRepelToTarget();

        Debug.Log(
            $"Ataque manual do Mon acertou com qualidade {quality} e dano final {finalDamage}. " +
            $"Requisito: {GetRequirementDebugText()}. " +
            $"Forma dominante: {GetCurrentFormName()}"
        );

        AddMorphInfluencesOnHit(quality);
        nextAttackTime = Time.time + attackCooldown;
    }

    private MoveExecutionQuality EvaluateMoveQuality()
    {
        return moveEvaluator != null
            ? moveEvaluator.Evaluate()
            : MoveExecutionQuality.Effective;
    }

    private int GetFinalDamage(MoveExecutionQuality quality)
    {
        float formMultiplier = formStats != null ? formStats.DamageMultiplier : 1f;
        float qualityMultiplier = moveEvaluator != null
            ? moveEvaluator.GetDamageMultiplier(quality)
            : 1f;

        return Mathf.Max(1, Mathf.RoundToInt(damage * formMultiplier * qualityMultiplier));
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

    private string GetRequirementDebugText()
    {
        if (moveEvaluator == null) return "sem avaliador (Effective por compatibilidade)";

        return
            $"{moveEvaluator.requiredType} {moveEvaluator.CurrentRequiredValue} " +
            $"(Effective >= {moveEvaluator.effectiveThreshold}, Critical >= {moveEvaluator.criticalThreshold})";
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

    private void AddMorphInfluencesOnHit(MoveExecutionQuality quality)
    {
        if (morphPointBank == null) return;

        float reinforcementMultiplier = moveEvaluator != null
            ? moveEvaluator.GetReinforcementMultiplier(quality)
            : 1f;

        morphPointBank.AddInfluences(
            primaryInfluence,
            secondaryInfluence,
            tertiaryInfluence,
            reinforcementMultiplier
        );
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
