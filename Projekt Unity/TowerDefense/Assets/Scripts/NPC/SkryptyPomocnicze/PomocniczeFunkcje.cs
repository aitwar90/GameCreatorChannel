using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PomocniczeFunkcje
{
    public static StrukturaDrzewa[] korzeńDrzewaPozycji = null;
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
    public static Component WyszukajWDrzewiePozycji(StrukturaDrzewa korzeń)
    {
        if(korzeń == null)
            return null;
        
        StrukturaDrzewa aktualnieSprawdzanyLiść = korzeń;
        while(true)
        {
            if(aktualnieSprawdzanyLiść.lewaGałąź != null)
            {
                aktualnieSprawdzanyLiść = aktualnieSprawdzanyLiść.lewaGałąź;
            }
            else
            {
                break;
            }
        }
        return aktualnieSprawdzanyLiść.komponentGałęzi;
    }
    public static void DodajDoDrzewaPozycji(Component dodawanyKomponent, Vector3 pozycjaObiektuZKomponentem, Vector3 pozycjaOdKtorejLiczonaJestOdległość, StrukturaDrzewa korzeń = null)
    {
        if (korzeń == null)
        {
            korzeń = new StrukturaDrzewa(dodawanyKomponent, pozycjaObiektuZKomponentem, Vector3.Distance(pozycjaObiektuZKomponentem, pozycjaOdKtorejLiczonaJestOdległość));
        }
        else
        {
            StrukturaDrzewa aktualnieSprawdzanaGałąź = korzeń;
            float odlOdPunktuDodawanegoObiektu = Vector3.Distance(pozycjaObiektuZKomponentem, pozycjaOdKtorejLiczonaJestOdległość);
            while (true)
            {

                if (odlOdPunktuDodawanegoObiektu < aktualnieSprawdzanaGałąź.odległośćOdPunktu)
                {
                    if (aktualnieSprawdzanaGałąź.lewaGałąź != null)
                    {
                        aktualnieSprawdzanaGałąź = aktualnieSprawdzanaGałąź.lewaGałąź;
                    }
                    else
                    {
                        aktualnieSprawdzanaGałąź.lewaGałąź = new StrukturaDrzewa(dodawanyKomponent, pozycjaObiektuZKomponentem, odlOdPunktuDodawanegoObiektu);
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanaGałąź.prawaGałąź != null)
                    {
                        aktualnieSprawdzanaGałąź = aktualnieSprawdzanaGałąź.prawaGałąź;
                    }
                    else
                    {
                        aktualnieSprawdzanaGałąź.prawaGałąź = new StrukturaDrzewa(dodawanyKomponent, pozycjaObiektuZKomponentem, odlOdPunktuDodawanegoObiektu);

                        break;
                    }
                }
            }
        }
    }
    public static void DodajDoDrzewaPozycji(StrukturaDrzewa _dodawanaGałąź, float _odległośćOdPunktuDodawanego, StrukturaDrzewa korzeń = null)
    {
        if(korzeń == null)
        {
            return;
        }
        StrukturaDrzewa aktualnieSprawdzanaGałąź = korzeń;
        while(true)
        {
            if (_odległośćOdPunktuDodawanego < aktualnieSprawdzanaGałąź.odległośćOdPunktu)
                {
                    if (aktualnieSprawdzanaGałąź.lewaGałąź != null)
                    {
                        aktualnieSprawdzanaGałąź = aktualnieSprawdzanaGałąź.lewaGałąź;
                    }
                    else
                    {
                        aktualnieSprawdzanaGałąź.lewaGałąź = _dodawanaGałąź;
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanaGałąź.prawaGałąź != null)
                    {
                        aktualnieSprawdzanaGałąź = aktualnieSprawdzanaGałąź.prawaGałąź;
                    }
                    else
                    {
                        aktualnieSprawdzanaGałąź.prawaGałąź = _dodawanaGałąź;

                        break;
                    }
                }
        }
    }
    public static void SkasujKorzeń(Component _komponent, Vector3 _pozycjaKomponentu, Vector3 pozycjaOdKtorejLiczonaJestOdległość, StrukturaDrzewa korzeń)
    {
        if (korzeń == null)
            return;
        else
        {
            StrukturaDrzewa aktualnieSprawdzanyLiść = korzeń;
            float dystans = Vector3.Distance(_pozycjaKomponentu, pozycjaOdKtorejLiczonaJestOdległość);
            while (true)
            {
                if (aktualnieSprawdzanyLiść.odległośćOdPunktu < dystans)
                {
                    if (aktualnieSprawdzanyLiść.lewaGałąź != null)
                    {
                        aktualnieSprawdzanyLiść = aktualnieSprawdzanyLiść.lewaGałąź;
                    }
                }
                else
                {
                    Vector3 posGałęzi = aktualnieSprawdzanyLiść.pozycjaGałęzi;
                    if (_komponent == aktualnieSprawdzanyLiść.komponentGałęzi &&
                    posGałęzi.x + 0.05f > _pozycjaKomponentu.x && posGałęzi.x - 0.05f < _pozycjaKomponentu.x &&
                    posGałęzi.z + 0.05f > _pozycjaKomponentu.x && posGałęzi.z - 0.05f < _pozycjaKomponentu.z)
                    {
                        //Znalazłem szukany liść
                        StrukturaDrzewa[] tempSD = SkasujZDrzewaPozycji(ref aktualnieSprawdzanyLiść);
                        for (byte i = 0; i < 2; i++)
                        {
                            if (tempSD[i] != null)
                            {
                                DodajDoDrzewaPozycji(tempSD[i], tempSD[i].odległośćOdPunktu, korzeń);
                            }
                        }
                    }
                }
            }

        }
    }
    private static StrukturaDrzewa[] SkasujZDrzewaPozycji(ref StrukturaDrzewa kasowanyLiść)
    {
        if (kasowanyLiść == null)
            return null;
        else
        {
            StrukturaDrzewa[] strDrzewa = new StrukturaDrzewa[2];
            if (kasowanyLiść.lewaGałąź != null)
            {
                strDrzewa[0] = kasowanyLiść.lewaGałąź;
            }
            if (kasowanyLiść.prawaGałąź != null)
            {
                strDrzewa[1] = kasowanyLiść.prawaGałąź;
            }
            kasowanyLiść = null;
            return strDrzewa;
        }

    }
}
