using UnityEngine;

// V1.7.5: golpes basicos com contato anatomico e dano.
// V1.8.1: primeiro reforco anatomico DIRETO de teste por acerto real.
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

    private MonAnatomicalStrikeHitboxes hitboxes;
    private MonAnatomy anatomy;

    private void Awake()
    {
        hitboxes = GetComponent<MonAnatomicalStrikeHitboxes>();
        anatomy = GetComponent<MonAnatomy>();
    }

    // Retorna true quando a tentativa foi executada, mesmo que erre.
    // Cooldown, estado Attacking e leitura de input sao do controlador.
    // Sem pontos antigos, bonus elemental ou alteracao visual nesta etapa.
    public bool TryExecute(MonBasicStrikeId strike, Health opponent)
    {
        if (opponent == null || opponent.IsDead) return false;

        if (hitboxes == null)
            hitboxes = GetComponent<MonAnatomicalStrikeHitboxes>();

        if (hitboxes == null || !hitboxes.enabled)
        {
            Debug.LogWarning("[V1.7.5] Hitboxes anatomicas ausentes ou desativadas.", this);
            return false;
        }

        bool punch = strike == MonBasicStrikeId.WeakPunch;
        float reach = punch ? hitboxes.PunchReach : hitboxes.KickReach;
        int damage = Mathf.Max(1, punch ? weakPunchDamage : weakKickDamage);

        // Garante que as consultas Physics2D usem as posicoes atuais.
        Physics2D.SyncTransforms();
        bool hit = hitboxes.OverlapsTarget(punch, opponent);

        if (hit)
        {
            opponent.TakeDamage(damage, transform);
            ReinforceAnatomyAfterHit(punch);
        }

        Debug.Log(
            $"[V1.7.5 {(punch ? "SOCO FRACO" : "CHUTE FRACO")}] " +
            $"{name} -> {opponent.name}: " +
            $"resultado={(hit ? "CONTATO" : "FORA")}; " +
            $"alcance={reach:0.###} u; danoAplicado={(hit ? damage : 0)}; " +
            $"HPAlvo={opponent.currentHP}/{opponent.maxHP}.", this
        );

        return true;
    }

    // V1.8.1: reforco incremental isolado para validar o elo
    // golpe acertou -> anatomia desenvolveu -> alcance futuro mudou.
    // Os coeficientes NAO sao regras definitivas de progressao.
    private void ReinforceAnatomyAfterHit(bool punch)
    {
        if (!enableAnatomicalGrowthOnHit) return;
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();
        if (anatomy == null) return;

        float gain = Mathf.Max(0f,
            punch ? armExtensionGainPerPunchHit : legExtensionGainPerKickHit);
        if (gain <= 0f) return;

        if (punch && anatomy.bracos != null)
        {
            float before = anatomy.bracos.extensao;
            anatomy.SetArmExtension(before + gain);
            Debug.Log(
                $"[V1.8.1 REFORCO BRACOS] {name}: " +
                $"extensao={before:0.###} -> {anatomy.bracos.extensao:0.###}; " +
                $"alcanceSoco={hitboxes.PunchReach:0.###} u.", this
            );
        }
        else if (!punch && anatomy.pernas != null)
        {
            float before = anatomy.pernas.extensao;
            anatomy.SetLegExtension(before + gain);
            Debug.Log(
                $"[V1.8.1 REFORCO PERNAS] {name}: " +
                $"extensao={before:0.###} -> {anatomy.pernas.extensao:0.###}; " +
                $"alcanceChute={hitboxes.KickReach:0.###} u.", this
            );
        }
    }
}
