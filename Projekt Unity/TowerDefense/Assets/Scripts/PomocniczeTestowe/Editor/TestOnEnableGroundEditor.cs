using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestOnEnableGround))]
public class TestOnEnableGroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TestOnEnableGround myarget = (TestOnEnableGround)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Sprawdź czy wieże są"))
        {
            myarget.PobierzInformacje();
        }
    }
}
