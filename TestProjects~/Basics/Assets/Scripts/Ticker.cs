using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnGUI()
    {
        GUILayout.Space(80);
        GUILayout.Label($".....Time {(int)Time.realtimeSinceStartup}");
    }
}
