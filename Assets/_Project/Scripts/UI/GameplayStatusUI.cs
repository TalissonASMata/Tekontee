using UnityEngine;
using TMPro;

public class GameplayStatusUI : MonoBehaviour
{
    [Header("Referências")]
    public PlayerControlManager controlManager;
    public TamerArgumentUser argumentUser;
    public MorphPointBank pointBank;
    public MonFormGameplayStats formStats;

    [Header("Texto")]
    public TMP_Text text;

    private void Update()
    {
        if (text == null) return;

        string controlLine = GetControlLine();
        string formLine = GetFormLine();
        string argumentLine = GetArgumentLine();
        string helpLine = GetHelpLine();

        text.text =
            $"{controlLine}\n" +
            $"{formLine}\n" +
            $"{argumentLine}\n" +
            $"{helpLine}";
    }

    private string GetControlLine()
    {
        if (controlManager == null)
        {
            return "Controle: ?";
        }

        if (!controlManager.GameplayEnabled)
        {
            return "Controle: encerrado";
        }

        return $"Controle: {controlManager.CurrentMode}";
    }

    private string GetFormLine()
    {
        if (pointBank == null || pointBank.TotalPoints <= 0)
        {
            return "Forma: Normal";
        }

        string dominant = pointBank.DominantType.ToString();

        if (formStats == null)
        {
            return $"Forma: {dominant}";
        }

        return
            $"Forma: {dominant} | " +
            $"Dano x{formStats.DamageMultiplier:0.#} | " +
            $"Repel x{formStats.RepelMultiplier:0.#} | " +
            $"Pulo x{formStats.JumpMultiplier:0.#} | " +
            $"Vel x{formStats.MoveSpeedMultiplier:0.#}";
    }

    private string GetArgumentLine()
    {
        if (argumentUser == null)
        {
            return "Argumento: ?";
        }

        if (argumentUser.IsArgumentActive)
        {
            return $"Argumento: {argumentUser.EquippedArgument} ativo ({argumentUser.RemainingActiveTime:0.0}s)";
        }

        return $"Argumento: {argumentUser.EquippedArgument} pronto";
    }

    private string GetHelpLine()
    {
        if (controlManager == null || !controlManager.GameplayEnabled)
        {
            return "R: reiniciar";
        }

        if (controlManager.CurrentMode == PlayerControlMode.Tamer)
        {
            return "E: coletar | Q: Intensify | Tab: controlar Mon";
        }

        return "A/D: mover | W/Espaço: pular | J: atacar | Tab: controlar Tamer";
    }
}