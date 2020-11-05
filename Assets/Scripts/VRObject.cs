using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VRObject : MonoBehaviour
{
    public int priority = 5;
    public GrabPoint[] grabPointList = { };

    //contains a refernce to each of the controllers grabbing this object.
    [NonSerialized]
    public List<VRController> grabList = new List<VRController>();

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

    public void Gripped(VRController gripper)
    {
        grabList.Add(gripper);
        Debug.Log(gameObject.name + " was gripped!");
    }

    public void Released(VRController releaser)
    {
        grabList.Remove(releaser);

        if(grabList.Count > 0)
        {
            bool wasAngularController = true;
            foreach (VRController vrc in grabList)
            {
                if (vrc.hasAngularDrive == true)
                {
                    wasAngularController = false;
                    break;
                }
            }

            if (wasAngularController)
            {
                grabList[0].RestoreAngularDrive();
            }
        }
        Debug.Log(gameObject.name + " was released!");
    }
}
