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

    [Header("Morte")]
    public bool applyDeathVisual = true;
    public Color deathColor = Color.gray;
    public float deathScaleY = 0.55f;
    public float deathScaleX = 1.15f;
    public float deathRotationZ = 0f;

    private Health health;
    private SpriteRenderer spriteRenderer;
    private Renderer normalRenderer;

    private Color originalSpriteColor;
    private Color originalRendererColor;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Coroutine feedbackRoutine;
    private bool isDead;

    private void Awake()
    {
        health = GetComponent<Health>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        normalRenderer = GetComponentInChildren<Renderer>();

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }

        if (normalRenderer == spriteRenderer)
        {
            normalRenderer = null;
        }

        if (normalRenderer != null && normalRenderer.material.HasProperty("_Color"))
        {
            originalRendererColor = normalRenderer.material.color;
        }

        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDamaged(Health damagedHealth, int damageAmount, Transform attacker)
    {
        if (isDead) return;

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(FeedbackRoutine(attacker));
    }

    private void HandleDeath(Health deadHealth)
    {
        isDead = true;

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (applyDeathVisual)
        {
            ApplyDeathVisual();
        }
    }

    private IEnumerator FeedbackRoutine(Transform attacker)
    {
        SetVisualColor(damageColor);

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

        if (!isDead)
        {
            RestoreOriginalColor();
        }
    }

    private void ApplyDeathVisual()
    {
        SetVisualColor(deathColor);

        transform.localScale = new Vector3(
            originalScale.x * deathScaleX,
            originalScale.y * deathScaleY,
            originalScale.z
        );

        transform.rotation = originalRotation * Quaternion.Euler(0f, 0f, deathRotationZ);
    }

    private void SetVisualColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        if (normalRenderer != null && normalRenderer.material.HasProperty("_Color"))
        {
            normalRenderer.material.color = color;
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