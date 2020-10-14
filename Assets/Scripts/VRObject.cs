using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRObject : MonoBehaviour
{
    public int priority = 5;

    private Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = gameObject.GetComponent<Collider>();
        if (!collider)
        {
            collider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
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
