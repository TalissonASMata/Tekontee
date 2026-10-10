using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class MorphPointCollectible : MonoBehaviour
{
    [Header("Ponto gerado")]
    public MorphPointType pointType = MorphPointType.Attack;
    public int pointsAmount = 1;

    [Header("Coleta normal")]
    public bool canRespawn = true;
    public float respawnCooldown = 3f;

    [Header("Argumento")]
    public bool acceptsArgument = true;
    public TamerArgumentType compatibleArgument = TamerArgumentType.Intensify;
    public int argumentMultiplier = 8;

    [Header("Visual")]
    public bool hideWhenCollected = true;

    private bool isAvailable = true;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public bool IsAvailable => isAvailable;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public bool TryCollect(MorphPointBank targetPointBank, GameObject collector)
    {
        if (!isAvailable) return false;
        if (targetPointBank == null) return false;

        targetPointBank.AddPoints(pointType, pointsAmount);

        Debug.Log($"{collector.name} coletou {pointsAmount} ponto(s) de {pointType} para {targetPointBank.gameObject.name}.");

        if (canRespawn)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            Destroy(gameObject);
        }

        return true;
    }

    public bool TryUseArgument(MorphPointBank targetPointBank, GameObject user, TamerArgumentType argumentType)
    {
        if (!isAvailable) return false;
        if (targetPointBank == null) return false;
        if (!acceptsArgument) return false;
        if (argumentType != compatibleArgument) return false;

        int totalPoints = pointsAmount * argumentMultiplier;

        targetPointBank.AddPoints(pointType, totalPoints);

        Debug.Log(
            $"{user.name} usou Argumento {argumentType} em {gameObject.name}. " +
            $"Ganhou {totalPoints} ponto(s) de {pointType}. O objeto foi consumido."
        );

        Destroy(gameObject);

        return true;
    }

    private IEnumerator RespawnRoutine()
    {
        isAvailable = false;

        if (hideWhenCollected && spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(respawnCooldown);

        isAvailable = true;

        if (hideWhenCollected && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
    }
}