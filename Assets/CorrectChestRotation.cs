using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CorrectChestRotation : MonoBehaviour
{
    public Transform chest;
    public Transform camera;

    private void OnEnable()
    {
        SetRotation();
        //SteamVR_Events.Initialized.Listen(OnIntialized);
        StartCoroutine(CorrectRotationAfterDelay(1f));
    }

    private void OnIntialized(bool initialized)
    {
        Debug.Log("Initialized!");
    }

    private void SetRotation()
    {
        transform.eulerAngles = Vector3.Scale(camera.eulerAngles, new Vector3(0, 1, 0));
    }

    IEnumerator CorrectRotationAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);

        SetRotation();
    }
}
