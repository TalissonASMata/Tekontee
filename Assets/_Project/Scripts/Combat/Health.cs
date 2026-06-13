using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    public bool IsDead => currentHP <= 0;

    public event Action<Health> OnDeath;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log($"{gameObject.name} tomou {amount} de dano. HP atual: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} morreu.");
        OnDeath?.Invoke(this);
    }
}