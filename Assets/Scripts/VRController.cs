using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class VRController : MonoBehaviour
{
    public SteamVR_Action_Boolean spawn = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    public Rigidbody grabAttachPoint;
    
    private FixedJoint grabJoint;
    private SteamVR_Behaviour_Pose trackedObj;
    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private List<GrabPoint> GrabPointCollidingList = new List<GrabPoint>();

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_Behaviour_Pose>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Collider grabCollider = gameObject.GetComponent<Collider>();
        if (!grabCollider)
        {
            Debug.LogWarning("The VR Controller does not have a collider. A default sphere collider has been added.", this);
            grabCollider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        }

        gameObject.layer = 8;
    }

    private void FixedUpdate()
    {
        if (grabJoint == null && spawn.GetStateDown(trackedObj.inputSource))
        {
            Grip();
        }
        else if(grabJoint != null && spawn.GetStateUp(trackedObj.inputSource))
        {
            Release();
        }
    }

    private void Grip()
    {
        if (grabJoint == null) 
        {



            //We try to grab GrabPoints before trying to grab VRObjects themselves.
            VRObject objectToGrip = null;
            GrabPoint grabbedGrabPoint = null;

            int highestPriority = int.MinValue;
            foreach (GrabPoint grabPoint in GrabPointCollidingList)
            {
                VRObject grabPointParent = grabPoint.GetParentVRObject();
                if (grabPointParent.priority + 1 > highestPriority)
                {
                    objectToGrip = grabPointParent;
                    highestPriority = grabPointParent.priority + 1;
                    grabbedGrabPoint = grabPoint;
                }
            }

            highestPriority = int.MinValue;
            foreach (VRObject vrObj in VRObjectCollidingList)
            {
                if (vrObj.priority > highestPriority)
                {
                    objectToGrip = vrObj;
                    highestPriority = vrObj.priority;
                    grabbedGrabPoint = null;
                }
            }

            if (objectToGrip)
            {

                Debug.Log("gripped", this);
                objectToGrip.Gripped();
                if (grabbedGrabPoint)
                {

                    Vector3 grabPointOffset = grabbedGrabPoint.transform.position - objectToGrip.transform.position;
                    objectToGrip.transform.position = grabAttachPoint.transform.position + grabPointOffset;
                }

                grabJoint = objectToGrip.gameObject.AddComponent<FixedJoint>();
                grabJoint.connectedBody = grabAttachPoint;
            }
        }
    }

    private void Release()
    {
        if (grabJoint != null)
        {
            grabJoint.gameObject.GetComponent<VRObject>().Released();
            Rigidbody rb = grabJoint.gameObject.GetComponent<Rigidbody>();
            Object.DestroyImmediate(grabJoint);
            grabJoint = null;
            
            Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
            if (origin != null)
            {
                rb.velocity = origin.TransformVector(trackedObj.GetVelocity());
                rb.angularVelocity = origin.TransformVector(trackedObj.GetAngularVelocity());
            }
            else
            {
                rb.velocity = trackedObj.GetVelocity();
                rb.angularVelocity = trackedObj.GetAngularVelocity();
            }

            rb.maxAngularVelocity = rb.angularVelocity.magnitude;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter: " + other.gameObject.name);
        VRObject vrObj = other.gameObject.GetComponent<VRObject>();
        if (vrObj)
        {
            VRObjectCollidingList.Add(vrObj);
        }

        GrabPoint grabPoint = other.gameObject.GetComponent<GrabPoint>();
        if (grabPoint)
        {
            GrabPointCollidingList.Add(grabPoint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit: " + other.gameObject.name);
        VRObject vrObj = other.gameObject.GetComponent<VRObject>();
        if (vrObj)
        {
            VRObjectCollidingList.Remove(vrObj);
        }

        GrabPoint grabPoint = other.gameObject.GetComponent<GrabPoint>();
        if (grabPoint)
        {
            GrabPointCollidingList.Remove(grabPoint);
        }
    }
}
