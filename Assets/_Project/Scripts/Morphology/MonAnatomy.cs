using System;
using UnityEngine;

// V1.7.2 - Dados anatomicos independentes da forma visual e do banco
// provisório Attack/Defense/Air/Energy da V1.6.
// "Revestimento" e uma caracteristica, nao uma estatistica numerica.
[Serializable]
public class MonAnatomicalRegion
{
    [Tooltip("Caracteristica da superficie; catalogo e efeitos ainda indefinidos.")]
    public string revestimento = "";
    [Min(0f)] public float massa;
    [Min(0f)] public float tenacidade;
}

[Serializable]
public class MonHeadAnatomy : MonAnatomicalRegion
{
    [Tooltip("Caracteristica de presa ou bico; regras futuras.")]
    public string presaOuBico = "";
    [Min(0f)] public float intelecto;
}

[Serializable]
public class MonTorsoAnatomy : MonAnatomicalRegion
{
    [Min(0f)] public float extensao;
    [Tooltip("Conceito de cobertura ainda pendente; nao equivale a Revestimento.")]
    public string cobertura = "";
}

[Serializable]
public class MonRepeatedLimbAnatomy : MonAnatomicalRegion
{
    [Min(0f)] public float extensao;
    [Min(0f)] public float arrasto;
    [Tooltip("Quantidade par de membros. Massa e tenacidade acima sao POR membro.")]
    [Min(0)] public int quantidade = 2;
}

[Serializable]
public class MonTailAnatomy : MonAnatomicalRegion
{
    [Min(0f)] public float extensao;
}

[Serializable]
public class MonElementAnatomy
{
    [Tooltip("Afinidade ou estado elemental; nenhum catalogo de elementos foi imposto.")]
    public string afinidadeOuEstado = "";
}

[DisallowMultipleComponent]
public class MonAnatomy : MonoBehaviour
{
    [Header("Cabeca")]
    public MonHeadAnatomy cabeca = new MonHeadAnatomy();

    [Header("Tronco")]
    public MonTorsoAnatomy tronco = new MonTorsoAnatomy();

    [Header("Bracos - atributos compartilhados por braco")]
    public MonRepeatedLimbAnatomy bracos = new MonRepeatedLimbAnatomy();

    [Header("Pernas - atributos compartilhados por perna")]
    public MonRepeatedLimbAnatomy pernas = new MonRepeatedLimbAnatomy();

    [Header("Cauda")]
    public MonTailAnatomy cauda = new MonTailAnatomy();

    [Header("Elemento")]
    public MonElementAnatomy elemento = new MonElementAnatomy();

    // Sem caches: os totais sempre refletem o estado anatomico atual.
    // Revestimentos categoricos NAO participam de somas globais.
    public float MassaGeral =>
        Safe(cabeca?.massa ?? 0f) +
        Safe(tronco?.massa ?? 0f) +
        Safe(bracos?.massa ?? 0f) * Count(bracos) +
        Safe(pernas?.massa ?? 0f) * Count(pernas) +
        Safe(cauda?.massa ?? 0f);

    public float TenacidadeGeral =>
        Safe(cabeca?.tenacidade ?? 0f) +
        Safe(tronco?.tenacidade ?? 0f) +
        Safe(bracos?.tenacidade ?? 0f) * Count(bracos) +
        Safe(pernas?.tenacidade ?? 0f) * Count(pernas) +
        Safe(cauda?.tenacidade ?? 0f);

    public event Action<MonAnatomy> OnAnatomyChanged;

    // Setters especificos para as primeiras fatias de teste:
    // pulo (pernas.massa/extensao + massa geral) e
    // alcance (bracos.extensao + tronco.extensao).
    // Apenas atualizam dados; nao mudam sprite, collider ou movimento.
    public void SetLegMass(float value)
    {
        if (pernas == null) return;
        SetValue(ref pernas.massa, value);
    }

    public void SetLegExtension(float value)
    {
        if (pernas == null) return;
        SetValue(ref pernas.extensao, value);
    }

    public void SetArmExtension(float value)
    {
        if (bracos == null) return;
        SetValue(ref bracos.extensao, value);
    }

    public void SetTorsoExtension(float value)
    {
        if (tronco == null) return;
        SetValue(ref tronco.extensao, value);
    }

    public bool SetArmQuantity(int value)
    {
        if (bracos == null || !ValidQuantity(value)) return false;
        if (bracos.quantidade == value) return true;
        bracos.quantidade = value;
        OnAnatomyChanged?.Invoke(this);
        return true;
    }

    public bool SetLegQuantity(int value)
    {
        if (pernas == null || !ValidQuantity(value)) return false;
        if (pernas.quantidade == value) return true;
        pernas.quantidade = value;
        OnAnatomyChanged?.Invoke(this);
        return true;
    }

    [ContextMenu("V1.7.2 - Mostrar resumo anatomico")]
    public void LogAnatomySummary()
    {
        Debug.Log(
            $"[V1.7.2] {name}: MassaGeral={MassaGeral:0.###}; " +
            $"TenacidadeGeral={TenacidadeGeral:0.###}; " +
            $"Bracos={Count(bracos)}; Pernas={Count(pernas)}; " +
            $"ExtensaoBracos={Safe(bracos?.extensao ?? 0f):0.###}; " +
            $"ExtensaoTronco={Safe(tronco?.extensao ?? 0f):0.###}; " +
            $"ExtensaoPernas={Safe(pernas?.extensao ?? 0f):0.###}."
        );
    }

    private void SetValue(ref float current, float next)
    {
        float sanitized = Safe(next);
        if (Mathf.Approximately(current, sanitized)) return;

        current = sanitized;
        OnAnatomyChanged?.Invoke(this);
    }

    private static bool ValidQuantity(int count) => count >= 0 && count % 2 == 0;
    private static int Count(MonRepeatedLimbAnatomy region) =>
        region == null ? 0 : Mathf.Max(0, region.quantidade);

    private static float Safe(float value) =>
        float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);

    private void OnValidate()
    {
        // Evita numeros negativos e quantidade impar inseridos pelo Inspector.
        // Nao dispara evento aqui: OnValidate tambem roda fora do Play Mode.
        if (cabeca != null)
        {
            cabeca.massa = Safe(cabeca.massa);
            cabeca.tenacidade = Safe(cabeca.tenacidade);
            cabeca.intelecto = Safe(cabeca.intelecto);
        }
        if (tronco != null)
        {
            tronco.massa = Safe(tronco.massa);
            tronco.tenacidade = Safe(tronco.tenacidade);
            tronco.extensao = Safe(tronco.extensao);
        }
        ValidateLimbs(bracos);
        ValidateLimbs(pernas);
        if (cauda != null)
        {
            cauda.massa = Safe(cauda.massa);
            cauda.tenacidade = Safe(cauda.tenacidade);
            cauda.extensao = Safe(cauda.extensao);
        }
    }

    private static void ValidateLimbs(MonRepeatedLimbAnatomy limb)
    {
        if (limb == null) return;
        limb.massa = Safe(limb.massa);
        limb.tenacidade = Safe(limb.tenacidade);
        limb.extensao = Safe(limb.extensao);
        limb.arrasto = Safe(limb.arrasto);
        limb.quantidade = Mathf.Max(0, limb.quantidade);
        if (limb.quantidade % 2 != 0) limb.quantidade += 1;
    }
}
