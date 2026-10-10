using UnityEngine;
using UnityEngine.InputSystem;

public class MorphPointDebugTester : MonoBehaviour
{
    public MorphPointBank pointBank;
    public bool enableKeyboardTest = true;
    public int amountPerKey = 1;

    private void Awake()
    {
        if (pointBank == null)
        {
            pointBank = GetComponent<MorphPointBank>();
        }
    }

    private void Update()
    {
        if (!enableKeyboardTest) return;
        if (pointBank == null) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            pointBank.AddPoints(MorphPointType.Attack, amountPerKey);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            pointBank.AddPoints(MorphPointType.Defense, amountPerKey);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            pointBank.AddPoints(MorphPointType.Air, amountPerKey);
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            pointBank.AddPoints(MorphPointType.Energy, amountPerKey);
        }
    }
}