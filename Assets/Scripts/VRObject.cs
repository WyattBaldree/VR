using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VRObject : MonoBehaviour
{
    public int priority = 5;
    public GrabPoint[] grabPointList = { };

    // Start is called before the first frame update
    void Start()
    {
        Collider myCollider = GetComponent<Collider>();
        if(!myCollider) myCollider = GetComponentInChildren<Collider>();
        Assert.IsNotNull(myCollider, "This VR Object and it's children do not have a collider. At least 1 required.");

        Rigidbody rigidBody = GetComponent<Rigidbody>();
        Assert.IsNotNull(rigidBody, "This VR Object does not have a rigid body.");

        gameObject.layer = 8;

        grabPointList = gameObject.GetComponentsInChildren<GrabPoint>();
        foreach(GrabPoint gp in grabPointList)
        {
            gp.SetParentVRObject(this);
        }
    }

    public void SetLayerOfChildren(int newLayer) 
    {
        gameObject.layer = newLayer;
        foreach(Transform t in GetComponentInChildren<Transform>())
        {
            t.gameObject.layer = newLayer;
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
