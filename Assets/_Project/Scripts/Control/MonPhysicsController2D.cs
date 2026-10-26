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
/// V1.8.11: soltar direcao em solo desfavoravel permite breve deslize.
/// V1.8.12: movimento voluntario evita empurrao corporal passivo.
/// V1.8.12.1: a cabeca do oponente nao funciona como plataforma.
/// V1.8.13: impacto anatomico com vetor horizontal/vertical observavel.
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

    [Header("V1.8.11 - Deslize contextual da locomocao (teste)")]
    [Tooltip("Frenagem horizontal ao soltar a direcao enquanto caminha em terreno desfavoravel (u/s²).")]
    [Min(0.1f)] public float unfavorableGroundBrakeDeceleration = 12f;

    [Header("V1.8.12 - Colisao corporal sem empurrao passivo (teste)")]
    [Tooltip("Margem antes de tocar o collider de outro Mon (unidades).")]
    [Min(0f)] public float monBodyContactSkin = 0.025f;
    [Tooltip("V1.8.12.1 - velocidade lateral do escape ao pousar sobre outro Mon (u/s).")]
    [Min(0.1f)] public float overheadSlipSpeed = 2.5f;
    [Tooltip("V1.8.12.1 - janela acima da cabeca para identificar contato vertical (u).")]
    [Min(0.01f)] public float overheadContactWindow = 0.20f;

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
    private bool walkedOnGroundLastPhysicsStep;
    private bool locomotionSlipActive;
    private MonPhysicsController2D[] otherMonMotors;
    private bool monBodyBlockReported;
    private bool monBodyFullStopReported;
    private bool monBodyTargetSamplePending;
    private MonPhysicsController2D overheadSlipTarget;
    private float overheadSlipDirection;
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
        // Ambos os Mons ja existem na cena quando Awake executa.
        // Cache evita procurar objetos a cada passo de fisica.
        otherMonMotors = FindObjectsByType<MonPhysicsController2D>(FindObjectsSortMode.None);
        Debug.Log($"[V1.8.12 ATIVO] {name}: motor carregado; " +
                  $"MonsEncontrados={otherMonMotors.Length}; margem={monBodyContactSkin:0.###} u.", this);

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
            EndOverheadSlip("MORTE");
            walkedOnGroundLastPhysicsStep = false;
            locomotionSlipActive = false;
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
                // Estados de combate sempre tem prioridade; nao iniciam deslize.
                nextX = 0f;
                locomotionSlipActive = false;
            }
            else if (isGrounded)
            {
                if (justLanded || Mathf.Abs(direction) > 0.01f && requestedSpeed > 0f)
                {
                    // PHY-083/090: caminhada normal responsiva e pouso sem
                    // comando com parada imediata, mesmo sobre piso liso.
                    nextX = desiredX;
                    locomotionSlipActive = false;
                }
                else
                {
                    // V1.8.11: so um Mon que CAMINHAVA no chao pode comecar
                    // a deslizar ao soltar o comando em uma interacao ruim.
                    // Impulso de golpe e aterrissagem nunca iniciam este deslize.
                    MonGroundSurface2D floor = supportingGroundCollider != null
                        ? supportingGroundCollider.GetComponentInParent<MonGroundSurface2D>()
                        : null;
                    string interaction = "NEUTRA";
                    bool hasLegContact = anatomy != null && anatomy.pernas != null &&
                        anatomy.pernas.quantidade > 0;
                    if (floor != null && hasLegContact)
                        floor.GetImpactResponseMultiplier(
                            anatomy.pernas.revestimento, out interaction);
                    bool unfavorable = interaction == "DESFAVORAVEL";

                    bool slipStarted = !locomotionSlipActive &&
                        walkedOnGroundLastPhysicsStep && unfavorable;
                    if (slipStarted) locomotionSlipActive = true;

                    if (locomotionSlipActive && unfavorable)
                    {
                        float brake = Mathf.Max(0.1f, unfavorableGroundBrakeDeceleration);
                        nextX = Mathf.MoveTowards(
                            beforeX, 0f, brake * Time.fixedDeltaTime);

                        if (slipStarted)
                            Debug.Log(
                                $"[V1.8.11 DESLIZE] {name}: evento=INICIO; " +
                                $"superficie={floor.surfaceKind}; " +
                                $"revestimento={anatomy.pernas.revestimento}; " +
                                $"interacao={interaction}; velAntes={beforeX:0.###}; " +
                                $"velDepois={nextX:0.###}; frenagem={brake:0.###} u/s².", this);

                        if (Mathf.Abs(nextX) <= 0.001f)
                        {
                            nextX = 0f;
                            locomotionSlipActive = false;
                            Debug.Log(
                                $"[V1.8.11 DESLIZE] {name}: evento=FIM; velX=0.", this);
                        }
                    }
                    else
                    {
                        // Em solo neutro ou compativel o comando continua
                        // parando imediatamente, sem inercia acumulada.
                        nextX = 0f;
                        locomotionSlipActive = false;
                    }
                }
            }
            else
            {
                locomotionSlipActive = false;
                // PHY-089: correcao parcial no ar, nao troca instantanea.
                // Sem comando, o impulso horizontal se dissipa gradualmente.
                float acceleration = Mathf.Abs(direction) > 0.01f
                    ? airSteeringAcceleration : airReleaseDeceleration;
                nextX = Mathf.MoveTowards(
                    beforeX, desiredX, Mathf.Max(0f, acceleration) * Time.fixedDeltaTime);
            }

            // Nao deixa a velocidade VOLUNTARIA de caminhada transferir
            // um empurrao ao corpo do outro Mon. Golpes usam rota propria.
            if (!locked && Mathf.Abs(nextX) > 0.0001f)
                nextX = LimitVoluntarySpeedAgainstOtherMon(nextX);
            else
            {
                monBodyBlockReported = false;
                monBodyFullStopReported = false;
            }

            walkedOnGroundLastPhysicsStep = isGrounded && !locked &&
                Mathf.Abs(direction) > 0.01f && requestedSpeed > 0f;

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
        else
        {
            // Reacoes forcadas de combate nao usam o limite de caminhada.
            walkedOnGroundLastPhysicsStep = false;
            locomotionSlipActive = false;
            monBodyBlockReported = false;
            monBodyFullStopReported = false;
        }

        UpdateAnatomicalGlide();
        UpdateOverheadSlip();

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

    // V1.8.12.1 / PHY-096: enquanto um Mon desce sobre o topo do outro,
    // a colisao fisica da DUPLA e suspensa so pelo tempo necessario para
    // o Mon de cima escorregar ate uma lateral livre. Depois restauramos
    // imediatamente a colisao habitual dos corpos. O piso real continua
    // colidindo e a gravidade nunca e anulada.
    private void UpdateOverheadSlip()
    {
        if (body == null || bodyCollider == null) return;

        if (overheadSlipTarget != null)
        {
            // Nunca restaurar os colisores ainda sobrepostos: o solver
            // poderia gerar um empurrao passivo ao separar os dois corpos.
            CapsuleCollider2D targetCollider = overheadSlipTarget.bodyCollider;
            if (targetCollider == null || !targetCollider.enabled ||
                !overheadSlipTarget.gameObject.activeInHierarchy)
            {
                EndOverheadSlip("ALVO_INATIVO");
                return;
            }

            Bounds upper = bodyCollider.bounds;
            Bounds lower = targetCollider.bounds;
            float clearance = Mathf.Max(0.015f, monBodyContactSkin);
            bool freeOnX = upper.min.x >= lower.max.x + clearance ||
                           upper.max.x <= lower.min.x - clearance;
            if (freeOnX)
            {
                EndOverheadSlip("LATERAL_LIVRE");
                return;
            }

            // Movimento temporario limitado a sair de cima da cabeca.
            // Permite a gravidade levar o Mon ao piso, sem transforma-lo
            // numa plataforma ou desabilitar colisoes com o terreno.
            body.linearVelocity = new Vector2(
                overheadSlipDirection * Mathf.Max(0.1f, overheadSlipSpeed),
                body.linearVelocity.y);
            return;
        }

        // Um golpe ou estado travado nao pode disparar esse escape.
        if (isGrounded || body.linearVelocity.y > 0.05f ||
            Time.time < knockbackControlUntil ||
            (stateMachine != null && stateMachine.IsLocked))
            return;

        if (otherMonMotors == null) return;
        foreach (MonPhysicsController2D other in otherMonMotors)
        {
            if (other == null || other == this ||
                other.body == null || other.bodyCollider == null ||
                !other.bodyCollider.enabled || other.bodyCollider.isTrigger ||
                !other.gameObject.activeInHierarchy || !other.isGrounded)
                continue;

            Bounds upper = bodyCollider.bounds;
            Bounds lower = other.bodyCollider.bounds;
            float gapY = upper.min.y - lower.max.y;
            float window = Mathf.Max(0.01f, overheadContactWindow);
            bool xOverlap = upper.min.x < lower.max.x &&
                            upper.max.x > lower.min.x;
            if (!xOverlap || gapY < -window * 0.5f || gapY > window)
                continue;

            float centerOffset = upper.center.x - lower.center.x;
            float escape = Mathf.Abs(centerOffset) > 0.025f
                ? Mathf.Sign(centerOffset)
                : Mathf.Abs(body.linearVelocity.x) > 0.05f
                    ? Mathf.Sign(body.linearVelocity.x)
                    : Mathf.Abs(inputX) > 0.05f
                        ? Mathf.Sign(inputX) : 1f;
            if (clampX)
            {
                float travel = upper.extents.x + lower.extents.x + 0.08f;
                if (body.position.x + escape * travel < minX ||
                    body.position.x + escape * travel > maxX)
                    escape = -escape;
            }

            overheadSlipTarget = other;
            overheadSlipDirection = escape;
            Physics2D.IgnoreCollision(bodyCollider, other.bodyCollider, true);
            walkedOnGroundLastPhysicsStep = false;
            locomotionSlipActive = false;
            Debug.Log(
                $"[V1.8.12 CABECA] {name}: evento=DESLIZE_INICIO; " +
                $"alvo={other.name}; distanciaVertical={gapY:0.###} u; " +
                $"direcao={(escape > 0f ? "DIREITA" : "ESQUERDA")}; " +
                $"velEscape={Mathf.Max(0.1f, overheadSlipSpeed):0.###} u/s; " +
                "colisaoDupla=TEMPORARIAMENTE_IGNORADA.", this);
            body.linearVelocity = new Vector2(
                escape * Mathf.Max(0.1f, overheadSlipSpeed),
                body.linearVelocity.y);
            break;
        }
    }

    private void EndOverheadSlip(string reason)
    {
        if (overheadSlipTarget == null) return;
        MonPhysicsController2D other = overheadSlipTarget;
        if (bodyCollider != null && bodyCollider.enabled &&
            other.bodyCollider != null && other.bodyCollider.enabled &&
            gameObject.activeInHierarchy && other.gameObject.activeInHierarchy)
        {
            Physics2D.IgnoreCollision(bodyCollider, other.bodyCollider, false);
        }

        Debug.Log(
            $"[V1.8.12 CABECA] {name}: evento=DESLIZE_FIM; " +
            $"alvo={other.name}; motivo={reason}; " +
            "colisaoDupla=RESTAURADA_QUANDO_POSSIVEL.", this);
        overheadSlipTarget = null;
        overheadSlipDirection = 0f;
    }

    private void OnDisable()
    {
        EndOverheadSlip("COMPONENTE_DESATIVADO");
    }

    // V1.8.12 / PHY-095/096: a caminhada nao deve empurrar passivamente
    // outro Mon. Collider2D.Distance mede a separacao REAL entre as duas
    // capsulas, inclusive quando o motor fisico ja estabeleceu contato.
    // Em alturas diferentes, o salto pode passar por cima normalmente.
    private float LimitVoluntarySpeedAgainstOtherMon(float voluntaryX)
    {
        if (bodyCollider == null || Mathf.Abs(voluntaryX) < 0.0001f)
        {
            monBodyBlockReported = false;
            monBodyFullStopReported = false;
            return voluntaryX;
        }

        float sign = Mathf.Sign(voluntaryX);
        float stepDistance = Mathf.Abs(voluntaryX) * Time.fixedDeltaTime;
        float skin = Mathf.Max(0f, monBodyContactSkin);
        float allowedX = voluntaryX;
        MonPhysicsController2D blockedBy = null;
        float closestDistance = float.PositiveInfinity;

        if (otherMonMotors != null)
        {
            foreach (MonPhysicsController2D other in otherMonMotors)
            {
                if (other == null || other == this ||
                    other.body == null || other.bodyCollider == null ||
                    !other.bodyCollider.enabled || other.bodyCollider.isTrigger ||
                    !other.gameObject.activeInHierarchy)
                    continue;

                // Se o outro esta atras, o movimento de afastamento e livre.
                if ((other.body.position.x - body.position.x) * sign <= 0f)
                    continue;

                ColliderDistance2D separation = bodyCollider.Distance(other.bodyCollider);
                float gap = Mathf.Max(0f, separation.distance);
                if (gap > stepDistance + skin) continue;

                float possibleSpeed = Mathf.Max(0f, gap - skin) /
                    Mathf.Max(0.001f, Time.fixedDeltaTime);
                if (possibleSpeed + 0.0001f < Mathf.Abs(allowedX))
                {
                    allowedX = sign * possibleSpeed;
                    blockedBy = other;
                    closestDistance = separation.distance;
                }
            }
        }

        bool limited = blockedBy != null;
        bool fullyStopped = limited && Mathf.Abs(allowedX) < 0.001f;
        if (limited && !monBodyBlockReported)
        {
            Debug.Log(
                $"[V1.8.12 CONTATO] {name}: alvo={blockedBy.name}; " +
                $"emSolo={(isGrounded ? "SIM" : "NAO")}; " +
                $"velPedidoX={voluntaryX:0.###}; velPermitidoX={allowedX:0.###}; " +
                $"distanciaColisao={closestDistance:0.###} u; " +
                "origem=LOCOMOCAO_VOLUNTARIA.", this);
            // Mesmo sem velocidade EXATAMENTE zero, verifica se o motor
            // deslocou o outro Mon durante o contato fisico.
            if (!monBodyTargetSamplePending)
            {
                monBodyTargetSamplePending = true;
                StartCoroutine(LogBodyBlockTargetPosition(
                    blockedBy, blockedBy.body.position.x));
            }
        }
        if (fullyStopped && !monBodyFullStopReported)
        {
            Debug.Log(
                $"[V1.8.12 BLOQUEIO] {name}: alvo={blockedBy.name}; " +
                $"velPedidoX={voluntaryX:0.###}; velPermitidoX={allowedX:0.###}; " +
                $"posAlvoX={blockedBy.body.position.x:0.###}.", this);
        }
        monBodyBlockReported = limited;
        monBodyFullStopReported = fullyStopped;
        return allowedX;
    }

    private IEnumerator LogBodyBlockTargetPosition(
        MonPhysicsController2D target, float startX)
    {
        // 15 passos, cerca de 0.3s na configuracao fisica padrao.
        for (int i = 0; i < 15; i++)
            yield return new WaitForFixedUpdate();
        if (target == null || target.body == null)
        {
            monBodyTargetSamplePending = false;
            yield break;
        }

        float endX = target.body.position.x;
        Debug.Log(
            $"[V1.8.12 ALVO] {name} -> {target.name}: " +
            $"posAlvoAntes={startX:0.###}; posAlvoDepois={endX:0.###}; " +
            $"deltaX={endX - startX:0.###} u; janela=15 FixedUpdates.", this);
        monBodyTargetSamplePending = false;
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
        float velocityBeforeY = body.linearVelocity.y;
        float positionBeforeX = body.position.x;
        float positionBeforeY = body.position.y;

        // F/V: velocidade de REACAO definida pelo golpe, nao velocidade acumulada.
        // Uma velocidade anterior causada por colisao/movimento nao amplifica
        // o empurrao. Preservamos o Y existente para nao cancelar saltos.
        float responseX = Mathf.Abs(requestedVelocityDelta.x) > 0.0001f
            ? reactionVelocity.x
            : velocityBeforeX;
        body.linearVelocity = new Vector2(
            responseX, body.linearVelocity.y + reactionVelocity.y);
        walkedOnGroundLastPhysicsStep = false;
        locomotionSlipActive = false;
        monBodyBlockReported = false;
        monBodyFullStopReported = false;
        knockbackControlUntil = Mathf.Max(
            knockbackControlUntil, Time.time + Mathf.Max(0f, controlLockSeconds));

        // V1.8.13: diagnóstico separado do X existente, para que impulso
        // vertical e ganho REAL de altura possam ser medidos no Unity.
        Debug.Log(
            $"[V1.8.13 IMPACTO] {name}: emSolo={(isGrounded ? "SIM" : "NAO")}; " +
            $"pedidoX={requestedVelocityDelta.x:0.###}; pedidoY={requestedVelocityDelta.y:0.###}; " +
            $"aplicadoX={reactionVelocity.x:0.###}; aplicadoY={reactionVelocity.y:0.###}; " +
            $"fatorMassa=x{factor:0.###}; aderenciaSoloX=x{tractionFactor * coveringSurfaceFactor:0.###}; " +
            $"velYAntes={velocityBeforeY:0.###}; velYDepois={body.linearVelocity.y:0.###}; " +
            "modoY=ADICAO.", this);
        if (requestedVelocityDelta.y > 0.0001f)
            StartCoroutine(LogHitVerticalDisplacement(positionBeforeY));

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

    // V1.8.13: amostra o pico efetivo do lancamento do alvo, nao apenas
    // a velocidade atribuida. Intervalo cobre o arco esperado com gravidade 18.
    private IEnumerator LogHitVerticalDisplacement(float initialY)
    {
        float maximumY = body.position.y;
        int steps = Mathf.Max(1, Mathf.CeilToInt(
            0.65f / Mathf.Max(0.001f, Time.fixedDeltaTime)));
        for (int step = 0; step < steps; step++)
        {
            yield return new WaitForFixedUpdate();
            if (body == null) yield break;
            maximumY = Mathf.Max(maximumY, body.position.y);
        }
        Debug.Log(
            $"[V1.8.13 ALTURA] {name}: inicioY={initialY:0.###}; " +
            $"picoY={maximumY:0.###}; subidaReal={maximumY - initialY:0.###} u; " +
            $"finalY={body.position.y:0.###}; amostras={steps}.", this);
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
        // Reacoes de golpes legados nao contam como locomocao voluntaria.
        walkedOnGroundLastPhysicsStep = false;
        locomotionSlipActive = false;
        monBodyBlockReported = false;
        monBodyFullStopReported = false;
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
