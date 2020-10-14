using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class VRController : MonoBehaviour
{
    private Collider collider;

    public SteamVR_Action_Boolean spawn = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    SteamVR_Behaviour_Pose trackedObj;

    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private VRObject grippedObject = null;

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_Behaviour_Pose>();
    }

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

    private void FixedUpdate()
    {
        if (spawn.GetStateDown(trackedObj.inputSource))
        {
            Grip();
        }
        else if (spawn.GetStateUp(trackedObj.inputSource))
        {
            Release();
        }
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
