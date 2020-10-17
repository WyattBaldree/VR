using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabPoint : MonoBehaviour
{
    public Text debugTextBox; 
    private void OnTriggerEnter(Collider other)
    {
        debugTextBox.text = (int.Parse(debugTextBox.text) + 1).ToString();
    }

    private void OnTriggerExit(Collider other)
    {
        debugTextBox.text = (int.Parse(debugTextBox.text) - 1).ToString();
    }
}
