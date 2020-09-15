using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KonkretnyNPCDynamiczny))]
public class KonkretnyNPCDynamicznyEditor : Editor
{
   public override void OnInspectorGUI()
    {
        KonkretnyNPCDynamiczny myTarget = (KonkretnyNPCDynamiczny)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Znajdź najbliższy obiekt"))
        {
            myTarget.WyszukajNajbliższyObiekt();
        }
    }
}
