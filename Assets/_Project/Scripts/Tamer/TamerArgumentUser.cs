using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class TamerArgumentUser : MonoBehaviour
{
    [Header("Estado")]
    public bool canUseArguments = true;

    [Header("Argumento equipado")]
    public TamerArgumentType equippedArgument = TamerArgumentType.Intensify;

    [Header("Destino dos pontos")]
    public MorphPointBank targetPointBank;

    [Header("Ativação")]
    public Key argumentKey = Key.Q;
    public float activeDuration = 3f;

    private float argumentActiveUntil;

    public bool IsArgumentActive
    {
        get
        {
            return Time.time <= argumentActiveUntil;
        }
    }

    public float RemainingActiveTime
    {
        get
        {
            return Mathf.Max(0f, argumentActiveUntil - Time.time);
        }
    }

    public TamerArgumentType EquippedArgument => equippedArgument;

    private void Update()
    {
        if (!canUseArguments) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[argumentKey].wasPressedThisFrame)
        {
            ActivateArgument();
        }
    }

    private void ActivateArgument()
    {
        argumentActiveUntil = Time.time + activeDuration;

        Debug.Log(
            $"{gameObject.name} ativou Argumento {equippedArgument} por {activeDuration} segundo(s)."
        );
    }

    public bool TryApplyArgumentTo(MorphPointCollectible collectible)
    {
        if (!canUseArguments) return false;
        if (!IsArgumentActive) return false;
        if (collectible == null) return false;
        if (targetPointBank == null) return false;

        bool success = collectible.TryUseArgument(targetPointBank, gameObject, equippedArgument);

        if (success)
        {
            argumentActiveUntil = 0f;

            Debug.Log($"{gameObject.name} consumiu o Argumento {equippedArgument}.");
        }

        return success;
    }
}