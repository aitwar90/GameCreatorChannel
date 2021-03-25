using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UstawTenDomyslnyButton : MonoBehaviour
{
    public GameObject[] domyślnyGO;
    private static GameObject[,] ostatnieButtony = new GameObject[10, 2];
    private static GameObject[] sDO;
    /*
    MENU
    0-Main Menu
    1-Options Menu
    2-Po Graj
    3-Panel skrzynki
    4-Creditsy
    GRA
    5-Odpalony Panel Statyczny
    6-Odpalony Panel do budowy budynków
    7-Virtualny Button w grze rozbudowa
    8-Game Over Panel
    9-Virtualny Button w grze Walka
    */
    public static byte aktualnyStanNaEkranie = 0;
    private static byte wczesniejszyStan = 0;
    void Awake()
    {
        sDO = domyślnyGO;
    }
    void LateUpdate()
    {
        if (PomocniczeFunkcje.eSystem.currentSelectedGameObject == null) //Brak aznaczonego obiektu
        {
            PomocniczeFunkcje.eSystem.SetSelectedGameObject(domyślnyGO[aktualnyStanNaEkranie]);
        }
        else if (Input.anyKey)
        {
            switch (aktualnyStanNaEkranie)
            {
                case 5: //Odpalony Panel Statyczny
                    if (Input.GetButtonDown("Cancel"))   //Wyłącz Panel
                    {
                        PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
                        PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt = null;
                        aktualnyStanNaEkranie = wczesniejszyStan;
                        OdpalOstatni();
                    }
                    else if (Input.GetButtonDown("Submit"))  //Kliknij napraw
                    {
                        PanelStatyczny ps = PomocniczeFunkcje.mainMenu.GetKontenerKomponentówStatic;
                        if (ps != null)
                        {
                            if (ps.naprawButton.interactable)
                            {
                                ps.NaprawBudynek();
                            }
                        }
                    }
                    break;
                case 6: //Odpalony Panel budowy budynków
                    if (Input.GetButtonDown("Cancel"))   //Zaznacz X
                    {
                        GameObject goDoZaznaczenia = PomocniczeFunkcje.mainMenu.uiBudynkiPanel.transform.Find("Wróć").gameObject;
                        if (goDoZaznaczenia == PomocniczeFunkcje.eSystem.currentSelectedGameObject)
                        {
                            OdpalWczesny();
                        }
                        else
                        {
                            UstawAktywnyButton(goDoZaznaczenia);
                        }
                    }
                    break;
                case 7: //Rozbudowa czas gry
                    if (Input.GetButtonDown("Cancel"))   //Zaznacz Wróć do MENU
                    {
                        GameObject goDoZaznaczenia;
                        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)   //Samouczek
                        {
                            goDoZaznaczenia = PomocniczeFunkcje.mainMenu.transform.Find("UIGry/SamouczekPanel/OpuśćSamouczekButton").gameObject;
                        }
                        else
                        {
                            goDoZaznaczenia = PomocniczeFunkcje.mainMenu.wróćDoMenu.gameObject;

                        }
                        if (goDoZaznaczenia == PomocniczeFunkcje.eSystem.currentSelectedGameObject)
                        {
                            OdpalWczesny();
                        }
                        else
                        {
                            UstawAktywnyButton(goDoZaznaczenia);
                        }
                    }
                    break;
            }
        }
    }
    public static void UstawAktywnyButton(GameObject obiektUstawiany)
    {
        if (ostatnieButtony[aktualnyStanNaEkranie, 0] == null)
        {
            ostatnieButtony[aktualnyStanNaEkranie, 0] = obiektUstawiany;
        }
        else
        {
            ostatnieButtony[aktualnyStanNaEkranie, 1] = ostatnieButtony[aktualnyStanNaEkranie, 0];
            ostatnieButtony[aktualnyStanNaEkranie, 0] = obiektUstawiany;
        }
        PomocniczeFunkcje.eSystem.SetSelectedGameObject(obiektUstawiany);
    }
    ///<summary>Metoda ustawia przedostatnio aktywny przycisk</summary>
    public static void OdpalWczesny()
    {
        if (ostatnieButtony[aktualnyStanNaEkranie, 1] != null)
        {
            UstawAktywnyButton(ostatnieButtony[aktualnyStanNaEkranie, 1]);
        }
        else
        {
            UstawAktywnyButton(sDO[aktualnyStanNaEkranie]);
        }
    }
    ///<summary>Metoda ustawia ostatnio aktywny przycisk</summary>
    public static void OdpalOstatni()
    {
        if (ostatnieButtony[aktualnyStanNaEkranie, 0] != null)
        {
            UstawAktywnyButton(ostatnieButtony[aktualnyStanNaEkranie, 0]);
        }
        else
        {
            UstawAktywnyButton(sDO[aktualnyStanNaEkranie]);
        }
    }
    ///<summary>Metoda ustawia ostatnio aktywny przycisk</summary>
    ///<param name="ustIdx">Indeks stanu UI jaki ma zostać ustawiony (aktualnyStanNaEkranie = domyślnie -1)</param>
    public static void UstawDomyślnyButton(sbyte ustIdx = -1, bool ustawPoprzedniStan = false)
    {
        if (ustIdx > -1 || ustawPoprzedniStan)
        {
            ZaktualizujStan((byte)ustIdx, ustawPoprzedniStan);
        }
        UstawAktywnyButton(sDO[aktualnyStanNaEkranie]);
    }
    ///<summary>Metoda aktualizuje stan</summary>
    ///<param name="ustIdx">Indeks stanu UI jaki ma zostać ustawiony (aktualnyStanNaEkranie)</param>
    public static void ZaktualizujStan(byte ustIdx, bool UstawPoprzedniStan = false)
    {
        if (UstawPoprzedniStan)
        {
            byte tmp = aktualnyStanNaEkranie;
            aktualnyStanNaEkranie = wczesniejszyStan;
            wczesniejszyStan = tmp;
        }
        else
        {
            wczesniejszyStan = aktualnyStanNaEkranie;
            aktualnyStanNaEkranie = ustIdx;
        }
    }
}
