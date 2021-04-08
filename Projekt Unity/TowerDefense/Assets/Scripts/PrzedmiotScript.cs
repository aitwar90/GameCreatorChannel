using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PrzedmiotScript : MonoBehaviour
{
    public string nazwaPrzedmiotu;
    public Image obrazek;
    public TypPrzedmiotu typPrzedmiotu;
    public ushort minParam;
    public ushort maxParam;
    public byte liczbaItemówOtrzymywanych = 1;
    public byte ilośćDanejNagrody = 0;
    private byte timer = 0;
    public void AktualizujNazwe()
    {
        switch (typPrzedmiotu)
        {
            case TypPrzedmiotu.Coiny:
                nazwaPrzedmiotu = PomocniczeFunkcje.mainMenu.przyciskiNagród[0].GetComponentInChildren<Text>().text;
                break;
            case TypPrzedmiotu.CudOcalenia:
                nazwaPrzedmiotu = PomocniczeFunkcje.mainMenu.przyciskiNagród[1].GetComponentInChildren<Text>().text;
                break;
            case TypPrzedmiotu.DodatkowaNagroda:
                nazwaPrzedmiotu = PomocniczeFunkcje.mainMenu.przyciskiNagród[2].GetComponentInChildren<Text>().text;
                break;
            case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:
                nazwaPrzedmiotu = PomocniczeFunkcje.mainMenu.przyciskiNagród[3].GetComponentInChildren<Text>().text;
                break;
        }
    }
    public void AktywujPrzedmiot()
    {
        if (ilośćDanejNagrody > 0)
        {
            switch (typPrzedmiotu)
            {
                case TypPrzedmiotu.Coiny:
                    PomocniczeFunkcje.managerGryScript.UstawIlośćCoinów((short)Random.Range(minParam, maxParam + 1));
                    ilośćDanejNagrody--;
                    break;
                case TypPrzedmiotu.CudOcalenia:
                    if (PomocniczeFunkcje.celWrogów.AktualneŻycie > 0)
                    {
                        PomocniczeFunkcje.managerGryScript.CudOcalenia();
                        ilośćDanejNagrody--;
                    }
                    break;
                case TypPrzedmiotu.DodatkowaNagroda:
                    ilośćDanejNagrody--;
                    byte t = DodajNagrode(true);
                    PomocniczeFunkcje.mainMenu.UstawButtonNagrody(t, PomocniczeFunkcje.managerGryScript.ekwipunekGracza[t].ilośćDanejNagrody);
                    break;
                case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:    //Na SWITCH zwiększa zadawane obrażenia przez wieże o 20
                    if(ManagerGryScript.bonusDoObrażeń > 30)   //Jeśli bonus jest większy niż 30 to nie aplikuj
                        break;
                    ManagerGryScript.bonusDoObrażeń += 10;
                    /* UNITY_ANDROID
                bool czyUzylem = false;
                for (byte i = 0; i < 4; i++)
                {
                    Skrzynka s = PomocniczeFunkcje.managerGryScript.ZwróćSkrzynkeOIndeksie(i);
                    if (s.ReuseTimer)
                    {
                        s.OdejmnijCzas(Random.Range(-minParam, -maxParam - 1));
                        czyUzylem = true;
                    }
                }
                if (czyUzylem) */
                    ilośćDanejNagrody--;
                    break;
            }
        }
        else
        {
            Debug.Log("Brak przedmiotu");
        }
    }
    public byte DodajNagrode(bool czyDodatkowa = false)
    {
        byte mP = (byte)(System.Enum.GetValues(typeof(TypPrzedmiotu)).Length);
        byte losowany = (byte)Random.Range(0, mP);
        PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].ilośćDanejNagrody += (czyDodatkowa) ?
        (byte)(PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].liczbaItemówOtrzymywanych * 2) :
        PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].liczbaItemówOtrzymywanych;
        PomocniczeFunkcje.mainMenu.tekstCoWygrales.text = PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].nazwaPrzedmiotu;
        return losowany;
    }
    public void Mrygaj()
    {
        if (ilośćDanejNagrody > 0)
        {
            timer += 5;
            Color tmpA = obrazek.color;
            tmpA.a = Mathf.Clamp01(Mathf.Lerp(0.1f, 1.0f, timer / 51.0f));
            obrazek.color = tmpA;
            if (timer > 51)
                timer = 0;
        }
        else if (timer != 51)
        {
            timer = 51;
            Color tmpA = obrazek.color;
            tmpA.a = 1.0f;
            obrazek.color = tmpA;
        }
    }
}

