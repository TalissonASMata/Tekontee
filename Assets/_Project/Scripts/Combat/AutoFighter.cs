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

    [Header("Morfologia por acerto")]
    public MorphInfluence primaryInfluence = new MorphInfluence(MorphPointType.Attack, 1);
    public MorphInfluence secondaryInfluence = new MorphInfluence(MorphPointType.Defense, 0);
    public MorphInfluence tertiaryInfluence = new MorphInfluence(MorphPointType.Energy, 0);

    [Header("Forma dominante")]
    public MonFormGameplayStats formStats;

    private float nextAttackTime;
    private Health selfHealth;
    private CombatantStateMachine stateMachine;
    private MorphPointBank morphPointBank;
    private MoveMorphologyEvaluator moveEvaluator;
    private UniversalMoveSet universalMoves;
    private int autoMoveIndex;
    private Vector3 originalScale;
    private Coroutine attackPulseRoutine;

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        stateMachine = GetComponent<CombatantStateMachine>();
        morphPointBank = GetComponent<MorphPointBank>();
        moveEvaluator = GetComponent<MoveMorphologyEvaluator>();
        universalMoves = GetComponent<UniversalMoveSet>();

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
        UniversalMoveId selectedMove = autoMoveIndex % 2 == 0 ? UniversalMoveId.HeavyStrike : UniversalMoveId.AerialStrike;
        UniversalMoveDefinition definition = universalMoves != null ? universalMoves.Find(selectedMove) : null;
        float desiredRange = definition != null ? definition.range : attackRange;

        if (distance > desiredRange)
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

        if (universalMoves != null)
        {
            UniversalMoveId selectedMove = autoMoveIndex % 2 == 0 ? UniversalMoveId.HeavyStrike : UniversalMoveId.AerialStrike;
            if (!universalMoves.TryExecute(selectedMove, target)) return;
            autoMoveIndex++;
            SetState(CombatState.Attacking, attackPulseDuration);
            ApplyBonusRepelToTarget();
            PlayAttackPulse();
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        SetState(CombatState.Attacking, attackPulseDuration);

        MoveExecutionQuality quality = EvaluateMoveQuality();
        int finalDamage = GetFinalDamage(quality);

        target.TakeDamage(finalDamage, transform);

        ApplyBonusRepelToTarget();

        Debug.Log(
            $"{gameObject.name} atacou com qualidade {quality} e dano final {finalDamage}. " +
            $"Requisito: {GetRequirementDebugText()}. " +
            $"Forma dominante: {GetCurrentFormName()}"
        );

        AddMorphInfluencesOnHit(quality);

        PlayAttackPulse();

        nextAttackTime = Time.time + attackCooldown;
    }

    private MoveExecutionQuality EvaluateMoveQuality()
    {
        return moveEvaluator != null
            ? moveEvaluator.Evaluate()
            : MoveExecutionQuality.Effective;
    }

    private int GetFinalDamage(MoveExecutionQuality quality)
    {
        float formMultiplier = formStats != null ? formStats.DamageMultiplier : 1f;
        float qualityMultiplier = moveEvaluator != null
            ? moveEvaluator.GetDamageMultiplier(quality)
            : 1f;

        return Mathf.Max(1, Mathf.RoundToInt(damage * formMultiplier * qualityMultiplier));
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

    private string GetRequirementDebugText()
    {
        if (moveEvaluator == null) return "sem avaliador (Effective por compatibilidade)";

        return
            $"{moveEvaluator.requiredType} {moveEvaluator.CurrentRequiredValue} " +
            $"(Effective >= {moveEvaluator.effectiveThreshold}, Critical >= {moveEvaluator.criticalThreshold})";
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

    private void AddMorphInfluencesOnHit(MoveExecutionQuality quality)
    {
        if (morphPointBank == null) return;

        float reinforcementMultiplier = moveEvaluator != null
            ? moveEvaluator.GetReinforcementMultiplier(quality)
            : 1f;

        morphPointBank.AddInfluences(
            primaryInfluence,
            secondaryInfluence,
            tertiaryInfluence,
            reinforcementMultiplier
        );
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
