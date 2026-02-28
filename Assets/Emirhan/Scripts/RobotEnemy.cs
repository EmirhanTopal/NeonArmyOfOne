using System;
using UnityEngine;

public class RobotEnemy : Enemy
{
    [SerializeField] private ParticleSystem explosionParticleSystem;
    [SerializeField] private ParticleSystem hitRobotParticleSystem;
    private string _playerTag = "Player";
    private Collider[] _results = new Collider[10];
    public static Action<int> OnHit;
    private int _damageToPlayer = 25;
    
    private void Awake()
    {
        health = 100;
    }

    private void Update()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, 1.5f, _results);
        for (int i = 0; i < count; i++)
        {
            if (_results[i].gameObject.CompareTag(_playerTag) && _results[i].gameObject.GetComponent<PlayerHealth>() != null)
            {
                Die();
                OnHit?.Invoke(_damageToPlayer);
                return;
            }
        }
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
