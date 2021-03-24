using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UstawTenDomyslnyButton : MonoBehaviour
{
    protected GameObject[] actILast = new GameObject[2];
    public GameObject domyślnyGO;
    public void UstawTenAktywny(GameObject ustawianyButtonNaDomyslny)
    {
        if (actILast[0] == null)
        {
            actILast[0] = ustawianyButtonNaDomyslny;
        }
        else
        {
            actILast[1] = actILast[0];
            actILast[0] = ustawianyButtonNaDomyslny;
        }
        PomocniczeFunkcje.eSystem.SetSelectedGameObject(actILast[0]);
    }
    public void UstawPoprzedni(GameObject orginalnyObiekt = null)
    {
        if (actILast[1] != null)
            UstawTenAktywny(actILast[1]);
        else
        {
            if (orginalnyObiekt != null) UstawTenAktywny(orginalnyObiekt);
        }
    }
    public void UstawOstatni(GameObject orginalnyObiekt = null)
    {
        if (actILast[0] != null) UstawTenAktywny(actILast[0]);
        else UstawPoprzedni(orginalnyObiekt);
    }
    void LateUpdate()
    {
        if (this.gameObject.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                if (PomocniczeFunkcje.eSystem.currentSelectedGameObject == null)
                {
                    UstawOstatni(domyślnyGO);
                }
            }
        }
    }
}
