using UnityEngine;

// V1.7.5: golpes basicos com contato anatomico e dano.
// V1.8.1: primeiro reforco anatomico DIRETO de teste por acerto real.
// V1.8.2: Fail sem dano; Effective por capacidade; Critical por contra-ataque real.
// V1.8.3: ate tres influencias anatomicas ponderadas por golpe (valores de teste).
// V1.8.4: execucao exige TODOS os requisitos anatomicos do golpe (AND).
// V1.8.8: golpes Effective/Critical aplicam impulso fisico de teste no alvo.
// V1.8.13: componentes horizontal e vertical independentes de impacto.
// V1.8.14: potencia de impulso influenciada pela massa do membro atacante.
// Nao converte os pontos Attack/Defense/Air/Energy da V1.6.
public enum MonBasicStrikeId
{
    WeakPunch,
    WeakKick
}

[DisallowMultipleComponent]
[RequireComponent(typeof(MonAnatomicalStrikeHitboxes))]
[RequireComponent(typeof(MonAnatomy))]
public class MonBasicStrikeSet : MonoBehaviour
{
    [Header("Dano de teste - balanceamento definitivo pendente")]
    [Min(1)] public int weakPunchDamage = 10;
    [Min(1)] public int weakKickDamage = 10;

    [Header("V1.8.1 - Crescimento anatomico por acerto (valores de teste)")]
    public bool enableAnatomicalGrowthOnHit = true;
    [Min(0f)] public float armExtensionGainPerPunchHit = 0.1f;
    [Min(0f)] public float legExtensionGainPerKickHit = 0.1f;

    [Header("V1.8.3 - Influencias secundarias e terciarias (teste)")]
    [Tooltip("Desative apenas para comparar com V1.8.2. A influencia primaria de extensao continua.")]
    public bool enableAdditionalAnatomicalInfluences = true;

    [Tooltip("Soco: influencia secundaria. Numeros por braco quando aplicavel.")]
    public MonAnatomicalInfluence punchSecondary =
        new MonAnatomicalInfluence(MonAnatomyParameter.ArmsTenacity, 0.04f);

    [Tooltip("Soco: influencia terciaria. Massa e armazenada por braco.")]
    public MonAnatomicalInfluence punchTertiary =
        new MonAnatomicalInfluence(MonAnatomyParameter.ArmsMass, 0.02f);

    [Tooltip("Chute: influencia secundaria. Numeros por perna quando aplicavel.")]
    public MonAnatomicalInfluence kickSecondary =
        new MonAnatomicalInfluence(MonAnatomyParameter.LegsTenacity, 0.04f);

    [Tooltip("Chute: influencia terciaria. Massa e armazenada por perna.")]
    public MonAnatomicalInfluence kickTertiary =
        new MonAnatomicalInfluence(MonAnatomyParameter.LegsMass, 0.02f);

    [Header("V1.8.4 - Requisitos anatomicos obrigatorios (teste)")]
    [Tooltip("Minimo de extensao de bracos/pernas para executar F/V.")]
    [Min(0f)] public float effectiveExtensionThreshold = 0.25f;
    [Tooltip("Minimo de tenacidade POR membro de bracos/pernas para executar F/V.")]
    [Min(0f)] public float effectiveTenacityThreshold = 0.10f;
    [Tooltip("Minimo de massa POR membro de bracos/pernas para executar F/V.")]
    [Min(0f)] public float effectiveMassThreshold = 0.05f;

    [Header("V1.8.8 - Empurrao fisico F/V (teste)")]
    [Tooltip("Acertos Effective/Critical F/V aplicam impulso; Fail/whiff nao.")]
    public bool enablePhysicalHitKnockback = true;
    [Tooltip("Velocidade horizontal de reacao do golpe fraco, antes da resistencia pela massa (u/s).")]
    [Min(0f)] public float baseHitKnockbackSpeed = 1f;
    [Tooltip("Duracao curta da reacao sem controle normal, para manter os Mons proximos (s).")]
    [Min(0f)] public float hitKnockbackControlSeconds = 0.10f;

