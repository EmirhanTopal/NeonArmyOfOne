using System;
using UnityEngine;

public class RobotEnemy : Enemy
{
    private void Awake()
    {
        health = 100;
    }
    
    public override void TakeDamage(int damage)
    {
        if (health - damage > 0)
        {
            health -= damage;
        }
        else
        {
            Die();
        }
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
