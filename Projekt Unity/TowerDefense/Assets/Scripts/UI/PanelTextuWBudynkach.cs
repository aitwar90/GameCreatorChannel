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
        nazwaObiektu.text = s[0];
        punktyŻyciaObiektu.text = s[1];
        koszt.text = s[2];
        obrażenia.text = s[3];
        wymaganyPoziom.text = s[4];
        opis.text = s[5];
        if (s[6] == "CZERWONY")
        {
            wymaganyPoziom.color = Color.red;
            if (s.Length > 6)
            {
                int t = System.Int32.Parse(s[7]);
                if (ManagerGryScript.iloscCoinów < t)
                {
                    kosztBadania.color = Color.red;
                }
                else
                    kosztBadania.color = Color.white;
            }
        }
        else
        {
            wymaganyPoziom.color = Color.white;
            if (s.Length > 6)
            {
                Debug.Log(s[7]);
                int t = System.Int32.Parse(s[7]);
                if (ManagerGryScript.iloscCoinów < t)
                {
                    kosztBadania.color = Color.red;
                }
                else
                    kosztBadania.color = Color.white;
            }
        }
        kosztBadania.text = s[7];
    }
}