    [Header("V1.8.13 - Impacto vetorial do chute (teste)")]
    [Tooltip("Velocidade vertical (Y) solicitada no chute V Effective/Critical. 0 preserva o chute horizontal validado anteriormente.")]
    [Min(0f)] public float weakKickVerticalLaunchSpeed = 0f;

    [Header("V1.8.14 - Potencia fisica pela massa do membro atacante (teste)")]
    [Tooltip("Aplica o mesmo fator de massa do braco/perna a X e Y do impacto; dano permanece independente.")]
    public bool enableStrikingLimbMassImpact = true;
    [Tooltip("Massa POR MEMBRO abaixo ou igual a este valor preserva o impulso base (sem bonus).")]
    [Min(0f)] public float referenceStrikingLimbMass = 0.10f;
    [Tooltip("Bonus de velocidade de impacto por unidade de massa do membro acima da referencia.")]
    [Min(0f)] public float strikingLimbMassImpactGain = 0.25f;
    [Tooltip("Teto experimental do multiplicador de impacto por massa do membro.")]
    [Min(1f)] public float maximumStrikingLimbMassImpactMultiplier = 2f;

    [Header("V1.8.2 - Multiplicadores de teste por qualidade")]
    [Min(0f)] public float effectiveDamageMultiplier = 1f;
    [Min(0f)] public float criticalDamageMultiplier = 1.5f;
    [Min(0f)] public float failGrowthMultiplier = 0.5f;
    [Min(0f)] public float effectiveGrowthMultiplier = 1f;
    [Min(0f)] public float criticalGrowthMultiplier = 2f;

    private MonAnatomicalStrikeHitboxes hitboxes;
    private MonAnatomy anatomy;

    private void Awake()
    {
        hitboxes = GetComponent<MonAnatomicalStrikeHitboxes>();
        anatomy = GetComponent<MonAnatomy>();
    }

