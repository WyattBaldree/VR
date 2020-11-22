using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetRotationOnStart : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 axisToCopy;

    void Start()
    {
        SetRotation();
    }
    private void Awake()
    {
        SetRotation();
    }

    private void OnEnable()
    {
        SetRotation();
    }

    private void SetRotation()
    {
        transform.eulerAngles = Vector3.Scale(objectToFollow.eulerAngles, axisToCopy);
    }

    public Texture btnTexture;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 50, 50), btnTexture))
            SetRotation();
    }
}
