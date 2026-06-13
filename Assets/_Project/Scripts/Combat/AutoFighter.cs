using UnityEngine;

public class AutoFighter : MonoBehaviour
{
    public Health target;

    [Header("Movimento")]
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;

    [Header("Ataque")]
    public int damage = 10;
    public float attackCooldown = 1f;

    private float nextAttackTime;

    private Health selfHealth;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
    }

    private void Update()
    {
        if (target == null) return;
        if (selfHealth != null && selfHealth.IsDead) return;
        if (target.IsDead) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            MoveTowardTarget();
        }
        else
        {
            TryAttack();
        }
    }

    private void MoveTowardTarget()
    {
        Vector2 direction = (target.transform.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        target.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
    }
}