    // Uma tentativa valida sempre consome cooldown no MonPlayerController.
    // Fail anatomico: zero dano, pequeno reforco por TENTATIVA (mesmo whiff).
    // Effective/Critical: dano e reforco somente quando a hitbox atinge alvo.
    public bool TryExecute(MonBasicStrikeId strike, Health opponent)
    {
        if (opponent == null || opponent.IsDead) return false;

        if (hitboxes == null)
            hitboxes = GetComponent<MonAnatomicalStrikeHitboxes>();

        if (hitboxes == null || !hitboxes.enabled)
        {
            Debug.LogWarning("[V1.8.2] Hitboxes anatomicas ausentes ou desativadas.", this);
            return false;
        }

        bool punch = strike == MonBasicStrikeId.WeakPunch;
        float reach = punch ? hitboxes.PunchReach : hitboxes.KickReach;

        Physics2D.SyncTransforms();
        bool geometricalContact = hitboxes.OverlapsTarget(punch, opponent);

        // Captura a oportunidade antes de qualquer dano, que poderia
        // interromper o startup e mudar o estado do adversario.
        CombatantStateMachine opponentState = opponent.GetComponent<CombatantStateMachine>();
        bool counterWindow = geometricalContact && opponentState != null &&
            opponentState.IsInAttackStartup;

        // A capacidade anatomica e medida antes de qualquer reforco no golpe.
        MoveExecutionQuality quality = EvaluateStrikeQuality(
            punch, counterWindow,
            out float extensionBefore, out float tenacityBefore, out float massBefore);

        bool hasExtension = MeetsMinimum(extensionBefore, effectiveExtensionThreshold);
        bool hasTenacity = MeetsMinimum(tenacityBefore, effectiveTenacityThreshold);
        bool hasMass = MeetsMinimum(massBefore, effectiveMassThreshold);
        Debug.Log(
            $"[V1.8.4 REQUISITOS] {name}: golpe={(punch ? "SOCO" : "CHUTE")}; " +
            $"extensao={extensionBefore:0.###}/{effectiveExtensionThreshold:0.###} {(hasExtension ? "OK" : "FALTA")}; " +
            $"tenacidade={tenacityBefore:0.###}/{effectiveTenacityThreshold:0.###} {(hasTenacity ? "OK" : "FALTA")}; " +
            $"massa={massBefore:0.###}/{effectiveMassThreshold:0.###} {(hasMass ? "OK" : "FALTA")}; " +
            $"elegivel={(quality == MoveExecutionQuality.Fail ? "NAO" : "SIM")}.", this
        );

        int damageApplied = 0;
        if (quality != MoveExecutionQuality.Fail && geometricalContact)
        {
            int baseDamage = Mathf.Max(1, punch ? weakPunchDamage : weakKickDamage);
            damageApplied = Mathf.Max(1,
                Mathf.RoundToInt(baseDamage * DamageMultiplier(quality)));
            opponent.TakeDamage(damageApplied, transform);

            // Aplica movimento real somente ao conectar um golpe valido.
            // Dano, requisitos e calculo de Critical continuam inalterados.
            if (enablePhysicalHitKnockback)
            {
                MonPhysicsController2D opponentMotor =
                    opponent.GetComponent<MonPhysicsController2D>();
                if (opponentMotor != null)
                {
                    float directionX = opponent.transform.position.x < transform.position.x ? -1f : 1f;
                    // V1.8.14: massBefore e a massa POR MEMBRO ANTES do reforco
                    // deste proprio golpe. Braco para F, perna para V.
                    // Fator inicial x1 em 0.10 para preservar V1.8.13;
                    // bonus acima da referencia, limitado para este prototipo.
                    float strikingLimbMass = Mathf.Max(0f, massBefore);
                    float referenceMass = Mathf.Max(0f, referenceStrikingLimbMass);
                    float massImpactMultiplier = enableStrikingLimbMassImpact
                        ? Mathf.Clamp(
                            1f + Mathf.Max(0f, strikingLimbMass - referenceMass) *
                                Mathf.Max(0f, strikingLimbMassImpactGain),
                            1f, Mathf.Max(1f, maximumStrikingLimbMassImpactMultiplier))
                        : 1f;

                    // O mesmo fator atua em X/Y do vetor do movimento,
                    // sem substituir a resistencia do Mon atingido.
                    // Dano, qualidade e crescimento permanecem inalterados.
                    Vector2 requestedHitVelocity = new Vector2(
                        directionX * Mathf.Max(0f, baseHitKnockbackSpeed),
                        punch ? 0f : Mathf.Max(0f, weakKickVerticalLaunchSpeed)) *
                        massImpactMultiplier;
                    Debug.Log(
                        $"[V1.8.14 POTENCIA] {name}: golpe={(punch ? "SOCO" : "CHUTE")}; " +
                        $"massaMembro={strikingLimbMass:0.###}; referencia={referenceMass:0.###}; " +
                        $"ganho={Mathf.Max(0f, strikingLimbMassImpactGain):0.###}; " +
                        $"fator=x{massImpactMultiplier:0.###}; " +
                        $"pedidoX={requestedHitVelocity.x:0.###}; " +
                        $"pedidoY={requestedHitVelocity.y:0.###}; alvo={opponent.name}.", this);
                    Debug.Log(
                        $"[V1.8.13 VETOR] {name}: golpe={(punch ? "SOCO" : "CHUTE")}; " +
                        $"qualidade={quality}; contato=SIM; " +
                        $"pedidoX={requestedHitVelocity.x:0.###} u/s; " +
                        $"pedidoY={requestedHitVelocity.y:0.###} u/s; " +
                        $"alvo={opponent.name}.", this);
                    opponentMotor.ApplyAnatomicalHitKnockback(
                        requestedHitVelocity,
                        Mathf.Max(0f, hitKnockbackControlSeconds));
                }
            }
        }

        float growthApplied = 0f;
        if (quality == MoveExecutionQuality.Fail || geometricalContact)
            growthApplied = ReinforceAnatomyAfterAttempt(punch, quality);

        float extensionAfter = anatomy == null ? extensionBefore :
            Mathf.Max(0f, punch ? (anatomy.bracos?.extensao ?? 0f)
                                : (anatomy.pernas?.extensao ?? 0f));

        // Uma unica linha filtravel para acompanhar a fatia ativa.
        // 'reforco' ainda informa apenas o parametro principal (extensao).
        Debug.Log(
            $"[V1.8.2 GOLPE] {name}: tipo={(punch ? "SOCO" : "CHUTE")}; " +
            $"qualidade={quality}; contatoGeometrico={(geometricalContact ? "SIM" : "NAO")}; " +
            $"contraJanela={(counterWindow ? "SIM" : "NAO")}; " +
            $"dano={damageApplied}; reforco={growthApplied:0.###}; " +
            $"extensao={extensionBefore:0.###}->{extensionAfter:0.###}; " +
            $"alcance={reach:0.###} u; HPAlvo={opponent.currentHP}/{opponent.maxHP}.", this
        );

        // Log legado da V1.7.5 preservado, agora sem confundir geometria com
        // acerto efetivo quando o corpo nao sustenta o movimento.
        string result = quality == MoveExecutionQuality.Fail
            ? "FALHA_ANATOMICA"
            : (geometricalContact ? "CONTATO" : "FORA");
        Debug.Log(
            $"[V1.7.5 {(punch ? "SOCO FRACO" : "CHUTE FRACO")}] " +
            $"{name} -> {opponent.name}: resultado={result}; " +
            $"alcance={reach:0.###} u; danoAplicado={damageApplied}; " +
            $"HPAlvo={opponent.currentHP}/{opponent.maxHP}.", this
        );

        return true;
    }

