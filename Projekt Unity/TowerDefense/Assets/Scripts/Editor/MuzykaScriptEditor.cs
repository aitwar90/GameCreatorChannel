using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MuzykaScript))]
public class MuzykaScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MuzykaScript myTarget = (MuzykaScript)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Sortuj tablice po nazwie"))
        {
            myTarget.SortujAlfabetyczniePoNazwie();
        }
    }
}
