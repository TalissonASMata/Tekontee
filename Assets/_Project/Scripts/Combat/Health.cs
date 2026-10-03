using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    public bool IsDead => currentHP <= 0;

    public event Action<Health> OnDeath;
    public event Action<Health, int, int> OnHealthChanged;
    public event Action<Health, int, Transform> OnDamaged;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(this, currentHP, maxHP);
    }

    public void TakeDamage(int amount, Transform attacker = null)
    {
        if (IsDead) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log($"{gameObject.name} tomou {amount} de dano. HP atual: {currentHP}");

        OnDamaged?.Invoke(this, amount, attacker);
        OnHealthChanged?.Invoke(this, currentHP, maxHP);

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