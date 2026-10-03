using UnityEngine;
using System.Collections;

public class DamageFeedback : MonoBehaviour
{
    [Header("Flash")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.12f;

    [Header("Knockback")]
    public float knockbackDistance = 0.25f;
    public float knockbackDuration = 0.08f;

    private Health health;
    private SpriteRenderer spriteRenderer;
    private Renderer normalRenderer;

    private Color originalSpriteColor;
    private Color originalRendererColor;

    private Coroutine feedbackRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        normalRenderer = GetComponentInChildren<Renderer>();

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }

        if (normalRenderer != null && normalRenderer.material.HasProperty("_Color"))
        {
            originalRendererColor = normalRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(Health damagedHealth, int damageAmount, Transform attacker)
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(FeedbackRoutine(attacker));
    }

    private IEnumerator FeedbackRoutine(Transform attacker)
    {
        SetDamageColor();

        Vector3 startPosition = transform.position;
        Vector3 knockbackTarget = startPosition;

        if (attacker != null)
        {
            Vector3 direction = (transform.position - attacker.position).normalized;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            knockbackTarget = startPosition + direction * knockbackDistance;
        }

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            transform.position = Vector3.Lerp(startPosition, knockbackTarget, t);

            yield return null;
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColor();
    }

    private void SetDamageColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
        }

        if (normalRenderer != null && normalRenderer.material.HasProperty("_Color"))
        {
            normalRenderer.material.color = damageColor;
        }
    }

    private void RestoreOriginalColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }

        if (normalRenderer != null && normalRenderer.material.HasProperty("_Color"))
        {
            normalRenderer.material.color = originalRendererColor;
        }
    }
}