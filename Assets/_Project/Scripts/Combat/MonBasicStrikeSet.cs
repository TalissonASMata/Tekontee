using UnityEngine;

// V1.7.5 - Dois golpes fisicos basicos que utilizam exclusivamente as
// hitboxes anatomicas V1.7.4; sem alterar os movimentos de teste V1.6.
public enum MonBasicStrikeId
{
    WeakPunch,
    WeakKick
}

[DisallowMultipleComponent]
[RequireComponent(typeof(MonAnatomicalStrikeHitboxes))]
public class MonBasicStrikeSet : MonoBehaviour
{
    [Header("Dano de teste - balanceamento definitivo pendente")]
    [Min(1)] public int weakPunchDamage = 10;
    [Min(1)] public int weakKickDamage = 10;

    private MonAnatomicalStrikeHitboxes hitboxes;

    private void Awake()
    {
        hitboxes = GetComponent<MonAnatomicalStrikeHitboxes>();
    }

    // Retorna true quando a tentativa foi executada, mesmo que erre.
    // Cooldown, estado Attacking e leitura de input sao do controlador.
    // Nenhum ponto morfologico ou bonus elemental e gerado nesta etapa.
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
            opponent.TakeDamage(damage, transform);

        Debug.Log(
            $"[V1.7.5 {(punch ? "SOCO FRACO" : "CHUTE FRACO")}] " +
            $"{name} -> {opponent.name}: " +
            $"resultado={(hit ? "CONTATO" : "FORA")}; " +
            $"alcance={reach:0.###} u; danoAplicado={(hit ? damage : 0)}; " +
            $"HPAlvo={opponent.currentHP}/{opponent.maxHP}.", this
        );

        return true;
    }
}
