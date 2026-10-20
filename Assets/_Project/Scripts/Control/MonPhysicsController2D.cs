using UnityEngine;

/// <summary>
/// V1.7.1: motor fisico compartilhado por PlayerMon e EnemyMon.
/// Os controladores (jogador/IA) apenas enviam intencao de movimento.
/// Os valores de morfologia anatomica serao conectados em outra etapa.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class MonPhysicsController2D : MonoBehaviour
{
    [Header("Gravidade universal do Mon (unidades/s²)")]
    [Min(0.1f)] public float gravityAcceleration = 18f;

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

    private float inputX;
    private float requestedSpeed;
    private float knockbackControlUntil;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        stateMachine = GetComponent<CombatantStateMachine>();
        health = GetComponent<Health>();

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

        if (health != null && health.IsDead)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        if (Time.time >= knockbackControlUntil)
        {
            float desiredX = stateMachine != null && stateMachine.IsLocked
                ? 0f
                : Mathf.Clamp(inputX, -1f, 1f) * Mathf.Max(0f, requestedSpeed);

            body.linearVelocity = new Vector2(desiredX, body.linearVelocity.y);
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
        return true;
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