    // V1.8.3 - Um golpe pode reforcar ate tres parametros: primario,
    // secundario e terciario. Os valores sao base POR MOVIMENTO e a qualidade
    // os multiplica de forma uniforme (Fail .5, Effective 1, Critical 2).
    // Fail reforca por tentativa; Effective/Critical so por contato.
    // A influencia primaria permanece extensao para preservar V1.8.1/2.
    private float ReinforceAnatomyAfterAttempt(bool punch, MoveExecutionQuality quality)
    {
        if (!enableAnatomicalGrowthOnHit) return 0f;
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();
        if (anatomy == null) return 0f;

        float multiplier = GrowthMultiplier(quality);
        float basePrimary = Mathf.Max(0f,
            punch ? armExtensionGainPerPunchHit : legExtensionGainPerKickHit);
        MonAnatomyParameter primary = punch
            ? MonAnatomyParameter.ArmsExtension
            : MonAnatomyParameter.LegsExtension;

        float primaryBefore = punch
            ? (anatomy.bracos?.extensao ?? 0f)
            : (anatomy.pernas?.extensao ?? 0f);

        float primaryApplied = anatomy.AddInfluence(primary, basePrimary * multiplier);

        // A distribuicao secundaria/terciaria e experimental e separada
        // dos quatro eixos legados Attack/Defense/Air/Energy.
        MonAnatomicalInfluence secondary = punch ? punchSecondary : kickSecondary;
        MonAnatomicalInfluence tertiary = punch ? punchTertiary : kickTertiary;
        float secondaryApplied = 0f;
        float tertiaryApplied = 0f;

        if (enableAdditionalAnatomicalInfluences)
        {
            if (secondary != null)
                secondaryApplied = anatomy.AddInfluence(
                    secondary.parameter, Mathf.Max(0f, secondary.amount) * multiplier);
            if (tertiary != null)
                tertiaryApplied = anatomy.AddInfluence(
                    tertiary.parameter, Mathf.Max(0f, tertiary.amount) * multiplier);
        }

        float primaryAfter = punch
            ? (anatomy.bracos?.extensao ?? 0f)
            : (anatomy.pernas?.extensao ?? 0f);

        // Mantem diagnosticos de V1.8.1 para comparacao, sem novas chamadas
        // de crescimento das extensoes.
        if (primaryApplied > 0f)
        {
            string limb = punch ? "BRACOS" : "PERNAS";
            float reach = punch ? hitboxes.PunchReach : hitboxes.KickReach;
            Debug.Log(
                $"[V1.8.1 REFORCO {limb}] {name}: " +
                $"extensao={primaryBefore:0.###} -> {primaryAfter:0.###}; " +
                $"alcance={(reach):0.###} u.", this
            );
        }

        string secondaryName = secondary != null ? secondary.parameter.ToString() : "Nenhum";
        string tertiaryName = tertiary != null ? tertiary.parameter.ToString() : "Nenhum";
        Debug.Log(
            $"[V1.8.3 MORFOLOGIA] {name}: golpe={(punch ? "SOCO" : "CHUTE")}; " +
            $"qualidade={quality}; primaria={primary} +{primaryApplied:0.###}; " +
            $"secundaria={secondaryName} +{secondaryApplied:0.###}; " +
            $"terciaria={tertiaryName} +{tertiaryApplied:0.###}; " +
            $"massaGeral={anatomy.MassaGeral:0.###}; " +
            $"tenacidadeGeral={anatomy.TenacidadeGeral:0.###}.", this
        );

        return primaryApplied;
    }

