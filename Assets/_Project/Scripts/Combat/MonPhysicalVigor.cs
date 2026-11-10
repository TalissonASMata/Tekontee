using System.Collections;
using UnityEngine;

/// <summary>
/// V1.8.17: primeiro nucleo independente de Vigor Fisico e Estresse Fisico.
/// Apenas recuperacao numerica; nenhum golpe/movimento gasta vigor ainda.
/// O estresse e um estado interno, nunca uma barra separada de HUD.
/// </summary>
[DisallowMultipleComponent]
public sealed class MonPhysicalVigor : MonoBehaviour
{
    [Header("V1.8.17 - Recuperacao de Vigor Fisico (teste)")]
    [Min(1f)] public float maximumPhysicalVigor = 100f;
    [Min(0f)] public float physicalRecoveryPerSecond = 10f;
    [Min(1f)] public float maximumPhysicalStress = 100f;

    [Tooltip("Reducao maxima proporcional na taxa de recuperacao quando o estresse esta no teto. Valor experimental.")]
    [Range(0f, 0.95f)] public float maximumStressRecoveryPenalty = 0.5f;

    [Header("Estado atual - somente diagnostico no Inspector")]
    [SerializeField] private float currentPhysicalVigor;
    // Nao exibir medidor de estresse no HUD; o Inspector/log sao ferramentas de depuracao.
    [SerializeField] private float currentPhysicalStress;

    private Health health;
    private Coroutine diagnosticRoutine;

    public float CurrentPhysicalVigor => currentPhysicalVigor;
    public float MaximumPhysicalVigor => Mathf.Max(1f, maximumPhysicalVigor);

    private void Awake()
    {
        health = GetComponent<Health>();
        currentPhysicalVigor = MaximumPhysicalVigor;
        currentPhysicalStress = 0f;
    }

    private void Update()
    {
        if (health != null && health.IsDead)
            return;

        // Nesta fatia o estresse e definido SOMENTE pelos testes de diagnostico.
        // Seus gastos/acumulo e dissipacao por repouso/defesa entram depois.
        float recoveryRate = CalculatePhysicalRecoveryRate();
        currentPhysicalVigor = Mathf.MoveTowards(
            Mathf.Clamp(currentPhysicalVigor, 0f, MaximumPhysicalVigor),
            MaximumPhysicalVigor,
            recoveryRate * Time.deltaTime);
    }

    private float CalculatePhysicalRecoveryRate()
    {
        float stressRatio = Mathf.Clamp01(
            Mathf.Max(0f, currentPhysicalStress) / Mathf.Max(1f, maximumPhysicalStress));
        float recoveryFactor = Mathf.Clamp01(
            1f - stressRatio * Mathf.Clamp(maximumStressRecoveryPenalty, 0f, 0.95f));
        return Mathf.Max(0f, physicalRecoveryPerSecond) * recoveryFactor;
    }

    [ContextMenu("V1.8.17 - Testar recuperacao / Estresse 0")]
    private void TestRecoveryWithoutStress()
    {
        StartRecoveryDiagnostic(0f);
    }

    [ContextMenu("V1.8.17 - Testar recuperacao / Estresse 80")]
    private void TestRecoveryWithStress()
    {
        StartRecoveryDiagnostic(0.8f);
    }

    private void StartRecoveryDiagnostic(float stressRatio)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[V1.8.17] Os testes de recuperacao devem ser executados em Play.", this);
            return;
        }

        if (diagnosticRoutine != null)
            StopCoroutine(diagnosticRoutine);

        currentPhysicalVigor = Mathf.Min(40f, MaximumPhysicalVigor * 0.4f);
        currentPhysicalStress = Mathf.Clamp01(stressRatio) *
                                Mathf.Max(1f, maximumPhysicalStress);

        float recoveryRate = CalculatePhysicalRecoveryRate();
        Debug.Log(
            $"[V1.8.17 INICIO] {name}: vigorAntes={currentPhysicalVigor:0.###}; " +
            $"vigorMax={MaximumPhysicalVigor:0.###}; estresseInterno={currentPhysicalStress:0.###}; " +
            $"taxaRecuperacao={recoveryRate:0.###} u/s; " +
            $"fatorEstresse=x{(physicalRecoveryPerSecond > 0f ? recoveryRate / physicalRecoveryPerSecond : 0f):0.###}.",
            this);

        diagnosticRoutine = StartCoroutine(LogRecoveryAfterOneSecond(
            currentPhysicalVigor, Time.time));
    }

    private IEnumerator LogRecoveryAfterOneSecond(float startVigor, float startTime)
    {
        yield return new WaitForSeconds(1f);

        float seconds = Mathf.Max(0f, Time.time - startTime);
        float delta = currentPhysicalVigor - startVigor;
        Debug.Log(
            $"[V1.8.17 RECUPERACAO] {name}: estresseInterno={currentPhysicalStress:0.###}; " +
            $"vigorAntes={startVigor:0.###}; vigorDepois={currentPhysicalVigor:0.###}; " +
            $"ganhoReal={delta:0.###}; tempo={seconds:0.###} s; " +
            $"taxaRecuperacao={CalculatePhysicalRecoveryRate():0.###} u/s.",
            this);
        diagnosticRoutine = null;
    }
}
