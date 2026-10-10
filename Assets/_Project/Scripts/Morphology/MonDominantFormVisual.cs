using UnityEngine;

[DisallowMultipleComponent]
public class MonDominantFormVisual : MonoBehaviour
{
    [Header("Referências")]
    public MorphPointBank pointBank;
    public SpriteRenderer formRenderer;
    public Transform formTransform;

    [Header("Comportamento")]
    public bool hideWhenNoPoints = true;

    [Header("Cores das formas")]
    public Color attackColor = new Color(1f, 0f, 0.75f, 1f);
    public Color defenseColor = Color.white;
    public Color airColor = Color.yellow;
    public Color energyColor = Color.cyan;

    [Header("Escalas das formas")]
    public Vector3 attackScale = new Vector3(1.35f, 0.45f, 1f);
    public Vector3 defenseScale = new Vector3(0.95f, 0.95f, 1f);
    public Vector3 airScale = new Vector3(0.45f, 1.35f, 1f);
    public Vector3 energyScale = new Vector3(1.15f, 1.15f, 1f);

    private bool hasAppliedForm;
    private MorphPointType lastAppliedType;

    private void Awake()
    {
        if (pointBank == null)
        {
            pointBank = GetComponent<MorphPointBank>();
        }

        if (formRenderer != null && formTransform == null)
        {
            formTransform = formRenderer.transform;
        }
    }

    private void OnEnable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged += HandlePointsChanged;
        }
    }

    private void OnDisable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged -= HandlePointsChanged;
        }
    }

    private void Start()
    {
        ApplyCurrentForm(true);
    }

    private void HandlePointsChanged(MorphPointBank bank)
    {
        ApplyCurrentForm(false);
    }

    private void ApplyCurrentForm(bool force)
    {
        if (pointBank == null) return;
        if (formRenderer == null) return;

        if (pointBank.TotalPoints <= 0)
        {
            if (hideWhenNoPoints)
            {
                formRenderer.enabled = false;
            }

            hasAppliedForm = false;
            return;
        }

        MorphPointType dominantType = pointBank.DominantType;

        if (!force && hasAppliedForm && dominantType == lastAppliedType)
        {
            return;
        }

        formRenderer.enabled = true;

        switch (dominantType)
        {
            case MorphPointType.Attack:
                ApplyVisual(attackColor, attackScale);
                break;

            case MorphPointType.Defense:
                ApplyVisual(defenseColor, defenseScale);
                break;

            case MorphPointType.Air:
                ApplyVisual(airColor, airScale);
                break;

            case MorphPointType.Energy:
                ApplyVisual(energyColor, energyScale);
                break;
        }

        lastAppliedType = dominantType;
        hasAppliedForm = true;

        Debug.Log($"{gameObject.name} mudou forma provisória para: {dominantType}");
    }

    private void ApplyVisual(Color color, Vector3 scale)
    {
        formRenderer.color = color;

        if (formTransform != null)
        {
            formTransform.localScale = scale;
        }
    }
}