using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PomocniczeFunkcje
{
    public static StrukturaDrzewa korzeńDrzewaPozycji = null;
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
    public static Component WyszukajWDrzewie(StrukturaDrzewa korzeń, Vector3 pozycjaWyszukiwującego)
    {
        if (korzeń == null)
            return null;
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
        while (true)
        {
            if (pozycjaWyszukiwującego.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x)
            {
                if (aktualnieSprawdzanyNode.lewaGałąźX != null)
                {
                    if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.x - pozycjaWyszukiwującego.x) <
                    Mathf.Abs(aktualnieSprawdzanyNode.lewaGałąźX.pozycjaGałęzi.x - pozycjaWyszukiwującego.x))
                    {   //Lewe odnóże drzewa
                        if (pozycjaWyszukiwującego.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.lewaGałąźZ != null)
                            {
                                if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z) <
                                    Mathf.Abs(aktualnieSprawdzanyNode.lewaGałąźZ.pozycjaGałęzi.z - pozycjaWyszukiwującego.z))
                                {
                                    //Wychodzi na to że ten jest najbliższy
                                    return aktualnieSprawdzanyNode.komponentGałęzi;
                                }
                                else
                                {
                                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                                }
                            }
                            else
                            {
                                return aktualnieSprawdzanyNode.komponentGałęzi;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ != null)
                            {
                                if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z) <
                                Mathf.Abs(aktualnieSprawdzanyNode.prawaGałąźZ.pozycjaGałęzi.z - pozycjaWyszukiwującego.z))
                                {
                                    return aktualnieSprawdzanyNode.komponentGałęzi;
                                }
                                else
                                {
                                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                                }
                            }
                            else
                            {
                                return aktualnieSprawdzanyNode.komponentGałęzi;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźX;
                    }
                }
                else
                {
                    return aktualnieSprawdzanyNode.komponentGałęzi;
                }
            }
            else    //Jeśli jednak ma szukać w prawym odnóżu drzewa
            {
                if (aktualnieSprawdzanyNode.prawaGałąźX != null)
                {
                    if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.x - pozycjaWyszukiwującego.x) <
                    Mathf.Abs(aktualnieSprawdzanyNode.prawaGałąźX.pozycjaGałęzi.x - pozycjaWyszukiwującego.x))
                    {   //Lewe odnóże drzewa
                        if (pozycjaWyszukiwującego.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ != null)
                            {
                                if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z) <
                                    Mathf.Abs(aktualnieSprawdzanyNode.lewaGałąźZ.pozycjaGałęzi.z - pozycjaWyszukiwującego.z))
                                {
                                    //Wychodzi na to że ten jest najbliższy
                                    return aktualnieSprawdzanyNode.komponentGałęzi;
                                }
                                else
                                {
                                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                                }
                            }
                            else
                            {
                                return aktualnieSprawdzanyNode.komponentGałęzi;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ != null)
                            {
                                if (Mathf.Abs(aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z) <
                                Mathf.Abs(aktualnieSprawdzanyNode.prawaGałąźZ.pozycjaGałęzi.z - pozycjaWyszukiwującego.z))
                                {
                                    return aktualnieSprawdzanyNode.komponentGałęzi;
                                }
                                else
                                {
                                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                                }
                            }
                            else
                            {
                                return aktualnieSprawdzanyNode.komponentGałęzi;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźX;
                    }
                }
                else
                {
                    return aktualnieSprawdzanyNode.komponentGałęzi;
                }
            }
        }
    }
    public static void SkasujElementDrzewa(StrukturaDrzewa korzeń, Component _komponentDoSkasowania)
    {
        if (korzeń == null)
            return;
        StrukturaDrzewa[,] ponownieUstaw = ZnajdźElementPoKomponencie(korzeń, _komponentDoSkasowania);
        for(byte i = 0; i < 2; i++)
        {
            for(byte j = 0; j < 2; j++)
            {
                if(ponownieUstaw[i,j] != null)
                {
                    DodajDoDrzewaPozycji(korzeń, ponownieUstaw[i,j]);
                }
            }
        }
    }
    private static StrukturaDrzewa[,] ZnajdźElementPoKomponencie(StrukturaDrzewa korzeń, Component _com)
    {
        StrukturaDrzewa aktualnyElement = korzeń;
        Vector3 pos = _com.transform.position;
        while (true)
        {
            if (_com == aktualnyElement.komponentGałęzi)
            {
                //Znalazłem :)
                StrukturaDrzewa [,] tabToReturn = {{null, null}, {null, null}};
                if(aktualnyElement.lewaGałąźX != null)
                {
                    tabToReturn[0,0] = aktualnyElement.lewaGałąźX;
                }
                if(aktualnyElement.prawaGałąźX != null)
                {
                    tabToReturn[0,1] = aktualnyElement.prawaGałąźX;
                }
                if(aktualnyElement.lewaGałąźZ != null)
                {
                    tabToReturn[1,0] = aktualnyElement.lewaGałąźZ;
                }
                if(aktualnyElement.prawaGałąźZ != null)
                {
                    tabToReturn[1,1] = aktualnyElement.prawaGałąźZ;
                }
                return tabToReturn;

            }
            if (pos.x < aktualnyElement.pozycjaGałęzi.x)
            {
                if (aktualnyElement.lewaGałąźX == null)
                {
                    Debug.Log("Nie odnalazłem elementu drzewa");
                    break;
                }
                else if (pos.x + 10f > aktualnyElement.pozycjaGałęzi.x &&
                    pos.x - 10f < aktualnyElement.pozycjaGałęzi.x)
                {
                    if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                    {
                        if (aktualnyElement.lewaGałąźZ == null)
                        {
                            Debug.Log("Nie odnalazłem elementu drzewa");
                            break;
                        }
                        else
                        {
                            aktualnyElement = aktualnyElement.lewaGałąźZ;
                        }
                    }
                    else
                    {
                        if (aktualnyElement.prawaGałąźZ == null)
                        {
                            Debug.Log("Nie odnalazłem elementu drzewa");
                            break;
                        }
                        else
                        {
                            aktualnyElement = aktualnyElement.prawaGałąźZ;
                        }
                    }
                }
            }
            else
            {
                if (aktualnyElement.prawaGałąźX == null)
                {
                    Debug.Log("Nie odnalazłem elementu drzewa");
                    break;
                }
                else if (pos.x + 10f > aktualnyElement.pozycjaGałęzi.x &&
                pos.x - 10f < aktualnyElement.pozycjaGałęzi.x)
                {
                    if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                    {
                        if (aktualnyElement.lewaGałąźZ == null)
                        {
                            Debug.Log("Nie odnalazłem elementu drzewa");
                            break;
                        }
                        else
                        {
                            aktualnyElement = aktualnyElement.lewaGałąźZ;
                        }
                    }
                    else
                    {
                        if (aktualnyElement.prawaGałąźZ == null)
                        {
                            Debug.Log("Nie odnalazłem elementu drzewa");
                            break;
                        }
                        else
                        {
                            aktualnyElement = aktualnyElement.prawaGałąźZ;
                        }
                    }
                }
                else
                {
                    aktualnyElement = aktualnyElement.prawaGałąźX;
                }
            }
        }
        return null;
    }
    private static void DodajDoDrzewaPozycji(StrukturaDrzewa korzeń, StrukturaDrzewa elementDodawany)
    {
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
            Vector3 posKom = elementDodawany.pozycjaGałęzi;
            while (true)
            {
                if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x) //Lewe odnóże drzewa
                {
                    if (aktualnieSprawdzanyNode.lewaGałąźX == null)
                    {
                        aktualnieSprawdzanyNode.lewaGałąźX = elementDodawany;
                        break;
                    }
                    else if (posKom.x + 10f > aktualnieSprawdzanyNode.pozycjaGałęzi.x &&
                    posKom.x - 10f < aktualnieSprawdzanyNode.pozycjaGałęzi.x)
                    {
                        if (posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.lewaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.lewaGałąźZ = elementDodawany;
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.prawaGałąźZ = elementDodawany;
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźX;
                    }
                }
                else    //Prawe odnóże drzewa
                {
                    if (aktualnieSprawdzanyNode.prawaGałąźX == null)
                    {
                        aktualnieSprawdzanyNode.prawaGałąźX = elementDodawany;
                        break;
                    }
                    else if (posKom.x + 10f > aktualnieSprawdzanyNode.pozycjaGałęzi.x &&
                    posKom.x - 10f < aktualnieSprawdzanyNode.pozycjaGałęzi.x)
                    {
                        if (posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.lewaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.lewaGałąźZ = elementDodawany;
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.prawaGałąźZ = elementDodawany;
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźX;
                    }
                }
            }
    }
    public static void DodajDoDrzewaPozycji(Component _component, StrukturaDrzewa korzeń = null)
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
                if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x) //Lewe odnóże drzewa
                {
                    if (aktualnieSprawdzanyNode.lewaGałąźX == null)
                    {
                        aktualnieSprawdzanyNode.lewaGałąźX = new StrukturaDrzewa(_component);
                        break;
                    }
                    else if (posKom.x + 10f > aktualnieSprawdzanyNode.pozycjaGałęzi.x &&
                    posKom.x - 10f < aktualnieSprawdzanyNode.pozycjaGałęzi.x)
                    {
                        if (posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.lewaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.lewaGałąźZ = new StrukturaDrzewa(_component);
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.prawaGałąźZ = new StrukturaDrzewa(_component);
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźX;
                    }
                }
                else    //Prawe odnóże drzewa
                {
                    if (aktualnieSprawdzanyNode.prawaGałąźX == null)
                    {
                        aktualnieSprawdzanyNode.prawaGałąźX = new StrukturaDrzewa(_component);
                        break;
                    }
                    else if (posKom.x + 10f > aktualnieSprawdzanyNode.pozycjaGałęzi.x &&
                    posKom.x - 10f < aktualnieSprawdzanyNode.pozycjaGałęzi.x)
                    {
                        if (posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                        {
                            if (aktualnieSprawdzanyNode.lewaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.lewaGałąźZ = new StrukturaDrzewa(_component);
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.lewaGałąźZ;
                            }
                        }
                        else
                        {
                            if (aktualnieSprawdzanyNode.prawaGałąźZ == null)
                            {
                                aktualnieSprawdzanyNode.prawaGałąźZ = new StrukturaDrzewa(_component);
                                break;
                            }
                            else
                            {
                                aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźZ;
                            }
                        }
                    }
                    else
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.prawaGałąźX;
                    }
                }
            }
        }
    }

}
