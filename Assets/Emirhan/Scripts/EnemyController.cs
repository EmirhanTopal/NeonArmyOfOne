using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private FirstPersonController playerFirstController;
    private NavMeshAgent agent;

    private void Awake()
    {
        playerFirstController = FindFirstObjectByType<FirstPersonController>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        agent.SetDestination(playerFirstController.transform.position);
    }
}
