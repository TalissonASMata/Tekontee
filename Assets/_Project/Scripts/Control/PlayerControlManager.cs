using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerControlManager : MonoBehaviour
{
    [Header("Estado geral")]
    public bool gameplayEnabled = true;

    [Header("Modo inicial")]
    public PlayerControlMode currentMode = PlayerControlMode.Tamer;

    [Header("Tamer")]
    public TamerController tamerController;
    public TamerPointCollector tamerPointCollector;
    public TamerAutoCollector tamerAutoCollector;
    public TamerArgumentUser tamerArgumentUser;

    [Header("Mon")]
    public AutoFighter monAutoFighter;
    public MonPlayerController monPlayerController;
    public MonAutoPreset monAutoPreset;

    [Header("UI opcional")]
    public TMP_Text controlModeText;

    public PlayerControlMode CurrentMode => currentMode;
    public bool GameplayEnabled => gameplayEnabled;

    private void Start()
    {
        if (monAutoPreset != null)
        {
            monAutoPreset.ApplyPreset();
        }

        ApplyControlMode();
    }

    private void Update()
    {
        if (!gameplayEnabled) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleControlMode();
        }
    }

    public void SetGameplayEnabled(bool enabled)
    {
        gameplayEnabled = enabled;
        ApplyControlMode();

        Debug.Log($"Gameplay enabled: {gameplayEnabled}");
    }

    private void ToggleControlMode()
    {
        if (currentMode == PlayerControlMode.Tamer)
        {
            currentMode = PlayerControlMode.Mon;
        }
        else
        {
            currentMode = PlayerControlMode.Tamer;
        }

        ApplyControlMode();
    }

    private void ApplyControlMode()
    {
        bool controllingTamer = gameplayEnabled && currentMode == PlayerControlMode.Tamer;
        bool controllingMon = gameplayEnabled && currentMode == PlayerControlMode.Mon;

        if (tamerController != null)
        {
            tamerController.canMove = controllingTamer;
            Debug.Log($"Tamer canMove: {tamerController.canMove}");
        }

        if (tamerPointCollector != null)
        {
            tamerPointCollector.canCollect = controllingTamer;
            Debug.Log($"Tamer manual collect enabled: {tamerPointCollector.canCollect}");
        }

        if (tamerAutoCollector != null)
        {
            tamerAutoCollector.canAutoCollect = controllingMon;
            Debug.Log($"Tamer auto collect: {tamerAutoCollector.canAutoCollect}");
        }

        if (tamerArgumentUser != null)
        {
            tamerArgumentUser.canUseArguments = controllingTamer;
            Debug.Log($"Tamer arguments enabled: {tamerArgumentUser.canUseArguments}");
        }

        if (monAutoFighter != null)
        {
            monAutoFighter.enabled = controllingTamer;
            Debug.Log($"Mon AutoFighter enabled: {monAutoFighter.enabled}");
        }

        if (monPlayerController != null)
        {
            monPlayerController.canControl = controllingMon;
            Debug.Log($"MonPlayerController canControl: {monPlayerController.canControl}");
        }

        if (controlModeText != null)
        {
            if (gameplayEnabled)
            {
                controlModeText.text = $"Controle: {currentMode}";
            }
            else
            {
                controlModeText.text = "Controle: encerrado";
            }
        }

        Debug.Log($"Controle atual: {currentMode}");
    }
}