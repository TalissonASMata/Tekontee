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
    private MonPhysicsController2D physicsController;
    private MonAnatomicalJumpStats anatomicalJumpStats;
    private MonBasicStrikeSet basicStrikeSet;
    private InputAction weakPunchAction;
    private InputAction weakKickAction;
    private InputAction heavyAction;
    private InputAction aerialAction;
    private InputAction defenseAction;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        stateMachine = GetComponent<CombatantStateMachine>();
        morphPointBank = GetComponent<MorphPointBank>();
        moveEvaluator = GetComponent<MoveMorphologyEvaluator>();
        universalMoves = GetComponent<UniversalMoveSet>();
        physicsController = GetComponent<MonPhysicsController2D>();
        anatomicalJumpStats = GetComponent<MonAnatomicalJumpStats>();
        basicStrikeSet = GetComponent<MonBasicStrikeSet>();

        if (formStats == null)
        {
            formStats = GetComponent<MonFormGameplayStats>();
        }
    }

    private void OnEnable()
    {
        // Golpes basicos: mesmas acoes logicas para teclado e controle.
        weakPunchAction = new InputAction("WeakPunch", InputActionType.Button);
        weakPunchAction.AddBinding("<Keyboard>/f");
        weakPunchAction.AddBinding("<Gamepad>/buttonWest"); // Quadrado no PS

        weakKickAction = new InputAction("WeakKick", InputActionType.Button);
        weakKickAction.AddBinding("<Keyboard>/v");
        weakKickAction.AddBinding("<Gamepad>/buttonSouth"); // X no PS

        weakPunchAction.Enable();
        weakKickAction.Enable();

        // Golpes genericos V1.6 ficam somente no teclado J/K/L para
        // liberar os face buttons do gamepad aos novos golpes anatomicos.
        heavyAction = new InputAction("HeavyStrike", InputActionType.Button);
        heavyAction.AddBinding("<Keyboard>/j");

        aerialAction = new InputAction("AerialStrike", InputActionType.Button);
        aerialAction.AddBinding("<Keyboard>/k");
        aerialAction.AddCompositeBinding("OneModifier")
            .With("Modifier", "<Keyboard>/leftShift")
            .With("Binding", "<Keyboard>/j");
        defenseAction = new InputAction("GuardBreak", InputActionType.Button);
        defenseAction.AddBinding("<Keyboard>/l");
        defenseAction.Enable();
        heavyAction.Enable();
        aerialAction.Enable();
    }

    private void OnDisable()
    {
        weakPunchAction?.Disable();
        weakKickAction?.Disable();
        weakPunchAction?.Dispose();
        weakKickAction?.Dispose();
        weakPunchAction = null;
        weakKickAction = null;

        defenseAction?.Disable();
        defenseAction?.Dispose();
        defenseAction = null;
        heavyAction?.Disable();
        aerialAction?.Disable();
        heavyAction?.Dispose();
        aerialAction?.Dispose();
        heavyAction = null;
        aerialAction = null;
    }

    private void Start()
    {
        if (physicsController != null) return;

        if (useInitialYAsGround)
        {
            groundY = transform.position.y;
        }

        isGrounded = true;
    }

    private void Update()
    {
        // Quando o jogador controla o Tamer, a IA e dona da intencao de movimento.
        // Nao limpar o comando do AutoFighter a cada Update.
        if (!canControl) return;

        if (selfHealth != null && selfHealth.IsDead)
        {
            physicsController?.StopHorizontalMovement();
            SetState(CombatState.Dead);
            return;
        }

        if (stateMachine != null && stateMachine.IsLocked)
        {
            physicsController?.StopHorizontalMovement();
            return;
        }

        HandleMovementAndJump();
        HandleAttack();
    }

    private void HandleMovementAndJump()
    {
        float horizontalInput = 0f;

        if (Keyboard.current != null && Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput -= 1f;
        }

        if (Keyboard.current != null && Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput += 1f;
        }

        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();
            horizontalInput = Mathf.Abs(stick) > 0.2f ? stick : Gamepad.current.dpad.left.isPressed ? -1f : Gamepad.current.dpad.right.isPressed ? 1f : horizontalInput;
        }

        float finalMoveSpeed = moveSpeed * GetMoveSpeedMultiplier();

        // V1.7.1: quando existe motor fisico, ele e o unico dono da posicao.
        // Sem ele, continua disponivel o movimento legado da V1.6.
        if (physicsController != null)
        {
            physicsController.SetHorizontalMovement(horizontalInput, finalMoveSpeed);
        }

        Vector3 position = transform.position;
        if (physicsController == null)
        {
            position.x += horizontalInput * finalMoveSpeed * Time.deltaTime;
        }

        // Teclado: somente seta para cima. Gamepad: salto ainda sem mapeamento
        // definitivo; buttonNorth/triangulo fica reservado para soco forte.
        bool jumpPressed =
            Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (physicsController != null)
        {
            if (jumpPressed)
            {
                // Com o novo componente, o salto depende da anatomia atual.
                // Sem ele, a V1.7.1 continua usando o multiplicador legado.
                float jumpSpeed = anatomicalJumpStats != null && anatomicalJumpStats.enabled
                    ? anatomicalJumpStats.CalculateJumpSpeed(jumpForce)
                    : jumpForce * GetJumpMultiplier();

                if (physicsController.TryJump(jumpSpeed))
                {
                    if (anatomicalJumpStats != null && anatomicalJumpStats.enabled)
                    {
                        Debug.Log(
                            $"[V1.7.3 SALTO] {name}: impulso={jumpSpeed:0.###} u/s; " +
                            $"fatorAnatomico=x{anatomicalJumpStats.CalculateMultiplier():0.###}."
                        );
                    }
                    else
                    {
                        Debug.Log($"[V1.7.1] {name} saltou com motor fisico.");
                    }
                }
            }
        }
        else
        {
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
        }

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
        // V1.7.5: soco/chute anatomicos tem prioridade e compartilham
        // cooldown e estado de ataque com os movimentos legados.
        bool punchPressed = weakPunchAction != null && weakPunchAction.WasPressedThisFrame();
        bool kickPressed = weakKickAction != null && weakKickAction.WasPressedThisFrame();

        if (punchPressed || kickPressed)
        {
            MonBasicStrikeId strike = punchPressed
                ? MonBasicStrikeId.WeakPunch
                : MonBasicStrikeId.WeakKick;

            if (Time.time < nextAttackTime)
            {
                Debug.Log($"[V1.7.5 INPUT] {name}: {strike} bloqueado por cooldown.");
                return;
            }

            if (target == null || target.IsDead)
            {
                Debug.Log($"[V1.7.5 INPUT] {name}: {strike} sem alvo valido.");
                return;
            }

            if (basicStrikeSet == null || !basicStrikeSet.enabled)
            {
                Debug.LogWarning(
                    $"[V1.7.5 INPUT] {name}: adicione Mon Basic Strike Set ao objeto Mon.",
                    this
                );
                return;
            }

            // Uma tentativa gasta cooldown mesmo quando o golpe erra.
            // Quando F e V chegam no mesmo frame, soco tem prioridade.
            if (!basicStrikeSet.TryExecute(strike, target)) return;

            SetState(CombatState.Attacking, 0.12f);
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // V1.6: mantem J/K/L e Shift+J para testes de regressao.
        // As bindings antigas de gamepad foram desativadas para nao
        // disputar Quadrado/X com os golpes basicos.
        bool keyboardJ = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
        bool keyboardK = Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
        bool keyboardL = Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame;
        bool keyboardShiftHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool keyboardShiftPressed = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
        bool keyboardJHeld = Keyboard.current != null && Keyboard.current.jKey.isPressed;

        bool chordAfterBase = keyboardShiftPressed && keyboardJHeld && !keyboardJ;
        bool inputEvent = keyboardJ || keyboardK || keyboardL || chordAfterBase;
        if (!inputEvent) return;

        bool keyboardChord = keyboardShiftHeld && keyboardJ;
        bool aerialPressed = aerialAction != null && aerialAction.WasPressedThisFrame();
        bool heavyPressed = heavyAction != null && heavyAction.WasPressedThisFrame();
        bool defensePressed = defenseAction != null && defenseAction.WasPressedThisFrame();

        UniversalMoveId selectedMove = keyboardL || defensePressed
            ? UniversalMoveId.GuardBreak
            : keyboardChord || keyboardK || aerialPressed
                ? UniversalMoveId.AerialStrike
                : UniversalMoveId.HeavyStrike;

        string inputSource = keyboardL ? "Teclado L" :
            keyboardK ? "Teclado K" :
            keyboardChord ? "Teclado Shift+J" :
            keyboardJ ? "Teclado J" :
            chordAfterBase ? "Modificador apos botao-base" : "Outro";

        if (chordAfterBase)
        {
            Debug.Log($"[V1.6 INPUT] {gameObject.name}: {inputSource}; nenhum novo golpe iniciado. " +
                      "A ordem inversa depende de uma janela de combinacao, ainda nao implementada.");
            return;
        }

        string blocked = Time.time < nextAttackTime ? "cooldown" :
            target == null ? "alvo ausente" :
            target.IsDead ? "alvo morto" : "nenhum";
        Debug.Log($"[V1.6 INPUT] {gameObject.name}: origem={inputSource}; " +
                  $"heavyAction={heavyPressed}; aerialAction={aerialPressed}; defenseAction={defensePressed}; " +
                  $"resolvido={selectedMove}; bloqueio={blocked}");

        if (blocked != "nenhum") return;

        if (universalMoves != null)
        {
            UniversalMoveDefinition definition = universalMoves.Find(selectedMove);
            if (definition == null)
            {
                Debug.LogWarning($"[V1.6 INPUT] {gameObject.name}: movimento {selectedMove} nao definido.");
                return;
            }
            float distance = Mathf.Abs(target.transform.position.x - transform.position.x);
            if (distance > definition.range)
            {
                Debug.Log($"[V1.6 INPUT] {gameObject.name}: {selectedMove} fora do alcance " +
                          $"(distancia {distance:0.00}, alcance {definition.range:0.00}).");
                return;
            }
            if (!universalMoves.TryExecute(selectedMove, target)) return;

            SetState(CombatState.Attacking, 0.12f);
            ApplyBonusRepelToTarget();
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // Compatibilidade com a cena V1.5 sem UniversalMoveSet.
        float legacyDistance = Mathf.Abs(target.transform.position.x - transform.position.x);
        if (legacyDistance > attackRange)
        {
            Debug.Log($"[V1.6 INPUT] {gameObject.name}: ataque legado fora do alcance.");
            return;
        }
        SetState(CombatState.Attacking, 0.12f);
        MoveExecutionQuality quality = EvaluateMoveQuality();
        int finalDamage = GetFinalDamage(quality);
        target.TakeDamage(finalDamage, transform);
        ApplyBonusRepelToTarget();
        Debug.Log($"Ataque manual legado do Mon: {quality}, dano {finalDamage}. " +
                  $"Requisito: {GetRequirementDebugText()}. Forma: {GetCurrentFormName()}");
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

        MonPhysicsController2D targetPhysics = target.GetComponent<MonPhysicsController2D>();
        if (targetPhysics != null)
        {
            // Impulso de teste; magnitude final sera definida na fase anatomica.
            targetPhysics.ApplyKnockback(new Vector2(directionX * bonusRepelDistance / 0.12f, 0f), 0.12f);
        }
        else
        {
            target.transform.position += new Vector3(directionX * bonusRepelDistance, 0f, 0f);
        }

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
