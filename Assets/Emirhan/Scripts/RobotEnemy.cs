using System;
using UnityEngine;

public class RobotEnemy : Enemy
{
    [SerializeField] private ParticleSystem explosionParticleSystem;
    [SerializeField] private ParticleSystem hitRobotParticleSystem;
    
    private void Awake()
    {
        health = 100;
    }
    
    public override void TakeDamage(int damage)
    {
        ParticleSystem hit = Instantiate(hitRobotParticleSystem, transform.position, Quaternion.identity);
        hit.Play();
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
        ParticleSystem exp = Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
        exp.Play();
        Destroy(gameObject);
    }
}
