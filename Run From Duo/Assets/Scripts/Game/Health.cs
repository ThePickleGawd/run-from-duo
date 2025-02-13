using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
  [HideInInspector] public float health;
  public float maxHealth = 100f;

  public UnityEvent OnDeath;
  public UnityEvent OnTakeDamage;

  private void Awake()
  {
    ResetHealth();
  }

  public void TakeDamage(float damage)
  {
    health -= damage;
    OnTakeDamage?.Invoke();

    if (health <= 0) Die();
  }

  public void ResetHealth()
  {
    health = maxHealth;
  }

  public void Die()
  {
    OnDeath?.Invoke();
  }
}
