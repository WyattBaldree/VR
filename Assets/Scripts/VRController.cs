using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Valve.VR;

public class VRController : MonoBehaviour
{
    public SteamVR_Action_Boolean spawn = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    public Rigidbody grabAttachPoint;
    public SteamVR_Behaviour_Pose trackedObj;

    private FixedJoint grabJoint;
    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private List<GrabPoint> GrabPointCollidingList = new List<GrabPoint>();

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        Collider grabCollider = gameObject.GetComponent<Collider>();
        Assert.IsNotNull(grabCollider);

        Assert.IsNotNull(trackedObj);

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
                    objectToGrip.transform.position = grabAttachPoint.transform.position;
                    objectToGrip.transform.rotation = grabAttachPoint.transform.rotation;
                    objectToGrip.transform.Rotate(grabbedGrabPoint.rotationOffset);

                    Vector3 grabPointPositionOffset = grabAttachPoint.transform.position - grabbedGrabPoint.transform.position;
                    objectToGrip.transform.position += grabPointPositionOffset + grabbedGrabPoint.postitionOffset;
                }

                grabJoint = objectToGrip.gameObject.AddComponent<FixedJoint>();
                /*grabJoint.xMotion = ConfigurableJointMotion.Limited;
                grabJoint.yMotion = ConfigurableJointMotion.Limited;
                grabJoint.zMotion = ConfigurableJointMotion.Limited;
                grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularZMotion = ConfigurableJointMotion.Locked;

                SoftJointLimit sjl = new SoftJointLimit();
                sjl.limit = 0.00001f;

                grabJoint.linearLimit = sjl;

                SoftJointLimitSpring sjls = new SoftJointLimitSpring();
                sjls.spring = 100000.0f;

                grabJoint.linearLimitSpring = sjls;

                grabJoint.anchor = new Vector3(0, 0, 0);
                grabJoint.axis = new Vector3(1, 0, 0);
                grabJoint.connectedAnchor = new Vector3(0, 0, 0);
                grabJoint.secondaryAxis = new Vector3(1, 0, 0);
                //grabJoint.autoConfigureConnectedAnchor = false;*/

                grabJoint.enableCollision = false;
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
        if(!vrObj) vrObj = other.gameObject.GetComponentInParent<VRObject>();
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
        if (!vrObj) vrObj = other.gameObject.GetComponentInParent<VRObject>();
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
