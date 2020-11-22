using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRotationOnAxis : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 axisToCopy;

    void Update()
    {
        transform.eulerAngles = Vector3.Scale(objectToFollow.eulerAngles, axisToCopy);
    }
}
