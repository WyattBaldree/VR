using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public Camera billboardCam;

    [ExecuteInEditMode]
    void Update()
    {
        transform.LookAt(billboardCam.transform.position, -Vector3.up);
    }
}
