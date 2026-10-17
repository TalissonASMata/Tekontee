using UnityEngine;

[DisallowMultipleComponent]
public class MoveMorphologyEvaluator : MonoBehaviour
{
    [Header("Requisito do golpe")]
    public MorphPointType requiredType = MorphPointType.Attack;
    [Min(0)] public int effectiveThreshold = 5;
    [Min(0)] public int criticalThreshold = 10;

    [Header("Multiplicadores de dano")]
    [Min(0f)] public float failDamageMultiplier = 0.5f;
    [Min(0f)] public float effectiveDamageMultiplier = 1f;
    [Min(0f)] public float criticalDamageMultiplier = 1.5f;

    [Header("Multiplicadores de reforço morfológico")]
    [Min(0f)] public float failReinforcementMultiplier = 0.5f;
    [Min(0f)] public float effectiveReinforcementMultiplier = 1f;
    [Min(0f)] public float criticalReinforcementMultiplier = 2f;

    private MorphPointBank morphPointBank;

    private void Awake()
    {
        morphPointBank = GetComponent<MorphPointBank>();
    }

    public MoveExecutionQuality Evaluate()
    {
        if (morphPointBank == null)
        {
            return MoveExecutionQuality.Fail;
        }

        int currentValue = morphPointBank.GetPoints(requiredType);

        int safeEffectiveThreshold = Mathf.Max(0, effectiveThreshold);
        int safeCriticalThreshold = Mathf.Max(safeEffectiveThreshold, criticalThreshold);

        if (currentValue >= safeCriticalThreshold)
        {
            return MoveExecutionQuality.Critical;
        }

        if (currentValue >= safeEffectiveThreshold)
        {
            return MoveExecutionQuality.Effective;
        }

        return MoveExecutionQuality.Fail;
    }

    public float GetDamageMultiplier(MoveExecutionQuality quality)
    {
        switch (quality)
        {
            case MoveExecutionQuality.Critical:
                return Mathf.Max(0f, criticalDamageMultiplier);

            case MoveExecutionQuality.Effective:
                return Mathf.Max(0f, effectiveDamageMultiplier);

            default:
                return Mathf.Max(0f, failDamageMultiplier);
        }
    }

    public float GetReinforcementMultiplier(MoveExecutionQuality quality)
    {
        switch (quality)
        {
            case MoveExecutionQuality.Critical:
                return Mathf.Max(0f, criticalReinforcementMultiplier);

            case MoveExecutionQuality.Effective:
                return Mathf.Max(0f, effectiveReinforcementMultiplier);

            default:
                return Mathf.Max(0f, failReinforcementMultiplier);
        }
    }

    public int CurrentRequiredValue =>
        morphPointBank != null ? morphPointBank.GetPoints(requiredType) : 0;
}
