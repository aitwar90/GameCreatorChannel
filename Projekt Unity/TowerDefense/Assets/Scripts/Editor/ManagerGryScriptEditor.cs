using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ManagerGryScript))]
public class ManagerGryScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ManagerGryScript myTarget = (ManagerGryScript)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Kasuj zapis"))
        {
            myTarget.KasujZapis();
        }
    }
}
