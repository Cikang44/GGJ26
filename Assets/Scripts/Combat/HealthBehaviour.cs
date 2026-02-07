using UnityEngine;
using UnityEngine.Events;

public class HealthBehaviour : MonoBehaviour
{
    public int health = 1;
    public UnityEvent<int> OnHealed = new();
    public UnityEvent<int> OnDamaged = new();
    public UnityEvent OnDeath = new();
    public void Heal(int healed)
    {
        health += healed;
        OnHealed.Invoke(healed);
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        OnDamaged.Invoke(damage);
        if (health <= 0) OnDeath.Invoke();
    }
}