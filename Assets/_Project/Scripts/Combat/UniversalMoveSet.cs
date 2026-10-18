using System;
using UnityEngine;

public enum UniversalMoveId { HeavyStrike, AerialStrike, GuardBreak }

[Serializable]
public class UniversalMoveDefinition
{
    public UniversalMoveId id;
    public string displayName;
    public MorphPointType requirement;
    [Min(0)] public int effectiveThreshold = 5;
    [Min(0)] public int criticalThreshold = 10;
    [Min(0)] public int baseDamage = 10;
    [Min(0.1f)] public float range = 1.4f;
    public MorphInfluence primary = new MorphInfluence(MorphPointType.Attack, 3);
    public MorphInfluence secondary = new MorphInfluence(MorphPointType.Air, 2);
    public MorphInfluence tertiary = new MorphInfluence(MorphPointType.Energy, 1);
}

[DisallowMultipleComponent]
public class UniversalMoveSet : MonoBehaviour
{
    [Header("Movimentos universais - mesmos IDs para todos os Mons")]
    public UniversalMoveDefinition[] moves =
    {
        new UniversalMoveDefinition
        {
            id = UniversalMoveId.HeavyStrike, displayName = "Golpe pesado",
            requirement = MorphPointType.Attack, baseDamage = 10, range = 1.4f,
            primary = new MorphInfluence(MorphPointType.Attack, 3),
            secondary = new MorphInfluence(MorphPointType.Defense, 2),
            tertiary = new MorphInfluence(MorphPointType.Energy, 1)
        },
        new UniversalMoveDefinition
        {
            id = UniversalMoveId.AerialStrike, displayName = "Golpe aereo",
            requirement = MorphPointType.Air, baseDamage = 10, range = 1.4f,
            primary = new MorphInfluence(MorphPointType.Air, 3),
            secondary = new MorphInfluence(MorphPointType.Attack, 2),
            tertiary = new MorphInfluence(MorphPointType.Energy, 1)
        },
        new UniversalMoveDefinition
        {
            id = UniversalMoveId.GuardBreak, displayName = "Quebra de guarda (teste)",
            requirement = MorphPointType.Defense, baseDamage = 10, range = 1.4f,
            primary = new MorphInfluence(MorphPointType.Defense, 3),
            secondary = new MorphInfluence(MorphPointType.Attack, 2),
            tertiary = new MorphInfluence(MorphPointType.Energy, 1)
        }
    };

    private MorphPointBank bank;
    private MoveMorphologyEvaluator evaluator;
    private MonFormGameplayStats formStats;

    private void Awake()
    {
        bank = GetComponent<MorphPointBank>();
        evaluator = GetComponent<MoveMorphologyEvaluator>();
        formStats = GetComponent<MonFormGameplayStats>();
    }

    public UniversalMoveDefinition Find(UniversalMoveId id)
    {
        if (moves == null) return null;
        foreach (UniversalMoveDefinition move in moves)
            if (move != null && move.id == id) return move;
        return null;
    }

    public bool TryExecute(UniversalMoveId id, Health target)
    {
        UniversalMoveDefinition move = Find(id);
        if (move == null || target == null || target.IsDead) return false;
        if (Mathf.Abs(target.transform.position.x - transform.position.x) > move.range) return false;

        int pointsBefore = bank != null ? bank.GetPoints(move.requirement) : 0;
        int effective = Mathf.Max(0, move.effectiveThreshold);
        int critical = Mathf.Max(effective, move.criticalThreshold);
        MoveExecutionQuality quality = bank == null ? MoveExecutionQuality.Fail :
            pointsBefore >= critical ? MoveExecutionQuality.Critical :
            pointsBefore >= effective ? MoveExecutionQuality.Effective : MoveExecutionQuality.Fail;

        float damageQuality = evaluator != null ? evaluator.GetDamageMultiplier(quality) :
            (quality == MoveExecutionQuality.Fail ? 0.5f : quality == MoveExecutionQuality.Critical ? 1.5f : 1f);
        float reinforcement = evaluator != null ? evaluator.GetReinforcementMultiplier(quality) :
            (quality == MoveExecutionQuality.Fail ? 0.5f : quality == MoveExecutionQuality.Critical ? 2f : 1f);
        float formMultiplier = formStats != null ? formStats.DamageMultiplier : 1f;
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(move.baseDamage * damageQuality * formMultiplier));
        target.TakeDamage(finalDamage, transform);

        Debug.Log($"{gameObject.name} executou {move.displayName} ({id}) {quality}. " +
                  $"Requisito {move.requirement}: {pointsBefore} (Effective >= {effective}, Critical >= {critical}). " +
                  $"Dano {finalDamage}. Forma {((formStats != null && formStats.HasAnyPoints) ? formStats.CurrentDominantType.ToString() : "Normal")}.");

        if (bank != null)
            bank.AddInfluences(move.primary, move.secondary, move.tertiary, reinforcement);
        return true;
    }
}
