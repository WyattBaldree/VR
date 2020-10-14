using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRController : MonoBehaviour
{
    private Collider collider;

    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private VRObject grippedObject = null;

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

    private void Grip()
    {
        VRObject objectToGrip = null;
        int highestPriority = int.MinValue;
        foreach(VRObject vrObj in VRObjectCollidingList)
        {
            if(vrObj.priority > highestPriority)
            {
                objectToGrip = vrObj;
                highestPriority = vrObj.priority;
            }
        }

        if (objectToGrip)
        {
            objectToGrip.Gripped();
            grippedObject = objectToGrip;
            grippedObject.transform.parent = transform;
        }
    }

    private void Release()
    {
        if (grippedObject)
        {
            grippedObject.Released();
            grippedObject = null;
            grippedObject.transform.parent = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter: " + collision.gameObject.name);
        VRObject vrObj = collision.gameObject.GetComponent<VRObject>();
        if(vrObj) VRObjectCollidingList.Add(vrObj);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit: " + collision.gameObject.name);
        VRObject vrObj = collision.gameObject.GetComponent<VRObject>();
        if (vrObj) VRObjectCollidingList.Remove(vrObj);
    }
}
