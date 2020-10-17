using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRObject : MonoBehaviour
{
    public int priority = 5;
    public List<GameObject> grabPointList = new List<GameObject>();

    private Collider collider;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
        if (!collider)
        {
            collider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        }

        rigidBody = GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            rigidBody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        }

        gameObject.layer = 8;
    }

    public void Gripped()
    {
        Debug.Log(gameObject.name + " was gripped!");
    }

    public void Released()
    {
        Debug.Log(gameObject.name + " was released!");
    }
}
