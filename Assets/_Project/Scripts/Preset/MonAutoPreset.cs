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

        MorphInfluence primary = autoFighter.primaryInfluence;
        primary.type = primaryCombatFocus;
        autoFighter.primaryInfluence = primary;

        MorphInfluence secondary = autoFighter.secondaryInfluence;
        secondary.type = secondaryCombatFocus;
        autoFighter.secondaryInfluence = secondary;

        MorphInfluence tertiary = autoFighter.tertiaryInfluence;
        tertiary.type = specialFocus;
        autoFighter.tertiaryInfluence = tertiary;

        Debug.Log(
            $"{gameObject.name} preset aplicado. " +
            $"Foco primário: {primaryCombatFocus}, " +
            $"secundário: {secondaryCombatFocus}, " +
            $"especial: {specialFocus}"
        );
    }
}
