using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dude : MonoBehaviour
{
    [SerializeField] private GameObject ragdoll;
    [SerializeField] private GameObject animatedModel;
    [SerializeField] private NavMeshAgent navmeshAgent;

    private bool dead;

    private void Awake()
    {
        ragdoll.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ToggleDead();
        }
    }

    [ContextMenu("ToggleDead")]
    private void ToggleDead()
    {
        dead = !dead;
        if (dead)
        {
            //switch to the ragdoll.
            CopyTransformData(animatedModel.transform, ragdoll.transform, navmeshAgent.velocity);
            ragdoll.gameObject.SetActive(true);
            animatedModel.gameObject.SetActive(false);
            navmeshAgent.enabled = false;
        }
        else
        {
            //switch back to the model and disable the ragdoll.
            ragdoll.gameObject.SetActive(false);
            animatedModel.gameObject.SetActive(true);
            navmeshAgent.enabled = true;
        }
    }

    private void CopyTransformData(Transform sourceTransform, Transform destinationTransform, Vector3 velocity)
    {
        if(sourceTransform.childCount != destinationTransform.childCount)
        {
            Debug.LogWarning("CopyTranformData requires that both transforms have the same hierarchy.");
            return;
        }

        for (int i = 0; i < sourceTransform.childCount; i++)
        {
            var source = sourceTransform.GetChild(i);
            var destination = destinationTransform.GetChild(i);
            destination.position = source.position;
            destination.rotation = source.rotation;

            var rb = destination.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.velocity = velocity;
            }

            CopyTransformData(source, destination, velocity);
        }
    }
}
