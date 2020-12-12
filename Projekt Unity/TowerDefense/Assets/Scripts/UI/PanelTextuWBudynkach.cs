using UnityEngine;
using UnityEngine.UI;
public class PanelTextuWBudynkach : KontenerKomponentów
{
    public Text nazwaObiektu;
    public Text punktyŻyciaObiektu;
    public Text koszt;
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
        }
        else
            wymaganyPoziom.color = Color.black;
    }
}
