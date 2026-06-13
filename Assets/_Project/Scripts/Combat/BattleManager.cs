using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public Health playerMon;
    public Health enemyMon;

    public TMP_Text resultText;

    private bool battleEnded;

    private void Start()
    {
        battleEnded = false;

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (playerMon != null)
        {
            playerMon.OnDeath += HandleDeath;
        }

        if (enemyMon != null)
        {
            enemyMon.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (!battleEnded) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartBattle();
        }
    }

    private void HandleDeath(Health deadCharacter)
    {
        if (battleEnded) return;

        if (deadCharacter == enemyMon)
        {
            EndBattle("Vitória\nPressione R para reiniciar");
        }
        else if (deadCharacter == playerMon)
        {
            EndBattle("Derrota\nPressione R para reiniciar");
        }
    }

    private void EndBattle(string result)
    {
        battleEnded = true;

        Debug.Log($"Fim da batalha: {result}");

        StopFighters();

        if (resultText != null)
        {
            resultText.text = result;
            resultText.gameObject.SetActive(true);
        }
    }

    private void StopFighters()
    {
        if (playerMon != null)
        {
            AutoFighter playerFighter = playerMon.GetComponent<AutoFighter>();

            if (playerFighter != null)
            {
                playerFighter.enabled = false;
            }
        }

        if (enemyMon != null)
        {
            AutoFighter enemyFighter = enemyMon.GetComponent<AutoFighter>();

            if (enemyFighter != null)
            {
                enemyFighter.enabled = false;
            }
        }
    }

    private void RestartBattle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}