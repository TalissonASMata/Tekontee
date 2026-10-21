using UnityEngine;

/// <summary>
/// V1.7.3 - Hipotese de conversao da anatomia em velocidade inicial de salto.
/// Coeficientes sao de teste e NAO constituem uma formula fisica definitiva.
/// Nenhuma variavel aqui modifica gravidade, sprite, corpo ou colisor.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MonAnatomy))]
public class MonAnatomicalJumpStats : MonoBehaviour
{
    [Header("Hipotese ajustavel da V1.7.3")]
    [Tooltip("Ganho de propulsao por unidade de Massa de uma perna.")]
    [Min(0f)] public float legMassDrive = 0.25f;

    [Tooltip("Ganho de propulsao por unidade de Extensao das pernas.")]
    [Min(0f)] public float legExtensionDrive = 0.25f;

    [Tooltip("Penalidade de inercia por unidade de MassaGeral.")]
    [Min(0f)] public float globalMassPenalty = 0.08f;

    [Header("Limites provisórios para teste")]
    [Min(0.1f)] public float minimumMultiplier = 0.5f;
    [Min(0.1f)] public float maximumMultiplier = 2.25f;

    private MonAnatomy anatomy;

    private void Awake()
    {
        anatomy = GetComponent<MonAnatomy>();
    }

    public float CalculateMultiplier()
    {
        if (anatomy == null)
            anatomy = GetComponent<MonAnatomy>();

        if (anatomy == null || anatomy.pernas == null) return 1f;

        float legMass = Mathf.Max(0f, anatomy.pernas.massa);
        float legExtension = Mathf.Max(0f, anatomy.pernas.extensao);
        float totalMass = Mathf.Max(0f, anatomy.MassaGeral);

        // Propulsao cresce com perna; a massa total dificulta o impulso.
        // A quantidade de pernas participa do total de massa, mas nao
        // acrescenta bonus automatico de propulsao (regra ainda em aberto).
        float propulsion = 1f
            + legMass * Mathf.Max(0f, legMassDrive)
            + legExtension * Mathf.Max(0f, legExtensionDrive);

        float inertia = Mathf.Sqrt(1f + totalMass * Mathf.Max(0f, globalMassPenalty));
        float rawMultiplier = propulsion / Mathf.Max(0.01f, inertia);

        float minimum = Mathf.Max(0.1f, minimumMultiplier);
        float maximum = Mathf.Max(minimum, maximumMultiplier);
        return Mathf.Clamp(rawMultiplier, minimum, maximum);
    }

    public float CalculateJumpSpeed(float baseJumpSpeed)
    {
        return Mathf.Max(0f, baseJumpSpeed) * CalculateMultiplier();
    }

    [ContextMenu("V1.7.3 - Mostrar calculo de salto")]
    public void LogJumpCalculation()
    {
        if (anatomy == null)
            anatomy = GetComponent<MonAnatomy>();

        if (anatomy == null || anatomy.pernas == null) return;

        Debug.Log(
            $"[V1.7.3 ANATOMIA] {name}: Pernas.Massa={anatomy.pernas.massa:0.###}; " +
            $"Pernas.Extensao={anatomy.pernas.extensao:0.###}; " +
            $"MassaGeral={anatomy.MassaGeral:0.###}; " +
            $"multiplicadorSalto=x{CalculateMultiplier():0.###}."
        );
    }
}
