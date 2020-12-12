using UnityEngine;
using UnityEngine.UI;
public class PanelDynamiczny : KontenerKomponentów
{
    public Text nazwaObiektu;
    public Text punktyZycia;
    public Text obrazenia;

    public override void UstawDane(string[] s)
    {
        nazwaObiektu.text = s[0];
        punktyZycia.text = s[1];
        obrazenia.text = s[2];
    }
}
