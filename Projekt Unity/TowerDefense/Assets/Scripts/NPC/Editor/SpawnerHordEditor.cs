using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnerHord))]
public class SpawnerHordEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        SpawnerHord myTarget = (SpawnerHord)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Wygeneruj kolejną falę"))
        {
            myTarget.GenerujSpawn(Epoki.EpokaKamienia);
        }
        if(GUILayout.Button("Odpal resetuj ścieżki"))
        {
            myTarget.WywołajResetujŚcieżki();
        }
        if(GUILayout.Button("Załaduj spawn punkty"))
        {
            myTarget.ŁadowanieTablicy();
        }
    }
}
