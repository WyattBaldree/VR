using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CorrectChestRotation : MonoBehaviour
{
    public Transform chest;
    public new Transform camera;

    private void SetRotation()
    {
        chest.transform.eulerAngles = Vector3.Scale(camera.eulerAngles, new Vector3(0, 1, 0));
    }

    private void Update()
    {
        if (Math.Abs(chest.transform.eulerAngles.y - camera.transform.eulerAngles.y) > 160)
            SetRotation();
    }
}
