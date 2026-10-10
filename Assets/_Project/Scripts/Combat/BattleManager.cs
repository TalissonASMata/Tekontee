using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Combatentes")]
    public Combatant playerCombatant;
    public Combatant enemyCombatant;

    [Header("Controle do jogador")]
    public PlayerControlManager playerControlManager;

    [Header("UI")]
    public TMP_Text resultText;

    [Header("Fim da batalha")]
    public float endDelay = 0.45f;

    private bool battleEnded = false;

    private void Start()
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        if (playerControlManager != null)
        {
            playerControlManager.SetGameplayEnabled(true);
        }

        SubscribeToDeaths();
    }

    private void Update()
    {
        if (!battleEnded) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromDeaths();
    }

    private void SubscribeToDeaths()
    {
        if (playerCombatant != null && playerCombatant.Health != null)
        {
            playerCombatant.Health.OnDeath += HandleDeath;
        }

        if (enemyCombatant != null && enemyCombatant.Health != null)
        {
            enemyCombatant.Health.OnDeath += HandleDeath;
        }
    }

    private void UnsubscribeFromDeaths()
    {
        if (playerCombatant != null && playerCombatant.Health != null)
        {
            playerCombatant.Health.OnDeath -= HandleDeath;
        }

        if (enemyCombatant != null && enemyCombatant.Health != null)
        {
            enemyCombatant.Health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath(Health deadHealth)
    {
        if (battleEnded) return;

        if (playerCombatant != null && deadHealth == playerCombatant.Health)
        {
            EndBattle("Derrota");
            return;
        }

        if (enemyCombatant != null && deadHealth == enemyCombatant.Health)
        {
            EndBattle("Vitória");
            return;
        }
    }

    private void EndBattle(string result)
    {
        battleEnded = true;

        StopCombatants();
        StopPlayerControl();

        Invoke(nameof(ShowResult), endDelay);

        Debug.Log($"Fim da batalha: {result}\nPressione R para reiniciar");

        pendingResult = result;
    }

    private string pendingResult;

    private void ShowResult()
    {
        if (resultText == null) return;

        resultText.gameObject.SetActive(true);
        resultText.text = $"{pendingResult}\nPressione R para\nreiniciar";
    }

    private void StopCombatants()
    {
        if (playerCombatant != null)
        {
            playerCombatant.StopCombat();
        }

        if (enemyCombatant != null)
        {
            enemyCombatant.StopCombat();
        }
    }

    private void StopPlayerControl()
    {
        if (playerControlManager != null)
        {
            playerControlManager.SetGameplayEnabled(false);
        }
    }
}