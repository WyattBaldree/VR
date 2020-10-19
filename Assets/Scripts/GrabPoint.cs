using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabPoint : MonoBehaviour
{
    static public float gizmoLineScale = 0.3f;

    public Text debugTextBox;

    private VRObject parentVRObject;

    public Vector3 postitionOffset;
    public Vector3 rotationOffset;

    public void SetParentVRObject(VRObject newParent)
    {
        parentVRObject = newParent;
    }

    public VRObject GetParentVRObject()
    {
        return parentVRObject;
    }
    private void OnTriggerEnter(Collider other)
    {
        debugTextBox.text = (int.Parse(debugTextBox.text) + 1).ToString();
    }

    private void OnTriggerExit(Collider other)
    {
        debugTextBox.text = (int.Parse(debugTextBox.text) - 1).ToString();
    }

    private void OnDrawGizmos()
    {
        Vector3 lineStart = transform.position + postitionOffset;

        Quaternion rotation = Quaternion.Euler(rotationOffset);
        Vector3 targetForward = rotation * Vector3.forward;

        Gizmos.DrawLine(lineStart, lineStart + targetForward * gizmoLineScale);
    }
}
