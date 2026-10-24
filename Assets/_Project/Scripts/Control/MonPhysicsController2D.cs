using UnityEngine;

/// <summary>
/// V1.7.1: motor fisico compartilhado por PlayerMon e EnemyMon.
/// Os controladores (jogador/IA) apenas enviam intencao de movimento.
/// V1.8.5: velocidade de caminhada no solo deriva da anatomia atual.
/// V1.8.6: controle horizontal parcial no ar e parada imediata no pouso normal.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class MonPhysicsController2D : MonoBehaviour
{
    [Header("Gravidade universal do Mon (unidades/s²)")]
    [Min(0.1f)] public float gravityAcceleration = 18f;

    [Header("V1.8.5 - Caminhada anatomica no solo (teste)")]
    [Tooltip("Contribuicao da extensao das pernas a velocidade horizontal.")]
    [Min(0f)] public float legExtensionSpeedGain = 0.25f;
    [Tooltip("Penalidade da massa geral sobre a velocidade horizontal.")]
    [Min(0f)] public float generalMassSpeedPenalty = 0.08f;
    [Tooltip("Limites provisórios; nao representam balanceamento definitivo.")]
    [Min(0.1f)] public float minimumGroundSpeedMultiplier = 0.5f;
    [Min(0.1f)] public float maximumGroundSpeedMultiplier = 2f;

    [Header("V1.8.6 - Direcao horizontal no ar (teste)")]
    [Tooltip("Velocidade de correcao horizontal no ar com direcao pressionada (u/s²).")]
    [Min(0f)] public float airSteeringAcceleration = 8f;
    [Tooltip("Perda gradual de velocidade horizontal no ar ao soltar a direcao (u/s²).")]
    [Min(0f)] public float airReleaseDeceleration = 2f;

    [Header("Deteccao de solo")]
    [Tooltip("Selecione APENAS a layer de piso (ex.: ArenaGround), nunca a layer dos Mons.")]
    public LayerMask groundLayers;
    [Min(0.01f)] public float groundProbeDepth = 0.10f;

    [Header("Limites horizontais da arena")]
    public bool clampX = true;
    public float minX = -5f;
    public float maxX = 5f;

    private Rigidbody2D body;
    private CapsuleCollider2D bodyCollider;
    private CombatantStateMachine stateMachine;
    private Health health;
    private MonAnatomy anatomy;

    private float inputX;
    private float requestedSpeed;
    private float knockbackControlUntil;
    private bool isGrounded;
    private float lastReportedGroundMultiplier = -1f;
    private float lastReportedRequestedSpeed = -1f;
    private bool sampledGroundedState;
    private bool groundedLastPhysicsStep;
    private float lastLoggedAirInput = 2f; // sentinel para logar o primeiro comando no ar

    // Diagnostico da V1.7.3: mede a altura realmente percorrida pelo Rigidbody2D.
    private bool trackingJumpApex;
    private float jumpStartY;
    private float jumpMaxY;
    private float jumpInitialSpeed;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        stateMachine = GetComponent<CombatantStateMachine>();
        health = GetComponent<Health>();
        anatomy = GetComponent<MonAnatomy>();

        body.bodyType = RigidbodyType2D.Dynamic;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        float worldGravity = Mathf.Abs(Physics2D.gravity.y);
        body.gravityScale = worldGravity > 0.001f ? gravityAcceleration / worldGravity : 0f;

        if (groundLayers.value == 0)
        {
            Debug.LogWarning($"[V1.7.1] {name}: configure Ground Layers no MonPhysicsController2D.");
        }
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        TrackJumpApex();

        // O estado anterior e guardado por passo FISICO: TryJump muda
        // isGrounded no Update, entre dois FixedUpdates.
        bool justTookOff = sampledGroundedState && groundedLastPhysicsStep && !isGrounded;
        bool justLanded = sampledGroundedState && !groundedLastPhysicsStep && isGrounded;
        groundedLastPhysicsStep = isGrounded;
        sampledGroundedState = true;
        if (isGrounded) lastLoggedAirInput = 2f;

        if (health != null && health.IsDead)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        if (Time.time >= knockbackControlUntil)
        {
            bool locked = stateMachine != null && stateMachine.IsLocked;
            float direction = Mathf.Clamp(inputX, -1f, 1f);
            float movementFactor = CalculateGroundSpeedMultiplier();
            // O alvo horizontal continua derivado de Pernas.Extensao/MassaGeral.
            // Apenas a RESPOSTA ao comando e diferente no ar.
            float desiredX = locked ? 0f :
                direction * Mathf.Max(0f, requestedSpeed) * movementFactor;

            float beforeX = body.linearVelocity.x;
            float nextX;
            if (locked)
            {
                // Preserva a prioridade existente de estados de combate.
                nextX = 0f;
            }
            else if (isGrounded)
            {
                // Caminhada responsiva; sem comando no pouso, freia imediatamente.
                nextX = desiredX;
            }
            else
            {
                // PHY-089: correcao parcial no ar, nao troca instantanea.
                // Sem comando, o impulso horizontal se dissipa gradualmente.
                float acceleration = Mathf.Abs(direction) > 0.01f
                    ? airSteeringAcceleration : airReleaseDeceleration;
                nextX = Mathf.MoveTowards(
                    beforeX, desiredX, Mathf.Max(0f, acceleration) * Time.fixedDeltaTime);
            }

            if (isGrounded && !locked && Mathf.Abs(direction) > 0.01f && requestedSpeed > 0f &&
                (Mathf.Abs(movementFactor - lastReportedGroundMultiplier) > 0.005f ||
                 Mathf.Abs(requestedSpeed - lastReportedRequestedSpeed) > 0.005f))
            {
                Debug.Log(
                    $"[V1.8.5 CAMINHADA] {name}: pernasExtensao={Mathf.Max(0f, anatomy?.pernas?.extensao ?? 0f):0.###}; " +
                    $"massaGeral={Mathf.Max(0f, anatomy?.MassaGeral ?? 0f):0.###}; " +
                    $"velocidadeBase={requestedSpeed:0.###}; fator=x{movementFactor:0.###}; " +
                    $"velocidadeSolo={Mathf.Abs(desiredX):0.###} u/s.", this);
                lastReportedGroundMultiplier = movementFactor;
                lastReportedRequestedSpeed = requestedSpeed;
            }

            if (justLanded && !locked)
                Debug.Log(
                    $"[V1.8.6 POUSO] {name}: entrada={direction:0.##}; " +
                    $"velAntes={beforeX:0.###}; velDepois={nextX:0.###}; " +
                    $"semComando={(Mathf.Abs(direction) < 0.01f ? "SIM" : "NAO")}.", this);

            // Logar entrada/soltura/troca de direcao, nunca por frame.
            if (!isGrounded && !locked &&
                (justTookOff || Mathf.Abs(direction - lastLoggedAirInput) > 0.01f))
            {
                Debug.Log(
                    $"[V1.8.6 AR] {name}: entrada={direction:0.##}; " +
                    $"velAntes={beforeX:0.###}; velDepois={nextX:0.###}; " +
                    $"alvo={desiredX:0.###}; fatorAnatomico=x{movementFactor:0.###}; " +
                    $"aceleracao={(Mathf.Abs(direction) > 0.01f ? airSteeringAcceleration : airReleaseDeceleration):0.###}.", this);
                lastLoggedAirInput = direction;
            }

            body.linearVelocity = new Vector2(nextX, body.linearVelocity.y);
        }

        if (clampX)
        {
            Vector2 position = body.position;
            float x = Mathf.Clamp(position.x, minX, maxX);

            if (!Mathf.Approximately(x, position.x))
            {
                body.position = new Vector2(x, position.y);
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
            else if ((position.x <= minX && body.linearVelocity.x < 0f)
                  || (position.x >= maxX && body.linearVelocity.x > 0f))
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
        }
    }

    // V1.8.5 / PHY-087: velocidade horizontal no solo depende de
    // Pernas.Extensao E MassaGeral, sem alterar gravidade, colisor ou sprite.
    // Coeficientes e limites sao exclusivamente hipoteses para teste no Unity.
    public float CalculateGroundSpeedMultiplier()
    {
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();
        if (anatomy == null || anatomy.pernas == null) return 1f;

        float legExtension = Mathf.Max(0f, anatomy.pernas.extensao);
        float totalMass = Mathf.Max(0f, anatomy.MassaGeral);
        float drive = 1f + legExtension * Mathf.Max(0f, legExtensionSpeedGain);
        float inertia = Mathf.Sqrt(1f + totalMass * Mathf.Max(0f, generalMassSpeedPenalty));
        float rawMultiplier = drive / Mathf.Max(0.01f, inertia);
        float minimum = Mathf.Max(0.1f, minimumGroundSpeedMultiplier);
        float maximum = Mathf.Max(minimum, maximumGroundSpeedMultiplier);
        return Mathf.Clamp(rawMultiplier, minimum, maximum);
    }

    [ContextMenu("V1.8.5 - Mostrar velocidade de caminhada")]
    public void LogGroundSpeedCalculation()
    {
        Debug.Log(
            $"[V1.8.5 CAMINHADA] {name}: pernasExtensao={Mathf.Max(0f, anatomy?.pernas?.extensao ?? 0f):0.###}; " +
            $"massaGeral={Mathf.Max(0f, anatomy?.MassaGeral ?? 0f):0.###}; " +
            $"velocidadeBase={requestedSpeed:0.###}; fator=x{CalculateGroundSpeedMultiplier():0.###}; " +
            $"velocidadeSolo={requestedSpeed * CalculateGroundSpeedMultiplier():0.###} u/s.", this);
    }

    /// <summary>Recebe apenas direcao e velocidade; nao altera o Transform.</summary>
    public void SetHorizontalMovement(float direction, float speed)
    {
        inputX = direction;
        requestedSpeed = speed;
    }

    public void StopHorizontalMovement()
    {
        inputX = 0f;
        requestedSpeed = 0f;
    }

    public bool TryJump(float jumpSpeed)
    {
        if (!isGrounded || jumpSpeed <= 0f || (health != null && health.IsDead))
            return false;

        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
        isGrounded = false;

        trackingJumpApex = true;
        jumpStartY = body.position.y;
        jumpMaxY = jumpStartY;
        jumpInitialSpeed = jumpSpeed;
        return true;
    }

    private void TrackJumpApex()
    {
        if (!trackingJumpApex) return;

        jumpMaxY = Mathf.Max(jumpMaxY, body.position.y);
        if (body.linearVelocity.y > 0f) return;

        trackingJumpApex = false;
        Debug.Log(
            $"[V1.7.3 APEX] {name}: alturaReal={jumpMaxY - jumpStartY:0.###} u; " +
            $"inicioY={jumpStartY:0.###}; picoY={jumpMaxY:0.###}; " +
            $"impulsoInicial={jumpInitialSpeed:0.###} u/s."
        );
    }

    /// <summary>
    /// Acrescenta uma variacao de velocidade de impacto ao corpo.
    /// V1.7.1: valor de teste, sem calculo anatomico de impulso.
    /// </summary>
    public void ApplyKnockback(Vector2 velocityDelta, float controlLockSeconds)
    {
        if (health != null && health.IsDead) return;

        body.linearVelocity += velocityDelta;
        knockbackControlUntil = Mathf.Max(knockbackControlUntil, Time.time + Mathf.Max(0f, controlLockSeconds));
    }

    private void UpdateGrounded()
    {
        if (groundLayers.value == 0 || bodyCollider == null)
        {
            isGrounded = false;
            return;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 probeCenter = new Vector2(bounds.center.x, bounds.min.y - groundProbeDepth * 0.5f);
        Vector2 probeSize = new Vector2(bounds.size.x * 0.75f, groundProbeDepth);
        isGrounded = Physics2D.OverlapBox(probeCenter, probeSize, 0f, groundLayers) != null
                  && body.linearVelocity.y <= 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null) return;

        Bounds bounds = capsule.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(bounds.center.x, bounds.min.y - groundProbeDepth * 0.5f, transform.position.z),
            new Vector3(bounds.size.x * 0.75f, groundProbeDepth, 0f)
        );
    }
}
