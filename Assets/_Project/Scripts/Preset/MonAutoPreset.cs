using UnityEngine;

[DisallowMultipleComponent]
public class MonAutoPreset : MonoBehaviour
{
    [Header("Foco automático do Mon")]
    public MorphPointType primaryCombatFocus = MorphPointType.Attack;
    public MorphPointType secondaryCombatFocus = MorphPointType.Air;
    public MorphPointType specialFocus = MorphPointType.Energy;

    [Header("Referências")]
    public AutoFighter autoFighter;

    private void Awake()
    {
        if (autoFighter == null)
        {
            autoFighter = GetComponent<AutoFighter>();
        }
    }

    private void Start()
    {
        ApplyPreset();
    }

    public void ApplyPreset()
    {
        if (autoFighter == null) return;

        autoFighter.pointTypeOnHit = primaryCombatFocus;

        Debug.Log(
            $"{gameObject.name} preset aplicado. " +
            $"Foco primário: {primaryCombatFocus}, " +
            $"secundário: {secondaryCombatFocus}, " +
            $"especial: {specialFocus}"
        );
    }
}