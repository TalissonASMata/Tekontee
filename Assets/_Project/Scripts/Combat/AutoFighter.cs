using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class AutoFighter : MonoBehaviour
{
    [Header("Alvo")]
    public Health target;

    [Header("Movimento")]
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;

    [Header("Ataque")]
    public int damage = 10;
    public float attackCooldown = 1f;
    public float attackPulseScale = 1.2f;
    public float attackPulseDuration = 0.12f;

    [Header("Repel bônus")]
    public float baseBonusRepelDistance = 0.35f;

    [Header("Morfologia")]
    public MorphPointType pointTypeOnHit = MorphPointType.Attack;
    public int pointsOnHit = 1;

    [Header("Forma dominante")]
    public MonFormGameplayStats formStats;

    private float nextAttackTime;
    private Health selfHealth;
    private CombatantStateMachine stateMachine;
    private MorphPointBank morphPointBank;
    private Vector3 originalScale;
    private Coroutine attackPulseRoutine;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        stateMachine = GetComponent<CombatantStateMachine>();
        morphPointBank = GetComponent<MorphPointBank>();

        if (formStats == null)
        {
            formStats = GetComponent<MonFormGameplayStats>();
        }

        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (target == null)
        {
            SetState(CombatState.Idle);
            return;
        }

        if (selfHealth != null && selfHealth.IsDead)
        {
            SetState(CombatState.Dead);
            return;
        }

        if (target.IsDead)
        {
            SetState(CombatState.Idle);
            return;
        }

        if (stateMachine != null && stateMachine.IsLocked)
        {
            return;
        }

        float distance = Mathf.Abs(target.transform.position.x - transform.position.x);

        if (distance > attackRange)
        {
            SetState(CombatState.Moving);
            MoveTowardTarget();
        }
        else
        {
            SetState(CombatState.Idle);
            TryAttack();
        }
    }

    private void MoveTowardTarget()
    {
        if (target == null) return;

        float directionX = Mathf.Sign(target.transform.position.x - transform.position.x);
        float finalMoveSpeed = moveSpeed * GetMoveSpeedMultiplier();

        transform.position += new Vector3(directionX * finalMoveSpeed * Time.deltaTime, 0f, 0f);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        if (target == null) return;
        if (target.IsDead) return;

        SetState(CombatState.Attacking, attackPulseDuration);

        int finalDamage = GetFinalDamage();

        target.TakeDamage(finalDamage, transform);

        ApplyBonusRepelToTarget();

        AddMorphPointsOnHit();

        PlayAttackPulse();

        Debug.Log(
            $"{gameObject.name} atacou com dano final {finalDamage}. " +
            $"Forma dominante: {GetCurrentFormName()}"
        );

        nextAttackTime = Time.time + attackCooldown;
    }

    private int GetFinalDamage()
    {
        float multiplier = formStats != null ? formStats.DamageMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    private float GetMoveSpeedMultiplier()
    {
        return formStats != null ? formStats.MoveSpeedMultiplier : 1f;
    }

    private float GetRepelMultiplier()
    {
        return formStats != null ? formStats.RepelMultiplier : 1f;
    }

    private string GetCurrentFormName()
    {
        if (formStats == null || !formStats.HasAnyPoints) return "Normal";
        return formStats.CurrentDominantType.ToString();
    }

    private void ApplyBonusRepelToTarget()
    {
        if (target == null) return;

        float repelMultiplier = GetRepelMultiplier();

        if (repelMultiplier <= 1f) return;

        float directionX = Mathf.Sign(target.transform.position.x - transform.position.x);
        float bonusRepelDistance = baseBonusRepelDistance * (repelMultiplier - 1f);

        target.transform.position += new Vector3(directionX * bonusRepelDistance, 0f, 0f);

        Debug.Log($"{gameObject.name} aplicou repel bônus x{repelMultiplier}.");
    }

    private void AddMorphPointsOnHit()
    {
        if (morphPointBank == null) return;
        if (pointsOnHit <= 0) return;

        morphPointBank.AddPoints(pointTypeOnHit, pointsOnHit);
    }

    private void PlayAttackPulse()
    {
        if (attackPulseRoutine != null)
        {
            StopCoroutine(attackPulseRoutine);
        }

        attackPulseRoutine = StartCoroutine(AttackPulseRoutine());
    }

    private IEnumerator AttackPulseRoutine()
    {
        transform.localScale = originalScale * attackPulseScale;

        yield return new WaitForSeconds(attackPulseDuration);

        transform.localScale = originalScale;
    }

    private void SetState(CombatState state)
    {
        if (stateMachine == null) return;

        stateMachine.SetState(state);
    }

    private void SetState(CombatState state, float lockDuration)
    {
        if (stateMachine == null) return;

        stateMachine.SetState(state, lockDuration);
    }
}