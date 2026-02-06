using UnityEngine;
using UnityEngine.Events;

public class HealthBehaviour : MonoBehaviour
{
    public int health = 1;
    public UnityEvent OnHeal;
    public UnityEvent OnDeath;
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