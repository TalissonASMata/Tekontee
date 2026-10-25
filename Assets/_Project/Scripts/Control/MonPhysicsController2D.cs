using System.Collections;
using UnityEngine;

/// <summary>
/// V1.7.1: motor fisico compartilhado por PlayerMon e EnemyMon.
/// Os controladores (jogador/IA) apenas enviam intencao de movimento.
/// V1.8.5: velocidade de caminhada no solo deriva da anatomia atual.
/// V1.8.6: controle horizontal parcial no ar e parada imediata no pouso normal.
/// V1.8.7: planagem opcional, suportada por arrasto anatomico dos bracos.
/// V1.8.8: resistencia da massa geral ao impulso de golpes anatomicos F/V.
/// V1.8.9: pernas em contato com solo atenuam reacao horizontal a golpes.
/// V1.8.10: interacao contextual piso/revestimento modifica reacao no solo.
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

    [Header("V1.8.7 - Planagem anatomica (hipotese de teste)")]
    [Tooltip("Minimo de arrasto dos bracos para planagem efetiva. Valor de teste.")]
    [Min(0f)] public float glideArmDragThreshold = 0.10f;
    [Tooltip("Reforco minimo no arrasto dos bracos por tentativa Fail valida.")]
    [Min(0f)] public float glideFailDragGain = 0.02f;
    [Tooltip("Reforco unico por nova planagem Effective, nunca a cada frame.")]
    [Min(0f)] public float glideEffectiveDragGain = 0.04f;
    [Tooltip("Limite superior inicial da velocidade de descida durante a planagem (u/s).")]
    [Min(0.1f)] public float glideBaseDescentSpeed = 2.5f;
    [Tooltip("Quanto o arrasto dos bracos reduz a velocidade limite de descida.")]
    [Min(0f)] public float glideDragStrength = 2f;
    [Tooltip("Velocidade de descida minima para impedir pairar indefinidamente.")]
    [Min(0.1f)] public float glideMinDescentSpeed = 0.6f;

    [Header("V1.8.8 - Resistencia ao impacto pela massa (teste)")]
    [Tooltip("Coeficiente experimental: mais massa geral atenua a velocidade recebida de um golpe.")]
    [Min(0f)] public float massImpactResistance = 0.08f;
    [Tooltip("Limite minimo do fator de impacto. Nao representa balanceamento final.")]
    [Range(0.01f, 1f)] public float minimumImpactMultiplier = 0.25f;

    [Header("V1.8.9 - Aderencia por quantidade de pernas (teste)")]
    [Tooltip("Resistencia extra por perna acima de duas, SOMENTE em solo; valores de teste.")]
    [Min(0f)] public float legCountGroundTractionGain = 0.15f;
    [Tooltip("Piso para o multiplicador horizontal de aderencia por numero de pernas.")]
    [Range(0.01f, 1f)] public float minimumLegCountTractionMultiplier = 0.5f;
    [Tooltip("Teto da reacao com menos de duas pernas (perda de aderencia).")]
    [Min(1f)] public float maximumLegCountTractionMultiplier = 1.5f;

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
    private Collider2D supportingGroundCollider;
    private float lastReportedGroundMultiplier = -1f;
    private float lastReportedRequestedSpeed = -1f;
    private bool sampledGroundedState;
    private bool groundedLastPhysicsStep;
    private float lastLoggedAirInput = 2f; // sentinel para logar o primeiro comando no ar
    private bool glideIntent;
    private bool glideAttemptedThisHold;
    private bool glideEligibleThisHold;
    private bool glideLoggedFirstDescent;

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

        UpdateAnatomicalGlide();

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

    // V1.8.7 / PHY-086, 122, 123: comando universal de tentativa.
    // O controle comunica intencao; o motor decide a capacidade anatomica.
    // A tentativa so conta enquanto o Mon esta realmente no ar.
    public void SetGlideIntent(bool held)
    {
        glideIntent = held;
        if (!held)
        {
            glideAttemptedThisHold = false;
            glideEligibleThisHold = false;
            glideLoggedFirstDescent = false;
        }
    }

    private void UpdateAnatomicalGlide()
    {
        if (isGrounded || health != null && health.IsDead)
        {
            // Permite nova tentativa no proximo salto; Down no solo nao conta.
            glideAttemptedThisHold = false;
            glideEligibleThisHold = false;
            glideLoggedFirstDescent = false;
            return;
        }
        if (!glideIntent || Time.time < knockbackControlUntil ||
            (stateMachine != null && stateMachine.IsLocked)) return;

        if (!glideAttemptedThisHold)
        {
            glideAttemptedThisHold = true;
            if (anatomy == null) anatomy = GetComponent<MonAnatomy>();

            float currentDrag = Mathf.Max(0f, anatomy?.bracos?.arrasto ?? 0f);
            glideEligibleThisHold = anatomy != null && anatomy.bracos != null &&
                currentDrag + 0.00001f >= Mathf.Max(0f, glideArmDragThreshold);

            // Similar aos golpes F/V: Falha reforca por tentativa,
            // Effective reforca uma vez por inicio, nunca por FixedUpdate.
            float delta = glideEligibleThisHold ? glideEffectiveDragGain : glideFailDragGain;
            float growth = anatomy == null ? 0f :
                anatomy.AddInfluence(MonAnatomyParameter.ArmsDrag, Mathf.Max(0f, delta));

            Debug.Log(
                $"[V1.8.7 PLANAGEM] {name}: qualidade={(glideEligibleThisHold ? "Effective" : "Fail")}; " +
                $"arrastoBracosAntes={currentDrag:0.###}; minimo={glideArmDragThreshold:0.###}; " +
                $"reforco={growth:0.###}; efeitoDescida={(glideEligibleThisHold ? "HABILITADO" : "NENHUM")}.", this);
        }

        // Gliding so atua na DESCIDA. A subida do pulo original nao muda.
        if (!glideEligibleThisHold || body.linearVelocity.y >= 0f) return;

        float drag = Mathf.Max(0f, anatomy?.bracos?.arrasto ?? 0f);
        float baseSpeed = Mathf.Max(0.1f, glideBaseDescentSpeed);
        float minimumSpeed = Mathf.Clamp(glideMinDescentSpeed, 0.1f, baseSpeed);
        float maxDownSpeed = Mathf.Clamp(
            baseSpeed / (1f + drag * Mathf.Max(0f, glideDragStrength)),
            minimumSpeed, baseSpeed);
        float oldY = body.linearVelocity.y;
        float newY = Mathf.Max(oldY, -maxDownSpeed);

        // Preserva a gravidade global do Rigidbody2D. Apenas aplica um
        // limite de descida na planagem ativa, nunca cria sustentacao positiva.
        if (newY > oldY)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, newY);
            if (!glideLoggedFirstDescent)
            {
                Debug.Log(
                    $"[V1.8.7 DESCIDA] {name}: arrastoBracos={drag:0.###}; " +
                    $"velYAntes={oldY:0.###}; velYDepois={newY:0.###}; " +
                    $"limiteDescida={maxDownSpeed:0.###} u/s.", this);
                glideLoggedFirstDescent = true;
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

    // V1.8.8 / PHY-093: em um mesmo golpe, Mon mais pesado absorve
    // maior parte da velocidade de reacao. F/V SUBSTITUEM o movimento X
    // anterior, evitando amplificar o empurrao por velocidade residual.
    // V1.8.9: quantidade de pernas contribui para a aderencia SOMENTE no solo.
    // Revestimento e tipo de terreno permanecem fora desta fatia.
    // Este metodo recebe um vetor 2D para ser reutilizavel em outros impactos.
    public void ApplyAnatomicalHitKnockback(Vector2 requestedVelocityDelta, float controlLockSeconds)
    {
        if (health != null && health.IsDead) return;
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();

        float totalMass = Mathf.Max(0f, anatomy?.MassaGeral ?? 0f);
        float factor = 1f / Mathf.Sqrt(1f + totalMass * Mathf.Max(0f, massImpactResistance));
        factor = Mathf.Clamp(factor, Mathf.Clamp01(minimumImpactMultiplier), 1f);

        int legs = Mathf.Max(0, anatomy?.pernas?.quantidade ?? 2);
        float tractionFactor = 1f;
        if (isGrounded)
        {
            // Dois membros preservam exatamente a reacao aprovada na V1.8.8.1.
            // Mais pernas => mais aderencia e menor velocidade horizontal.
            // Menos pernas => menos aderencia e maior reacao horizontal.
            float denominator = 1f + (legs - 2) * Mathf.Max(0f, legCountGroundTractionGain);
            float minimum = Mathf.Clamp(minimumLegCountTractionMultiplier, 0.01f, 1f);
            float maximum = Mathf.Max(1f, maximumLegCountTractionMultiplier);
            tractionFactor = Mathf.Clamp(1f / Mathf.Max(0.01f, denominator), minimum, maximum);
        }

        string legCovering = anatomy?.pernas?.revestimento ?? string.Empty;
        string surfaceLabel = "SEM_COMPONENTE";
        string interaction = "NEUTRA";
        float coveringSurfaceFactor = 1f;
        if (isGrounded)
        {
            MonGroundSurface2D surface = supportingGroundCollider != null
                ? supportingGroundCollider.GetComponentInParent<MonGroundSurface2D>()
                : null;
            if (surface != null)
            {
                surfaceLabel = surface.surfaceKind.ToString();
                if (legs > 0)
                    coveringSurfaceFactor = surface.GetImpactResponseMultiplier(
                        legCovering, out interaction);
            }
        }
        else
        {
            surfaceLabel = "NO_AR";
            interaction = "INATIVA";
        }

        Vector2 reactionVelocity = new Vector2(
            requestedVelocityDelta.x * factor * tractionFactor * coveringSurfaceFactor,
            requestedVelocityDelta.y * factor); // aderencia nao cancela lancamento vertical
        float velocityBeforeX = body.linearVelocity.x;
        float positionBeforeX = body.position.x;

        // F/V: velocidade de REACAO definida pelo golpe, nao velocidade acumulada.
        // Uma velocidade anterior causada por colisao/movimento nao amplifica
        // o empurrao. Preservamos o Y existente para nao cancelar saltos.
        float responseX = Mathf.Abs(requestedVelocityDelta.x) > 0.0001f
            ? reactionVelocity.x
            : velocityBeforeX;
        body.linearVelocity = new Vector2(
            responseX, body.linearVelocity.y + reactionVelocity.y);
        knockbackControlUntil = Mathf.Max(
            knockbackControlUntil, Time.time + Mathf.Max(0f, controlLockSeconds));

        Debug.Log(
            $"[V1.8.8 IMPACTO] {name}: massaGeral={totalMass:0.###}; " +
            $"emSolo={(isGrounded ? "SIM" : "NAO")}; fator=x{factor:0.###}; " +
            $"pernasQtd={legs}; aderenciaPernas=x{tractionFactor:0.###}; " +
            $"aderenciaRevestimento=x{coveringSurfaceFactor:0.###}; " +
            $"impulsoPedidoX={requestedVelocityDelta.x:0.###}; " +
            $"impulsoAplicadoX={reactionVelocity.x:0.###}; modo=SUBSTITUICAO; " +
            $"velXAntes={velocityBeforeX:0.###}; velXDepois={body.linearVelocity.x:0.###}; " +
            $"posX={positionBeforeX:0.###}; limiteX=[{minX:0.###},{maxX:0.###}]; " +
            $"tempoControle={Mathf.Max(0f, controlLockSeconds):0.###} s.", this);

        if (isGrounded)
        {
            Debug.Log(
                $"[V1.8.9 ADERENCIA] {name}: emSolo=SIM; pernasQtd={legs}; " +
                $"massaGeral={totalMass:0.###}; fatorMassa=x{factor:0.###}; " +
                $"fatorPernas=x{tractionFactor:0.###}; " +
                $"velReacaoX={reactionVelocity.x:0.###} u/s.", this);
        }
        else
        {
            Debug.Log(
                $"[V1.8.9 ADERENCIA] {name}: emSolo=NAO; pernasQtd={legs}; " +
                $"fatorPernas=x1; velReacaoX={reactionVelocity.x:0.###} u/s.", this);
        }

        Debug.Log(
            $"[V1.8.10 TERRENO] {name}: emSolo={(isGrounded ? "SIM" : "NAO")}; " +
            $"superficie={surfaceLabel}; revestimento={(string.IsNullOrWhiteSpace(legCovering) ? "Nenhum" : legCovering)}; " +
            $"interacao={interaction}; fatorRevestimento=x{coveringSurfaceFactor:0.###}; " +
            $"fatorPernas=x{tractionFactor:0.###}; fatorMassa=x{factor:0.###}; " +
            $"velReacaoX={reactionVelocity.x:0.###} u/s.", this);

        StartCoroutine(LogAnatomicalHitDisplacement(
            positionBeforeX, reactionVelocity.x,
            Mathf.Max(0.06f, controlLockSeconds)));
    }

    // Diagnostico temporario: velocidade pode ser bloqueada por borda/colisao.
    // Compara coordenadas reais do Rigidbody2D apos o tempo de reacao.
    private IEnumerator LogAnatomicalHitDisplacement(
        float startX, float appliedImpulseX, float sampleDuration)
    {
        int physicsSteps = Mathf.Max(1,
            Mathf.CeilToInt(sampleDuration / Mathf.Max(0.001f, Time.fixedDeltaTime)));
        for (int step = 0; step < physicsSteps; step++)
            yield return new WaitForFixedUpdate();

        float actualDeltaX = body.position.x - startX;
        bool nearLimit = clampX &&
            (body.position.x <= minX + 0.05f || body.position.x >= maxX - 0.05f);

        Debug.Log(
            $"[V1.8.8 DESLOCAMENTO] {name}: posXAntes={startX:0.###}; " +
            $"posXDepois={body.position.x:0.###}; deltaX={actualDeltaX:0.###}; " +
            $"impulsoX={appliedImpulseX:0.###}; encostouLimite={(nearLimit ? "SIM" : "NAO")}; " +
            $"limiteX=[{minX:0.###},{maxX:0.###}].", this);
    }

    /// <summary>
    /// Acrescenta uma variacao de velocidade de impacto ao corpo.
    /// V1.7.1: valor de teste, sem calculo anatomico de impulso.
    /// Este metodo continua sem escala de massa para preservar efeitos legados.
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
            supportingGroundCollider = null;
            isGrounded = false;
            return;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 probeCenter = new Vector2(bounds.center.x, bounds.min.y - groundProbeDepth * 0.5f);
        Vector2 probeSize = new Vector2(bounds.size.x * 0.75f, groundProbeDepth);
        supportingGroundCollider = Physics2D.OverlapBox(
            probeCenter, probeSize, 0f, groundLayers);
        isGrounded = supportingGroundCollider != null && body.linearVelocity.y <= 0.01f;
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
