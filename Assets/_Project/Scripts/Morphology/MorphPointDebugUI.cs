using UnityEngine;
using TMPro;

public class MorphPointDebugUI : MonoBehaviour
{
    public MorphPointBank pointBank;
    public TMP_Text text;

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged += UpdateText;
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (pointBank != null)
        {
            pointBank.OnPointsChanged -= UpdateText;
        }
    }

    private void UpdateText(MorphPointBank bank)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (text == null || pointBank == null) return;

        text.text = pointBank.GetDebugText();
    }
}