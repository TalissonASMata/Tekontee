using System;
using UnityEngine;

// V1.8.10: classificacao minima de superficie para testar interacao
// entre o revestimento das pernas e a aderencia durante um impacto.
// Nao altera friccao fisica global, locomocao nem a layer do piso.
public enum MonGroundSurfaceKind
{
    Neutro,
    Rugoso,
    Liso
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class MonGroundSurface2D : MonoBehaviour
{
    [Header("V1.8.10 - Tipo de piso (teste)")]
    public MonGroundSurfaceKind surfaceKind = MonGroundSurfaceKind.Neutro;

    [Tooltip("Multiplicador de velocidade horizontal recebida no impacto para par compativel. Menor que 1 = mais aderencia.")]
    [Range(0.1f, 1f)] public float compatibleImpactMultiplier = 0.8f;

    [Tooltip("Multiplicador de velocidade horizontal recebida no impacto para par desfavoravel. Maior que 1 = menos aderencia.")]
    [Range(1f, 2f)] public float incompatibleImpactMultiplier = 1.2f;

    // Estes sao somente DOIS pares de prova: Garras/Rugoso e Ventosas/Liso.
    // Revestimento diferente ou terreno neutro NAO altera o knockback.
    // Nao converter qualquer revestimento em bonus universal.
    public float GetImpactResponseMultiplier(string legCovering, out string interaction)
    {
        interaction = "NEUTRA";
        if (surfaceKind == MonGroundSurfaceKind.Neutro)
            return 1f;

        string covering = (legCovering ?? string.Empty).Trim();
        bool claws = string.Equals(covering, "Garras", StringComparison.OrdinalIgnoreCase);
        bool suction = string.Equals(covering, "Ventosas", StringComparison.OrdinalIgnoreCase);
        if (!claws && !suction) return 1f;

        bool compatible =
            (claws && surfaceKind == MonGroundSurfaceKind.Rugoso) ||
            (suction && surfaceKind == MonGroundSurfaceKind.Liso);

        interaction = compatible ? "COMPATIVEL" : "DESFAVORAVEL";
        return compatible
            ? Mathf.Clamp(compatibleImpactMultiplier, 0.1f, 1f)
            : Mathf.Clamp(incompatibleImpactMultiplier, 1f, 2f);
    }
}
