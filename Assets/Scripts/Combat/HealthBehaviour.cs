using UnityEngine;
using UnityEngine.Events;

public class HealthBehaviour : MonoBehaviour
{
    public int health = 1;
    public UnityEvent OnHeal = new();
    public UnityEvent OnDeath = new();
    public void Heal(int healed)
    {
        health += healed;
        OnHeal.Invoke();
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) OnDeath.Invoke();
    }
}