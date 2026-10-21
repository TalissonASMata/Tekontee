using UnityEngine;

// V1.7.4 - Prototipo de alcance fisico do soco/chute, sem dano nem novo input.
// A hitbox e uma consulta Physics2D, NAO um Collider2D adicional no corpo.
[DisallowMultipleComponent]
[RequireComponent(typeof(MonAnatomy))]
[RequireComponent(typeof(Collider2D))]
public class MonAnatomicalStrikeHitboxes : MonoBehaviour
{
    [Header("Alvo do teste (se vazio, usa o alvo do controlador)")]
    public Health target;

    [Header("Alcance base alem da borda do colisor corporal")]
    [Min(0.05f)] public float punchBaseReach = 1f;
    [Min(0.05f)] public float kickBaseReach = 1f;

    [Header("Ganho de alcance por extensao - valores de teste")]
    [Min(0f)] public float punchArmExtensionGain = 0.4f;
    [Min(0f)] public float kickLegExtensionGain = 0.4f;
    [Min(0f)] public float torsoExtensionGain = 0.25f;

    [Header("Volume da area de contato - valores de teste")]
    [Min(0.05f)] public float hitboxHeight = 0.65f;
    public float punchHeightOffset = 0.08f;
    public float kickHeightOffset = -0.15f;

    [Header("Debug (Scene View com Mon selecionado)")]
    public bool drawHitboxes = true;

    private MonAnatomy anatomy;
    private Collider2D bodyCollider;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        if (anatomy == null) anatomy = GetComponent<MonAnatomy>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
    }

    private Health ResolveTarget()
    {
        if (target != null) return target;

        MonPlayerController player = GetComponent<MonPlayerController>();
        if (player != null && player.target != null) return player.target;

        AutoFighter autoFighter = GetComponent<AutoFighter>();
        return autoFighter != null ? autoFighter.target : null;
    }

    // A anatomia e consultada sempre que o alcance e solicitado.
    // Nao ha cache que dependa de uma transformacao visual.
    public float PunchReach
    {
        get
        {
            EnsureComponents();
            if (anatomy == null) return Mathf.Max(0.05f, punchBaseReach);
            return Mathf.Max(0.05f, punchBaseReach)
                + Mathf.Max(0f, anatomy.bracos?.extensao ?? 0f) * Mathf.Max(0f, punchArmExtensionGain)
                + Mathf.Max(0f, anatomy.tronco?.extensao ?? 0f) * Mathf.Max(0f, torsoExtensionGain);
        }
    }

    public float KickReach
    {
        get
        {
            EnsureComponents();
            if (anatomy == null) return Mathf.Max(0.05f, kickBaseReach);
            return Mathf.Max(0.05f, kickBaseReach)
                + Mathf.Max(0f, anatomy.pernas?.extensao ?? 0f) * Mathf.Max(0f, kickLegExtensionGain)
                + Mathf.Max(0f, anatomy.tronco?.extensao ?? 0f) * Mathf.Max(0f, torsoExtensionGain);
        }
    }

    private float FacingDirection(Health opponent)
    {
        // PHY-097: a direcao depende do oponente, nao da direcao de caminhada.
        return opponent != null && opponent.transform.position.x < transform.position.x ? -1f : 1f;
    }

    // Uma area retangular diante do corpo, partindo de sua borda fisica.
    // O tamanho do CapsuleCollider2D NAO e alterado.
    public Bounds GetHitbox(bool punch, Health opponent)
    {
        EnsureComponents();
        Bounds body = bodyCollider != null
            ? bodyCollider.bounds
            : new Bounds(transform.position, Vector3.one * 0.5f);

        float reach = punch ? PunchReach : KickReach;
        float direction = FacingDirection(opponent);
        Vector3 center = new Vector3(
            body.center.x + direction * (body.extents.x + reach * 0.5f),
            body.center.y + (punch ? punchHeightOffset : kickHeightOffset),
            0f
        );
        return new Bounds(center, new Vector3(reach, Mathf.Max(0.05f, hitboxHeight), 0.1f));
    }

    // Consultas da V1.7.4 retornam contato sem causar dano ou ganhar pontos.
    public bool OverlapsTarget(bool punch, Health opponent)
    {
        if (opponent == null || opponent.IsDead) return false;
        Bounds hitbox = GetHitbox(punch, opponent);

        Collider2D[] overlaps = Physics2D.OverlapBoxAll(
            hitbox.center, new Vector2(hitbox.size.x, hitbox.size.y), 0f
        );

        foreach (Collider2D collider in overlaps)
        {
            if (collider == null) continue;
            if (collider.GetComponentInParent<Health>() == opponent) return true;
        }

        return false;
    }

    [ContextMenu("V1.7.4 - Testar hitbox de SOCO")]
    private void ProbePunch()
    {
        Probe(true);
    }

    [ContextMenu("V1.7.4 - Testar hitbox de CHUTE")]
    private void ProbeKick()
    {
        Probe(false);
    }

    private void Probe(bool punch)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[V1.7.4] Inicie Play antes de testar a sobreposicao 2D.", this);
            return;
        }

        Health opponent = ResolveTarget();
        if (opponent == null)
        {
            Debug.LogWarning("[V1.7.4] Nao encontrei o Health do adversario.", this);
            return;
        }

        EnsureComponents();
        Collider2D opponentCollider = opponent.GetComponent<Collider2D>();
        if (bodyCollider == null || opponentCollider == null)
        {
            Debug.LogWarning("[V1.7.4] O teste precisa do Collider2D corporal de cada Mon.", this);
            return;
        }

        // Atualiza consultas quando o alvo foi reposicionado pelo Inspector em Play.
        Physics2D.SyncTransforms();
        bool hit = OverlapsTarget(punch, opponent);
        float reach = punch ? PunchReach : KickReach;
        float centerDistance = Mathf.Abs(opponent.transform.position.x - transform.position.x);
        float gapBetweenBodies = Mathf.Max(0f,
            centerDistance - bodyCollider.bounds.extents.x - opponentCollider.bounds.extents.x);

        Debug.Log(
            $"[V1.7.4 {(punch ? "SOCO" : "CHUTE")}] {name} -> {opponent.name}: " +
            $"resultado={(hit ? "CONTATO" : "FORA")}; alcance={reach:0.###} u; " +
            $"distanciaCentros={centerDistance:0.###} u; " +
            $"espacoEntreCorpos={gapBetweenBodies:0.###} u; " +
            $"bracosExt={anatomy?.bracos?.extensao ?? 0f:0.###}; " +
            $"pernasExt={anatomy?.pernas?.extensao ?? 0f:0.###}; " +
            $"troncoExt={anatomy?.tronco?.extensao ?? 0f:0.###}.", this
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawHitboxes) return;

        Health opponent = ResolveTarget();
        Color oldColor = Gizmos.color;

        Bounds punch = GetHitbox(true, opponent);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(punch.center, punch.size);

        Bounds kick = GetHitbox(false, opponent);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(kick.center, kick.size);

        Gizmos.color = oldColor;
    }
}
