using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        RobotEnemy.OnHit += TakeDamage;
    }

    private void OnDisable()
    {
        RobotEnemy.OnHit -= TakeDamage;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Debug.Log("You Dead");
        }
    }
}
