using UnityEngine;

// V1.7.5: golpes basicos com contato anatomico e dano.
// V1.8.1: primeiro reforco anatomico DIRETO de teste por acerto real.
// V1.8.2: Fail sem dano; Effective por capacidade; Critical por contra-ataque real.
// V1.8.3: ate tres influencias anatomicas ponderadas por golpe (valores de teste).
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

    [Header("V1.8.2 - Capacidade anatomica (limiar de teste)")]
    [Tooltip("Extensao minima do membro para executar o golpe. Abaixo dela: Fail, zero dano e pouco reforco mesmo sem contato.")]
    [Min(0f)] public float effectiveExtensionThreshold = 0.25f;

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

        MoveExecutionQuality quality = EvaluateStrikeQuality(
            punch, counterWindow, out float extensionBefore);

        int damageApplied = 0;
        if (quality != MoveExecutionQuality.Fail && geometricalContact)
        {
            int baseDamage = Mathf.Max(1, punch ? weakPunchDamage : weakKickDamage);
            damageApplied = Mathf.Max(1,
                Mathf.RoundToInt(baseDamage * DamageMultiplier(quality)));
            opponent.TakeDamage(damageApplied, transform);
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

    // Qualidade depende da capacidade ANTES do golpe + oportunidade de
    // contra-ataque. Bracos/Pernas so definem Fail vs golpe executavel.
    // Critical requer contato real durante o STARTUP do adversario.
    // Tronco so altera alcance neste prototipo.
    private MoveExecutionQuality EvaluateStrikeQuality(
        bool punch, bool counterWindow, out float extension)
    {
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();

        extension = anatomy == null ? 0f : Mathf.Max(0f,
            punch ? (anatomy.bracos?.extensao ?? 0f)
                  : (anatomy.pernas?.extensao ?? 0f));

        float effective = Mathf.Max(0f, effectiveExtensionThreshold);
        if (extension < effective) return MoveExecutionQuality.Fail;
        return counterWindow ? MoveExecutionQuality.Critical : MoveExecutionQuality.Effective;
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
