using UnityEngine;

public class ArenaBounds2D : MonoBehaviour
{
    [Header("Limites X")]
    public bool clampX = true;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Limites Y")]
    public bool clampY = true;
    public float minY = -3f;
    public float maxY = 3f;

    private void LateUpdate()
    {
        Vector3 position = transform.position;

        if (clampX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (clampY)
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        transform.position = position;
    }
}