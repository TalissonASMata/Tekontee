using UnityEngine;
using System;

[DisallowMultipleComponent]
public class MorphPointBank : MonoBehaviour
{
    [Header("Pontos Morfológicos")]
    public int attackPoints;
    public int defensePoints;
    public int airPoints;
    public int energyPoints;

    public event Action<MorphPointBank> OnPointsChanged;

    public int TotalPoints => attackPoints + defensePoints + airPoints + energyPoints;

    public MorphPointType DominantType
    {
        get
        {
            int highest = attackPoints;
            MorphPointType dominant = MorphPointType.Attack;

            if (defensePoints > highest)
            {
                highest = defensePoints;
                dominant = MorphPointType.Defense;
            }

            if (airPoints > highest)
            {
                highest = airPoints;
                dominant = MorphPointType.Air;
            }

            if (energyPoints > highest)
            {
                dominant = MorphPointType.Energy;
            }

            return dominant;
        }
    }

    public void AddPoints(MorphPointType type, int amount)
    {
        if (!AddPointsInternal(type, amount)) return;

        Debug.Log($"{gameObject.name} ganhou {amount} ponto(s) de {type}. Dominante atual: {DominantType}");

        OnPointsChanged?.Invoke(this);
    }

    public void AddInfluences(
        MorphInfluence primary,
        MorphInfluence secondary,
        MorphInfluence tertiary)
    {
        AddInfluences(primary, secondary, tertiary, 1f);
    }

    public void AddInfluences(
        MorphInfluence primary,
        MorphInfluence secondary,
        MorphInfluence tertiary,
        float multiplier)
    {
        float safeMultiplier = Mathf.Max(0f, multiplier);

        int primaryAmount = ScaleInfluenceAmount(primary.amount, safeMultiplier);
        int secondaryAmount = ScaleInfluenceAmount(secondary.amount, safeMultiplier);
        int tertiaryAmount = ScaleInfluenceAmount(tertiary.amount, safeMultiplier);

        bool changed = false;

        changed |= AddPointsInternal(primary.type, primaryAmount);
        changed |= AddPointsInternal(secondary.type, secondaryAmount);
        changed |= AddPointsInternal(tertiary.type, tertiaryAmount);

        if (!changed) return;

        Debug.Log(
            $"{gameObject.name} recebeu influência morfológica: " +
            $"Primária {primary.type} +{primaryAmount}, " +
            $"Secundária {secondary.type} +{secondaryAmount}, " +
            $"Terciária {tertiary.type} +{tertiaryAmount}. " +
            $"Multiplicador x{safeMultiplier:0.##}. " +
            $"Dominante atual: {DominantType}"
        );

        OnPointsChanged?.Invoke(this);
    }

    private int ScaleInfluenceAmount(int baseAmount, float multiplier)
    {
        if (baseAmount <= 0 || multiplier <= 0f) return 0;

        return Mathf.Max(1, Mathf.RoundToInt(baseAmount * multiplier));
    }

    private bool AddPointsInternal(MorphPointType type, int amount)
    {
        if (amount <= 0) return false;

        switch (type)
        {
            case MorphPointType.Attack:
                attackPoints += amount;
                break;

            case MorphPointType.Defense:
                defensePoints += amount;
                break;

            case MorphPointType.Air:
                airPoints += amount;
                break;

            case MorphPointType.Energy:
                energyPoints += amount;
                break;

            default:
                return false;
        }

        return true;
    }

    public int GetPoints(MorphPointType type)
    {
        switch (type)
        {
            case MorphPointType.Attack:
                return attackPoints;

            case MorphPointType.Defense:
                return defensePoints;

            case MorphPointType.Air:
                return airPoints;

            case MorphPointType.Energy:
                return energyPoints;

            default:
                return 0;
        }
    }

    public string GetDebugText()
    {
        return
            $"Morph Points\n" +
            $"Attack: {attackPoints}\n" +
            $"Defense: {defensePoints}\n" +
            $"Air: {airPoints}\n" +
            $"Energy: {energyPoints}\n" +
            $"Dominante: {DominantType}";
    }

    public void ResetPoints()
    {
        attackPoints = 0;
        defensePoints = 0;
        airPoints = 0;
        energyPoints = 0;

        OnPointsChanged?.Invoke(this);
    }
}
