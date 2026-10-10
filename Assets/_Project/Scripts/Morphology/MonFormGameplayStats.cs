using UnityEngine;

[DisallowMultipleComponent]
public class MonFormGameplayStats : MonoBehaviour
{
    [Header("Referência")]
    public MorphPointBank pointBank;

    [Header("Multiplicadores por forma dominante")]
    public float attackDamageMultiplier = 3f;
    public float defenseRepelMultiplier = 4f;
    public float airJumpMultiplier = 2f;
    public float energyMoveSpeedMultiplier = 1.8f;

    public MorphPointType CurrentDominantType
    {
        get
        {
            if (pointBank == null) return MorphPointType.Attack;
            return pointBank.DominantType;
        }
    }

    public bool HasAnyPoints
    {
        get
        {
            return pointBank != null && pointBank.TotalPoints > 0;
        }
    }

    public float DamageMultiplier
    {
        get
        {
            if (!HasAnyPoints) return 1f;
            return CurrentDominantType == MorphPointType.Attack ? attackDamageMultiplier : 1f;
        }
    }

    public float RepelMultiplier
    {
        get
        {
            if (!HasAnyPoints) return 1f;
            return CurrentDominantType == MorphPointType.Defense ? defenseRepelMultiplier : 1f;
        }
    }

    public float JumpMultiplier
    {
        get
        {
            if (!HasAnyPoints) return 1f;
            return CurrentDominantType == MorphPointType.Air ? airJumpMultiplier : 1f;
        }
    }

    public float MoveSpeedMultiplier
    {
        get
        {
            if (!HasAnyPoints) return 1f;
            return CurrentDominantType == MorphPointType.Energy ? energyMoveSpeedMultiplier : 1f;
        }
    }

    private MorphPointType lastLoggedType;
    private bool hasLoggedType;

    private void Awake()
    {
        if (pointBank == null)
        {
            pointBank = GetComponent<MorphPointBank>();
        }
    }

    private void OnEnable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged += HandlePointsChanged;
        }
    }

    private void OnDisable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged -= HandlePointsChanged;
        }
    }

    private void HandlePointsChanged(MorphPointBank bank)
    {
        if (!HasAnyPoints) return;

        MorphPointType currentType = CurrentDominantType;

        if (hasLoggedType && currentType == lastLoggedType) return;

        lastLoggedType = currentType;
        hasLoggedType = true;

        Debug.Log(
            $"{gameObject.name} atributos atualizados pela forma {currentType}. " +
            $"Dano x{DamageMultiplier}, Repel x{RepelMultiplier}, " +
            $"Pulo x{JumpMultiplier}, Velocidade x{MoveSpeedMultiplier}"
        );
    }
}