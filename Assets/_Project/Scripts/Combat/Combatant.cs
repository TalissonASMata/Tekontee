using UnityEngine;

public enum CombatantTeam
{
    Player,
    Enemy
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CombatantStateMachine))]
public class Combatant : MonoBehaviour
{
    [Header("Identidade")]
    public string displayName;
    public CombatantTeam team;

    [Header("Componentes")]
    [SerializeField] private Health health;
    [SerializeField] private AutoFighter fighter;
    [SerializeField] private DamageFeedback feedback;
    [SerializeField] private CombatantStateMachine stateMachine;

    public Health Health => health;
    public AutoFighter Fighter => fighter;
    public DamageFeedback Feedback => feedback;
    public CombatantStateMachine StateMachine => stateMachine;

    public bool IsDead => health != null && health.IsDead;

    private void Awake()
    {
        CacheComponents();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = gameObject.name;
        }
    }

    private void Reset()
    {
        CacheComponents();
        displayName = gameObject.name;
    }

    private void OnValidate()
    {
        CacheComponents();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = gameObject.name;
        }
    }

    private void CacheComponents()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (fighter == null)
        {
            fighter = GetComponent<AutoFighter>();
        }

        if (feedback == null)
        {
            feedback = GetComponent<DamageFeedback>();
        }

        if (stateMachine == null)
        {
            stateMachine = GetComponent<CombatantStateMachine>();
        }
    }

    public void SetFighterEnabled(bool enabled)
    {
        if (fighter != null)
        {
            fighter.enabled = enabled;
        }
    }

    public void StopCombat()
    {
        SetFighterEnabled(false);
    }
}