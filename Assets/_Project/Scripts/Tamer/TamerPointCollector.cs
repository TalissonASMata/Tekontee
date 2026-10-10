using UnityEngine;
using UnityEngine.InputSystem;

public class TamerPointCollector : MonoBehaviour
{
    [Header("Estado")]
    public bool canCollect = true;

    [Header("Destino dos pontos")]
    public MorphPointBank targetPointBank;

    [Header("Coleta manual")]
    public float collectRadius = 0.7f;
    public Key collectKey = Key.E;

    [Header("Argumento")]
    public TamerArgumentUser argumentUser;

    private void Awake()
    {
        if (argumentUser == null)
        {
            argumentUser = GetComponent<TamerArgumentUser>();
        }
    }

    private void Update()
    {
        if (!canCollect) return;
        if (targetPointBank == null) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[collectKey].wasPressedThisFrame)
        {
            TryCollectNearestObject();
        }
    }

    private void TryCollectNearestObject()
    {
        MorphPointCollectible collectible = FindNearestAvailableCollectible();

        if (collectible == null)
        {
            Debug.Log($"{gameObject.name} tentou coletar, mas não havia objeto próximo.");
            return;
        }

        if (argumentUser != null && argumentUser.IsArgumentActive)
        {
            bool argumentWorked = argumentUser.TryApplyArgumentTo(collectible);

            if (argumentWorked)
            {
                return;
            }
        }

        collectible.TryCollect(targetPointBank, gameObject);
    }

    private MorphPointCollectible FindNearestAvailableCollectible()
    {
        MorphPointCollectible[] collectibles =
            FindObjectsByType<MorphPointCollectible>(FindObjectsInactive.Exclude);

        MorphPointCollectible bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (MorphPointCollectible collectible in collectibles)
        {
            if (collectible == null) continue;
            if (!collectible.IsAvailable) continue;

            float horizontalDistance = Mathf.Abs(transform.position.x - collectible.transform.position.x);

            if (horizontalDistance > collectRadius) continue;

            if (horizontalDistance < bestDistance)
            {
                bestDistance = horizontalDistance;
                bestTarget = collectible;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}