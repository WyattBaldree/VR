using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Valve.VR;

public class VRController : MonoBehaviour
{
    public SteamVR_Action_Boolean spawn = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    public Rigidbody grabAttachPoint;
    public ConfigurableJoint controllerJoint;
    public SteamVR_Behaviour_Pose trackedObj;
    public OrientationEnum orientation = OrientationEnum.right;

    [NonSerialized]
    public bool hasAngularDrive = true;

    private ConfigurableJoint grabJoint;
    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private List<GrabPoint> GrabPointCollidingList = new List<GrabPoint>();

    private Vector3 defaultDriveSpring;
    private Vector2 defaultAngularDriveSpring;
    private Vector2 defaultAngularDriveMaxForce;

    private bool setup = false;

    public enum OrientationEnum { 
        right,
        left
    }

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

        defaultDriveSpring = new Vector3(controllerJoint.xDrive.positionSpring, controllerJoint.yDrive.positionSpring, controllerJoint.zDrive.positionSpring);
        defaultAngularDriveSpring = new Vector2(controllerJoint.angularXDrive.positionSpring, controllerJoint.angularYZDrive.positionSpring);
        defaultAngularDriveMaxForce = new Vector2(controllerJoint.angularXDrive.maximumForce, controllerJoint.angularYZDrive.maximumForce);
    }

    private void FixedUpdate()
    {
        if (!setup)
        {
            grabAttachPoint.transform.position = trackedObj.transform.position;
            grabAttachPoint.velocity = new Vector3(0, 0, 0);
            grabAttachPoint.angularVelocity = new Vector3(0, 0, 0);
            setup = true;
        }

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
                
                if (grabbedGrabPoint)
                {
                    /*objectToGrip.transform.rotation = grabAttachPoint.transform.rotation;
                    Vector3 grabPointRotationOffset = grabAttachPoint.transform.eulerAngles - grabbedGrabPoint.transform.eulerAngles;
                    objectToGrip.transform.eulerAngles += grabPointRotationOffset + grabbedGrabPoint.rotationOffset;
                    
                    objectToGrip.transform.position = grabAttachPoint.transform.position;
                    Vector3 grabPointPositionOffset = grabAttachPoint.transform.position - grabbedGrabPoint.transform.position;
                    objectToGrip.transform.position += grabPointPositionOffset + grabbedGrabPoint.postitionOffset;
                    Physics.IgnoreCollision(grabAttachPoint.GetComponent<Collider>(), objectToGrip.GetComponent<Collider>());*/

                    grabAttachPoint.transform.rotation = objectToGrip.transform.rotation;
                    Vector3 grabPointRotationOffset =  grabbedGrabPoint.transform.eulerAngles - grabAttachPoint.transform.eulerAngles;
                    grabAttachPoint.transform.eulerAngles += grabPointRotationOffset + grabbedGrabPoint.rotationOffset;

                    grabAttachPoint.transform.position = objectToGrip.transform.position;
                    Vector3 grabPointPositionOffset = grabbedGrabPoint.transform.position - grabAttachPoint.transform.position;
                    grabAttachPoint.transform.position += grabPointPositionOffset + grabbedGrabPoint.postitionOffset;
                    Physics.IgnoreCollision(grabAttachPoint.GetComponent<Collider>(), objectToGrip.GetComponent<Collider>());

                    //if (orientation == OrientationEnum.right) objectToGrip.SetLayerOfChildren(11);
                    //else objectToGrip.SetLayerOfChildren(12);
                }

                grabJoint = objectToGrip.gameObject.AddComponent<ConfigurableJoint>();

                grabJoint.xMotion = ConfigurableJointMotion.Locked;
                grabJoint.yMotion = ConfigurableJointMotion.Locked;
                grabJoint.zMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularZMotion = ConfigurableJointMotion.Locked;
                //grabJoint.projectionMode = JointProjectionMode.PositionAndRotation;

                if (objectToGrip.grabList.Count > 0)
                {
                    RemoveAngularDrive();
                }


                /*SoftJointLimit sjl = new SoftJointLimit();
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


                objectToGrip.Gripped(this);
            }
        }
    }

    private void Release()
    {
        if (grabJoint != null)
        {
            VRObject vro = grabJoint.gameObject.GetComponent<VRObject>();
            Physics.IgnoreCollision(grabAttachPoint.GetComponent<Collider>(), vro.GetComponent<Collider>(), false);
            //vro.SetLayerOfChildren(8);
            vro.Released(this);

            Rigidbody rb = grabJoint.gameObject.GetComponent<Rigidbody>();
            DestroyImmediate(grabJoint);
            grabJoint = null;

            RestoreAngularDrive();
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

    public void RemoveAngularDrive()
    {
        hasAngularDrive = false;

        JointDrive jd = new JointDrive();
        jd.positionSpring = 0;
        jd.positionDamper = 0;
        jd.maximumForce = 0;

        controllerJoint.angularXDrive = jd;
        controllerJoint.angularYZDrive = jd;
    }

    public void RestoreAngularDrive()
    {
        hasAngularDrive = true;

        JointDrive jd = new JointDrive();
        jd.positionSpring = defaultAngularDriveSpring.x;
        jd.positionDamper = 0;
        jd.maximumForce = defaultAngularDriveMaxForce.x;

        controllerJoint.angularXDrive = jd;

        jd = new JointDrive();
        jd.positionSpring = defaultAngularDriveSpring.y;
        jd.positionDamper = 0;
        jd.maximumForce = defaultAngularDriveMaxForce.y;

        controllerJoint.angularYZDrive = jd;
    }
}
