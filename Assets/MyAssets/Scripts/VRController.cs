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
    public SteamVR_Action_Boolean trigger;
    public SteamVR_Action_Boolean grip;
    public Rigidbody grabAttachPoint;
    public ConfigurableJoint controllerJoint;
    public SteamVR_Behaviour_Pose trackedObj;
    public GameObject controllerGripPoint;

    [NonSerialized]
    public bool hasAngularDrive = true;

    private FixedJoint grabJoint;
    private List<VRObject> VRObjectCollidingList = new List<VRObject>();
    private List<GrabPoint> GrabPointCollidingList = new List<GrabPoint>();

    private Vector3 defaultDriveSpring;
    private Vector2 defaultAngularDriveSpring;
    private Vector2 defaultAngularDriveMaxForce;

    private bool setup = false;

    private VRObject grippedObject;

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
            grabAttachPoint.MovePosition(trackedObj.GetComponentInChildren<Rigidbody>().position);
            grabAttachPoint.velocity = new Vector3(0, 0, 0);
            grabAttachPoint.angularVelocity = new Vector3(0, 0, 0);
            setup = true;
        }

        RemoveOrbitVelocityComponent();

        if (grabJoint == null && grip.GetStateDown(trackedObj.inputSource))
        {
            Grip();
        }
        else if(grabJoint != null && grip.GetStateUp(trackedObj.inputSource))
        {
            Release();
        }

        if (trigger.GetStateDown(trackedObj.inputSource))
        {
            TriggerPressed();
            if (grippedObject)
            {
                grippedObject.TriggerPressed();
            }
        }
    }

    private void TriggerPressed()
    {
        throw new NotImplementedException();
    }

    private void OnDrawGizmos()
    {
        Vector3 toControllerVector = controllerGripPoint.transform.position - grabAttachPoint.transform.position;
        Gizmos.DrawLine(grabAttachPoint.transform.position, grabAttachPoint.transform.position + toControllerVector);
    }

    private void RemoveOrbitVelocityComponent()
    {
        //get dot product of hand velocity and the vector from the hand to the controller
        Vector3 handVelocity = grabAttachPoint.velocity;
        Vector3 handToControllerVector = trackedObj.transform.position - grabAttachPoint.transform.position;

        Plane orbitPlane  = new Plane(handToControllerVector / handToControllerVector.magnitude, grabAttachPoint.transform.position);

        Vector3 orbitVelocity = Vector3.ProjectOnPlane(handVelocity, handToControllerVector / handToControllerVector.magnitude);

        grabAttachPoint.velocity -= orbitVelocity;
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
                    grabAttachPoint.transform.rotation = objectToGrip.transform.rotation;
                    Vector3 grabPointRotationOffset =  grabbedGrabPoint.transform.eulerAngles - grabAttachPoint.transform.eulerAngles;
                    grabAttachPoint.transform.eulerAngles += grabPointRotationOffset + grabbedGrabPoint.rotationOffset;

                    grabAttachPoint.transform.position = objectToGrip.transform.position;
                    Vector3 grabPointPositionOffset = grabbedGrabPoint.transform.position - grabAttachPoint.transform.position;
                    grabAttachPoint.transform.position += grabPointPositionOffset + grabbedGrabPoint.postitionOffset;
                    Physics.IgnoreCollision(grabAttachPoint.GetComponent<Collider>(), objectToGrip.GetComponent<Collider>());
                }

                grabJoint = grabAttachPoint.gameObject.AddComponent<FixedJoint>();
                grabJoint.enablePreprocessing = false;


                if (objectToGrip.grabList.Count > 0)
                {
                    RemoveAngularDrive();
                }

                grabJoint.enableCollision = false;
                grabJoint.connectedBody = objectToGrip.GetComponent<Rigidbody>();


                objectToGrip.Gripped(this);
                grippedObject = objectToGrip;
            }
        }
    }

    private void Release()
    {
        if (grabJoint != null)
        {
            Physics.IgnoreCollision(grabAttachPoint.GetComponent<Collider>(), grippedObject.GetComponent<Collider>(), false);
            //vro.SetLayerOfChildren(8);
            grippedObject.Released(this);

            Rigidbody rb = grabJoint.gameObject.GetComponent<Rigidbody>();
            DestroyImmediate(grabJoint);
            grabJoint = null;
            grippedObject = null;

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
