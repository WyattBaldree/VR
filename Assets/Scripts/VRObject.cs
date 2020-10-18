using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRObject : MonoBehaviour
{
    public int priority = 5;
    public GrabPoint[] grabPointList = { };

    private Collider myCollider;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider>();
        if (!myCollider)
        {
            myCollider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        }

        rigidBody = GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            rigidBody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        }

        gameObject.layer = 8;

        grabPointList = gameObject.GetComponentsInChildren<GrabPoint>();
        foreach(GrabPoint gp in grabPointList)
        {
            gp.SetParentVRObject(this);
        }
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
