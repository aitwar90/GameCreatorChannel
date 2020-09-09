using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PomocniczeFunkcje
{
    public static Vector3 OkreślPozycjęŚwiataKursora(Vector3 lastPos)
    {
        Ray ray;
        #if UNITY_STANDALONE
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        #endif
        #if UNITY_ANDROID
        ray = Camera.main.ScreenPointToRay(Input.GetTouch(0));
        #endif
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.GetType() == typeof(TerrainCollider))
            {
                return hit.point;
            }
        }
        return lastPos;
    }
}
