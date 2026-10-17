using UnityEngine;
using System;

[Serializable]
public struct MorphInfluence
{
    public MorphPointType type;

    [Min(0)]
    public int amount;

    public MorphInfluence(MorphPointType type, int amount)
    {
        this.type = type;
        this.amount = Mathf.Max(0, amount);
    }

    public bool IsActive => amount > 0;
}
