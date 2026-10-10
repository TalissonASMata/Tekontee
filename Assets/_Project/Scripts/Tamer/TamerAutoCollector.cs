using UnityEngine;

[DisallowMultipleComponent]
public class TamerAutoCollector : MonoBehaviour
{
    [Header("Controle automático")]
    public bool canAutoCollect = false;

    [Header("Foco de coleta")]
    public MorphPointType primaryCollectFocus = MorphPointType.Air;
    public MorphPointType secondaryCollectFocus = MorphPointType.Energy;

    [Header("Destino dos pontos")]
    public MorphPointBank targetPointBank;

    [Header("Movimento automático")]
    public float moveSpeed = 3f;
    public float collectRadius = 0.7f;
    public float minX = -5f;
    public float maxX = 5f;
    public bool clampX = true;

    private MorphPointCollectible currentTarget;

    private void Update()
    {
        if (!canAutoCollect) return;
        if (targetPointBank == null) return;

        currentTarget = FindBestTarget();

        if (currentTarget == null) return;

        MoveTowardTarget(currentTarget);
        TryCollectTarget(currentTarget);
    }

    private MorphPointCollectible FindBestTarget()
    {
        MorphPointCollectible[] collectibles =
            FindObjectsByType<MorphPointCollectible>(FindObjectsInactive.Exclude);

        MorphPointCollectible bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (MorphPointCollectible collectible in collectibles)
        {
            if (collectible == null) continue;
            if (!collectible.IsAvailable) continue;

            int priority = GetPriority(collectible.pointType);

            if (priority < 0) continue;

            float distance = Mathf.Abs(transform.position.x - collectible.transform.position.x);
            float score = distance + priority * 100f;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = collectible;
            }
        }

        return bestTarget;
    }

    private int GetPriority(MorphPointType type)
    {
        if (type == primaryCollectFocus) return 0;
        if (type == secondaryCollectFocus) return 1;

        return -1;
    }

    private void MoveTowardTarget(MorphPointCollectible target)
    {
        Vector3 position = transform.position;

        float directionX = Mathf.Sign(target.transform.position.x - position.x);

        if (Mathf.Abs(target.transform.position.x - position.x) > 0.05f)
        {
            position.x += directionX * moveSpeed * Time.deltaTime;
        }

        if (clampX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        transform.position = position;

        UpdateFacing(directionX);
    }

    private void TryCollectTarget(MorphPointCollectible target)
    {
        float horizontalDistance = Mathf.Abs(transform.position.x - target.transform.position.x);

        if (horizontalDistance <= collectRadius)
        {
            bool collected = target.TryCollect(targetPointBank, gameObject);

            if (collected)
            {
                Debug.Log(
                    $"{gameObject.name} automático coletou {target.pointType}. " +
                    $"Focos: {primaryCollectFocus} / {secondaryCollectFocus}"
                );
            }
        }
    }

    private void UpdateFacing(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
        transform.localScale = scale;
    }
}