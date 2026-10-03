using UnityEngine;
using System;

public class CombatantStateMachine : MonoBehaviour
{
    public CombatState CurrentState { get; private set; } = CombatState.Idle;

    public bool logStateChanges = true;

    public event Action<CombatState> OnStateChanged;

    private Health health;
    private float lockedUntil;

    public bool IsLocked
    {
        get
        {
            return CurrentState == CombatState.Attacking ||
                   CurrentState == CombatState.TakingDamage ||
                   CurrentState == CombatState.Dead;
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (CurrentState == CombatState.Dead) return;

        if ((CurrentState == CombatState.Attacking || CurrentState == CombatState.TakingDamage) &&
            Time.time >= lockedUntil)
        {
            SetState(CombatState.Idle);
        }
    }

    public void SetState(CombatState newState, float lockDuration = 0f)
    {
        if (CurrentState == CombatState.Dead) return;

        if (CurrentState == newState && lockDuration <= 0f) return;

        CurrentState = newState;

        if (lockDuration > 0f)
        {
            lockedUntil = Time.time + lockDuration;
        }

        if (logStateChanges)
        {
            Debug.Log($"{gameObject.name} mudou para estado: {CurrentState}");
        }

        OnStateChanged?.Invoke(CurrentState);
    }

    private void HandleDamaged(Health damagedHealth, int damageAmount, Transform attacker)
    {
        SetState(CombatState.TakingDamage, 0.12f);
    }

    private void HandleDeath(Health deadHealth)
    {
        CurrentState = CombatState.Dead;

        if (logStateChanges)
        {
            Debug.Log($"{gameObject.name} mudou para estado: {CurrentState}");
        }

        OnStateChanged?.Invoke(CurrentState);
    }
}