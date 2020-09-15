using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class PomocniczeFunkcje
{
    public static StrukturaDrzewa korzeńDrzewaPozycji = null;
    public static Transform celWrogów = null;
    private static string test = "Assets/Resources/Debug.txt";
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
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            if (hit.collider.GetType() == typeof(TerrainCollider))
            {
                return hit.point;
            }
        }
        return lastPos;
    }

    //Wyszukuje najbliższy budynek w drzewie dla punktu wysłanego w parametrze pozycjaWyszukiwujacego
    public static Component WyszukajWDrzewie(ref StrukturaDrzewa korzeń, Vector3 pozycjaWyszukiwującego)
    {
        if (korzeń == null)
        {
            Debug.Log("Korzeń jest null");
            return null;
        }
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
        float minDistance = 10000f;
        Component compDoZwrotu = korzeń.komponentGałęzi;
        while (true)
        {
            float deltaX = aktualnieSprawdzanyNode.pozycjaGałęzi.x - pozycjaWyszukiwującego.x;
            float deltaZ = aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z;
            float d = Vector3.Distance(aktualnieSprawdzanyNode.pozycjaGałęzi, pozycjaWyszukiwującego);
            if (d < minDistance)
            {
                minDistance = d;
                compDoZwrotu = aktualnieSprawdzanyNode.komponentGałęzi;
            }
            if (deltaX > 0)  //Lewa strona drzewa
            {
                if (deltaZ > 0)  //Dolna strona drzewa
                {
                    if (aktualnieSprawdzanyNode.MxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else    //Górna strona drzewa
                {
                    if (aktualnieSprawdzanyNode.MxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else    //Prawa strona drzewa
            {
                if (deltaZ > 0)
                {
                    if (aktualnieSprawdzanyNode.PxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanyNode.PxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return compDoZwrotu;
    }

    public static void SkasujElementDrzewa(ref StrukturaDrzewa korzeń, Component _komponentDoSkasowania)
    {
        if (korzeń == null)
        {
            Debug.Log("Korzeń jest null");
            return;
        }
        StrukturaDrzewa[,] ponownieUstaw = ZnajdźElementPoKomponencie(ref korzeń, _komponentDoSkasowania);
        if (ponownieUstaw == null)
        {
            Debug.Log("Nie odnalazłem elemenu drzewa do skasowania");
            return;
        }
        for (byte i = 0; i < 2; i++)
        {
            for (byte j = 0; j < 2; j++)
            {
                if (ponownieUstaw[i, j] != null)
                {
                    DodajDoDrzewaPozycji(ref korzeń, ponownieUstaw[i, j]);
                }
            }
        }
    }
    private static StrukturaDrzewa[,] ZnajdźElementPoKomponencie(ref StrukturaDrzewa korzeń, Component _com)
    {
        StrukturaDrzewa aktualnyElement = korzeń;
        Vector3 pos = _com.transform.position;
        StrukturaDrzewa prev = null;
        sbyte idx = -1; //0 +X+Z, 1 - +X-Z, 2 - -X+Z, 3 - -X-Z
        while (true)
        {
            if (_com == aktualnyElement.komponentGałęzi)
            {
                //Znalazłem :)
                StrukturaDrzewa[,] tabToReturn = { { null, null }, { null, null } };
                if (aktualnyElement.PxPz != null)
                {
                    tabToReturn[0, 0] = aktualnyElement.PxPz;
                }
                if (aktualnyElement.PxMz != null)
                {
                    tabToReturn[0, 1] = aktualnyElement.PxMz;
                }
                if (aktualnyElement.MxPz != null)
                {
                    tabToReturn[1, 0] = aktualnyElement.MxPz;
                }
                if (aktualnyElement.MxMz != null)
                {
                    tabToReturn[1, 1] = aktualnyElement.MxMz;
                }
                if (prev != null && idx > -1)
                {
                    switch (idx)
                    {
                        case 0:
                            prev.PxPz = null;
                            break;
                        case 1:
                            prev.PxMz = null;
                            break;
                        case 2:
                            prev.MxPz = null;
                            break;
                        case 3:
                            prev.MxMz = null;
                            break;

                    }
                }
                return tabToReturn;
            }
            if (pos.x < aktualnyElement.pozycjaGałęzi.x)
            {
                if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                {
                    if (aktualnyElement.MxMz != null)
                    {
                        idx = 3;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.MxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnyElement.MxPz != null)
                    {
                        idx = 2;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.MxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                {
                    if (aktualnyElement.PxMz != null)
                    {
                        idx = 1;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.PxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnyElement.PxPz != null)
                    {
                        idx = 0;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.PxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return null;
    }
    private static void DodajDoDrzewaPozycji(ref StrukturaDrzewa korzeń, StrukturaDrzewa elementDodawany)
    {
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
        Vector3 posKom = elementDodawany.pozycjaGałęzi;
        while (true)
        {
            if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
                //SaveInformationInDebug("If1 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.MxMz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                }
                else
                {
                    aktualnieSprawdzanyNode.MxMz = elementDodawany;
                    break;
                }
            }
            else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
                //SaveInformationInDebug("If2 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.PxMz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                }
                else
                {
                    aktualnieSprawdzanyNode.PxMz = elementDodawany;
                    break;
                }
            }
            else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z >= aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
               //SaveInformationInDebug("If3 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.PxPz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                }
                else
                {
                    aktualnieSprawdzanyNode.PxPz = elementDodawany;
                    break;
                }
            }
            else
            {
                //SaveInformationInDebug("If4 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.MxPz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                }
                else    //
                {
                    aktualnieSprawdzanyNode.MxPz = elementDodawany;
                    break;
                }
            }
        }
    }
    public static void DodajDoDrzewaPozycji(Component _component, ref StrukturaDrzewa korzeń)
    {
        if (korzeń == null)
        {
            korzeń = new StrukturaDrzewa(_component);
        }
        else
        {
            StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
            Vector3 posKom = _component.transform.position;
            while (true)
            {
                if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.MxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.MxMz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.PxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.PxMz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z >= aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.PxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.PxPz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanyNode.MxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.MxPz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
            }
        }
    }
    public static void SaveInformationInDebug(string s)
    {
        StreamWriter writer = new StreamWriter(test, true);
        writer.WriteLine(s);
        writer.Close();
    }

}
