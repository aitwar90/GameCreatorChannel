using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnBudynki))]
public class SpawnBudynkiEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpawnBudynki myTarget = (SpawnBudynki)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Załaduj prefaby"))
        {
            myTarget.ZaladujWszystkieKonkrenyDoTablicy();
        }
        if(GUILayout.Button("Sortuj po poziomie"))
        {
            myTarget.SortujPoEpoceIPoziomie();
        }

    }
}
