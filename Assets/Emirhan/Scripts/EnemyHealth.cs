using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamagable
{
    protected int health;

    public abstract void TakeDamage(int damage);

    public abstract void Die();
}
