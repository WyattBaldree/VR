using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class RandomRoam : MonoBehaviour
{
    private NavMeshAgent navmeshAgent;

    private void Awake()
    {
        navmeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(navmeshAgent.enabled == false)
        {
            return;
        }
        if (navmeshAgent.hasPath == false || navmeshAgent.remainingDistance < 1f)
        {
            ChooseNewPosition();
        }
    }

    private void ChooseNewPosition()
    {
        Vector3 randomOffset = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        var destination = transform.position + randomOffset;
        navmeshAgent.SetDestination(destination);
    }
}
