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
                highest = energyPoints;
                dominant = MorphPointType.Energy;
            }

            return dominant;
        }
    }

    public void AddPoints(MorphPointType type, int amount)
    {
        if (amount <= 0) return;

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
        }

        Debug.Log($"{gameObject.name} ganhou {amount} ponto(s) de {type}. Dominante atual: {DominantType}");

        OnPointsChanged?.Invoke(this);
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