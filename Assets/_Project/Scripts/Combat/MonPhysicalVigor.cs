using System.Collections;
using UnityEngine;

/// <summary>
/// V1.8.17: primeiro nucleo independente de Vigor Fisico e Estresse Fisico.
/// V1.8.17.1: estresse diminui apenas durante repouso fisico real.
/// Nenhum golpe/movimento gasta vigor ou gera estresse ainda.
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

    [Header("V1.8.17.1 - Dissipacao do estresse em repouso (teste)")]
    [Tooltip("Somente quando no chao, sem comando, sem deslocamento e sem combate.")]
    public bool enableIdlePhysicalStressDecay = true;
    [Tooltip("Reducao de Estresse Fisico por segundo durante repouso valido.")]
    [Min(0f)] public float idlePhysicalStressDecayPerSecond = 5f;
    [Tooltip("Velocidade X maxima considerada repouso (u/s).")]
    [Min(0f)] public float maximumRestHorizontalSpeed = 0.10f;
    [Tooltip("Velocidade Y maxima considerada repouso (u/s).")]
    [Min(0f)] public float maximumRestVerticalSpeed = 0.10f;

    [Header("Estado atual - somente diagnostico no Inspector")]
    [SerializeField] private float currentPhysicalVigor;
    // Nao exibir medidor de estresse no HUD; o Inspector/log sao ferramentas de depuracao.
    [SerializeField] private float currentPhysicalStress;

    private Health health;
    private MonPhysicsController2D physicsMotor;
    private Coroutine diagnosticRoutine;
    private bool freezeStressDecayForRecoveryDiagnostic;

    public float CurrentPhysicalVigor => currentPhysicalVigor;
    public float MaximumPhysicalVigor => Mathf.Max(1f, maximumPhysicalVigor);

    private void Awake()
    {
        health = GetComponent<Health>();
        physicsMotor = GetComponent<MonPhysicsController2D>();
        currentPhysicalVigor = MaximumPhysicalVigor;
        currentPhysicalStress = 0f;
    }

    private void Update()
    {
        if (health != null && health.IsDead)
            return;

        // V1.8.17.1: somente em repouso verdadeiro. A entrada do jogador,
        // deslocamento residual, acoes de combate e knockback impedem dissipacao.
        // Os antigos testes V1.8.17 congelam estresse apenas durante a medicao
        // para continuar comparaveis com os resultados ja validados.
        if (enableIdlePhysicalStressDecay &&
            !freezeStressDecayForRecoveryDiagnostic &&
            currentPhysicalStress > 0f &&
            physicsMotor != null &&
            physicsMotor.IsRestingForPhysicalStressRecovery(
                maximumRestHorizontalSpeed, maximumRestVerticalSpeed))
        {
            currentPhysicalStress = Mathf.MoveTowards(
                currentPhysicalStress, 0f,
                Mathf.Max(0f, idlePhysicalStressDecayPerSecond) * Time.deltaTime);
        }

        // Conforme o estresse cai, a taxa de vigor se recupera gradualmente.
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

    [ContextMenu("V1.8.17.1 - Testar repouso / Estresse 80")]
    private void TestIdleStressDissipation()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[V1.8.17.1] Execute o teste em Play.", this);
            return;
        }

        // Nao forcar o repouso: exigir condicoes reais de cena/fisica.
        // Para testar, controlar Mon com Tab, ficar no chao e soltar as teclas.
        if (physicsMotor == null ||
            !physicsMotor.IsRestingForPhysicalStressRecovery(
                maximumRestHorizontalSpeed, maximumRestVerticalSpeed))
        {
            Debug.LogWarning(
                "[V1.8.17.1] Mon nao esta em repouso: use Tab para controlar " +
                "Mon, solte direcoes, fique parado no solo e aguarde sair de combate.",
                this);
            return;
        }

        if (diagnosticRoutine != null)
            StopCoroutine(diagnosticRoutine);

        freezeStressDecayForRecoveryDiagnostic = false;
        currentPhysicalVigor = Mathf.Min(40f, MaximumPhysicalVigor * 0.4f);
        currentPhysicalStress = 0.8f * Mathf.Max(1f, maximumPhysicalStress);
        float initialRecoveryRate = CalculatePhysicalRecoveryRate();
        Debug.Log(
            $"[V1.8.17.1 REPOUSO INICIO] {name}: repouso=SIM; " +
            $"vigorAntes={currentPhysicalVigor:0.###}; " +
            $"estresseAntes={currentPhysicalStress:0.###}; " +
            $"dissipacao={Mathf.Max(0f, idlePhysicalStressDecayPerSecond):0.###} u/s; " +
            $"taxaVigorAntes={initialRecoveryRate:0.###} u/s.", this);

        diagnosticRoutine = StartCoroutine(LogIdleStressAfterOneSecond(
            currentPhysicalVigor, currentPhysicalStress, Time.time));
    }

    private IEnumerator LogIdleStressAfterOneSecond(
        float vigorBefore, float stressBefore, float startTime)
    {
        yield return new WaitForSeconds(1f);

        float seconds = Mathf.Max(0f, Time.time - startTime);
        bool restingNow = physicsMotor != null &&
            physicsMotor.IsRestingForPhysicalStressRecovery(
                maximumRestHorizontalSpeed, maximumRestVerticalSpeed);
        Debug.Log(
            $"[V1.8.17.1 REPOUSO FIM] {name}: " +
            $"repousoAgora={(restingNow ? "SIM" : "NAO")}; " +
            $"tempo={seconds:0.###} s; " +
            $"estresseAntes={stressBefore:0.###}; " +
            $"estresseDepois={currentPhysicalStress:0.###}; " +
            $"reducaoEstresse={stressBefore - currentPhysicalStress:0.###}; " +
            $"vigorAntes={vigorBefore:0.###}; " +
            $"vigorDepois={currentPhysicalVigor:0.###}; " +
            $"taxaVigorDepois={CalculatePhysicalRecoveryRate():0.###} u/s.", this);
        diagnosticRoutine = null;
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

        // Congela o estresse apenas durante os antigos testes de taxa
        // constante, preservando a comparacao 10 u/s vs 6 u/s da V1.8.17.
        freezeStressDecayForRecoveryDiagnostic = true;
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
        freezeStressDecayForRecoveryDiagnostic = false;
    }
}
