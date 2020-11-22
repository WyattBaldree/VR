using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ActivateSteamVRComponent : MonoBehaviour
{
    public SteamVR_CameraHelper targetSteamVR_CameraHelper;

    private bool frameWaited = false;

    private void Update()
    {
        if (!frameWaited)
        {
            frameWaited = true;
        }
        else
        {
            targetSteamVR_CameraHelper.enabled = true;
        }
    }
}
