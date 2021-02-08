using UnityEngine;
using UnityEngine.UI;
public class PanelTextuWBudynkach : KontenerKomponentów
{
    public Text nazwaObiektu;
    public Text punktyŻyciaObiektu;
    public Text koszt;
    public Text kosztBadania;
    public Text obrażenia;
    public Text wymaganyPoziom;
    public Text opis;

    public override void UstawDane(string[] s)
    {
        bool wyłączKupIPostaw = false;
        nazwaObiektu.text = s[0];
        punktyŻyciaObiektu.text = s[1];
        koszt.text = s[2];
        kosztBadania.text = s[3];
        obrażenia.text = s[4];
        if (s[5] != "ERROR")
            wymaganyPoziom.text = s[5];
        else
            wymaganyPoziom.text = "-";
        opis.text = s[6];
        if (s[7] == "CZERWONY")
        {
            wyłączKupIPostaw = true;
            wymaganyPoziom.color = Color.red;
            int t = System.Int32.Parse(s[4]);
            if (ManagerGryScript.iloscCoinów < t)
            {
                kosztBadania.color = Color.red;
            }
        }
        else
        {
            wymaganyPoziom.color = Color.white;
            int t = System.Int32.Parse(s[4]);
            if (ManagerGryScript.iloscCoinów < t)
            {
                wyłączKupIPostaw = true;
                kosztBadania.color = Color.red;
            }
        }
        if (wyłączKupIPostaw)
        {

            //Wyłącz opcję kupna
            PomocniczeFunkcje.mainMenu.kup.interactable = false;
            PomocniczeFunkcje.mainMenu.stawiajBudynek.interactable = false;
        }
    }
}