    // Regra confirmada V1.8.4: TODOS os atributos exigidos pelo golpe
    // precisam satisfazer seus proprios minimos, sem compensacao.
    // Usa somente os 3 parametros envolvidos em F/V nesta fatia.
    // Critical continua exigindo contato real no startup do adversario.
    private MoveExecutionQuality EvaluateStrikeQuality(
        bool punch, bool counterWindow,
        out float extension, out float tenacity, out float mass)
    {
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();

        MonRepeatedLimbAnatomy limb = anatomy == null ? null
            : punch ? anatomy.bracos : anatomy.pernas;
        extension = limb == null ? 0f : Mathf.Max(0f, limb.extensao);
        tenacity = limb == null ? 0f : Mathf.Max(0f, limb.tenacidade);
        mass = limb == null ? 0f : Mathf.Max(0f, limb.massa);

        // Tolerancia pequena contra erros de arredondamento perto do limiar.
        if (limb == null ||
            !MeetsMinimum(extension, effectiveExtensionThreshold) ||
            !MeetsMinimum(tenacity, effectiveTenacityThreshold) ||
            !MeetsMinimum(mass, effectiveMassThreshold))
            return MoveExecutionQuality.Fail;

        return counterWindow ? MoveExecutionQuality.Critical : MoveExecutionQuality.Effective;
    }

    private static bool MeetsMinimum(float current, float minimum)
    {
        const float epsilon = 0.00001f;
        return current + epsilon >= Mathf.Max(0f, minimum);
    }

    private float DamageMultiplier(MoveExecutionQuality quality)
    {
        switch (quality)
        {
            case MoveExecutionQuality.Critical: return Mathf.Max(0f, criticalDamageMultiplier);
            case MoveExecutionQuality.Effective: return Mathf.Max(0f, effectiveDamageMultiplier);
            default: return 0f; // Fail nunca causa dano.
        }
    }

    private float GrowthMultiplier(MoveExecutionQuality quality)
    {
        switch (quality)
        {
            case MoveExecutionQuality.Critical: return Mathf.Max(0f, criticalGrowthMultiplier);
            case MoveExecutionQuality.Effective: return Mathf.Max(0f, effectiveGrowthMultiplier);
            default: return Mathf.Max(0f, failGrowthMultiplier);
        }
    }
}
