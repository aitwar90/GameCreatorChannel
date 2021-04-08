using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSamouczekScript : MonoBehaviour
{
    public static ManagerSamouczekScript mssInstance = null;
    private SamouczekInfoPanelScript sips = null;
    private SamouczekKliknijTuVisual sktv = null;
    private byte idxProgresuSamouczka = 0;
    private byte symulujManageraUpdate = 0;
    private bool sprawdzajCzyZaliczone = false;
    public TextAsset plikTekstowySamouczka;
    private string[] zaladujTextKonkretne = null;
    private int tmpHajs = 0;
    private Skrzynka[] skrzynki = null;
    private byte e1 = 0, e2 = 0, e3 = 0, e4 = 0;
    private ushort thp = 0, atkIdx = 0, defidx = 0;
    private sbyte zmiennaPomocnicza = -1;
    public static bool byloZaladowane = false;
    private Font fToLoad = null;
    private ushort lastTimer = 0;
    public sbyte ZmiennaPomocnicza
    {
        get
        {
            return zmiennaPomocnicza;
        }
        set
        {
            zmiennaPomocnicza = value;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        if (mssInstance == null)
        {
            mssInstance = this;
            sips = this.GetComponentInChildren<SamouczekInfoPanelScript>();
            sktv = this.GetComponentInChildren<SamouczekKliknijTuVisual>();
            sips.gameObject.SetActive(false);
            sktv.gameObject.SetActive(false);
            sips.UstawTłoEnabeld(this.GetComponent<UnityEngine.UI.Image>());
        }
        else
        {
            Destroy(this);
        }
    }
    public void ŁadujDaneSamouczek()
    {
        this.gameObject.SetActive(true);
        PomocniczeFunkcje.mainMenu.wróćDoMenu.gameObject.SetActive(false);
        idxProgresuSamouczka = 0;
        ManagerGryScript mgs = PomocniczeFunkcje.managerGryScript;
        if (!byloZaladowane)
        {
            //Stworzenie danych tymczasowych zapisujących stan gracza
            tmpHajs = ManagerGryScript.iloscCoinów;
            skrzynki = mgs.skrzynki;
            e1 = mgs.ekwipunekGracza[0].ilośćDanejNagrody;
            e2 = mgs.ekwipunekGracza[1].ilośćDanejNagrody;
            e3 = mgs.ekwipunekGracza[2].ilośćDanejNagrody;
            e4 = mgs.ekwipunekGracza[3].ilośćDanejNagrody;
            skrzynki = new Skrzynka[4];
            for(byte i = 0; i < 4; i++)
            {
                skrzynki[i] = mgs.skrzynki[i];
            }
            thp = mgs.hpIdx;
            atkIdx = mgs.atkIdx;
            defidx = mgs.defIdx;
            byloZaladowane = true;
            PomocniczeFunkcje.mainMenu.AktywujDezaktywujPrzyciskPaneliBudynku();
            lastTimer = (ushort)PomocniczeFunkcje.managerGryScript.czasMiędzyFalami;
        }
        //Przypisanie nowych danych
        ManagerGryScript.iloscCoinów = 50;
        for (byte i = 0; i < mgs.ekwipunekGracza.Length; i++)
        {
            mgs.ekwipunekGracza[i].ilośćDanejNagrody = 0;
        }
        mgs.hpIdx = 0;
        mgs.atkIdx = 0;
        mgs.defIdx = 0;
        sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[0], fToLoad);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("UI_LicznikCzasu", false);
        lastTimer = (ushort)PomocniczeFunkcje.managerGryScript.czasMiędzyFalami;
        PomocniczeFunkcje.managerGryScript.czasMiędzyFalami = 10f;
    }
    public void WywolajProgress()
    {
        idxProgresuSamouczka++;
        switch (idxProgresuSamouczka)
        {
            case 1: //Gracz przesunął kamerę
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[1], fToLoad);
                break;
            case 2: //Gracz otworzył i zamknął panel budynku
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[2], fToLoad);
                PomocniczeFunkcje.celWrogów.ZmianaHP(20);
                break;
            case 3: //Gracz otrzymał podstawowe informacje o interfejsie
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[3], fToLoad);
                PomocniczeFunkcje.mainMenu.AktywujDezaktywujPrzyciskPaneliBudynku(0, true);
                break;
            case 4: //Gracz odpalił panel z budynkami wież
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[4], fToLoad);
                UstawIkonkePomocnicza("kupnoWieża", 0, 0);
                break;
            case 5:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[5], fToLoad);
                UstawIkonkePomocnicza("stawiajBudynek", 0, 0);
                break;
            case 6:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[6], fToLoad);
                PomocniczeFunkcje.mainMenu.AktywujDezaktywujPrzyciskPaneliBudynku(1, true);
                break;
            case 7:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[7], fToLoad);
                UstawIkonkePomocnicza("kupnoMur", 0, 0);
                break;
            case 8:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[8], fToLoad);
                UstawIkonkePomocnicza("kup", 0, 0);
                break;
            case 9:
                UstawIkonkePomocnicza("stawiajBudynek", 0, 0);
                break;
            case 10:
                sprawdzajCzyZaliczone = true;
                UstawIkonkePomocnicza("rotacjaBudynku", 0, 0);
                break;
            case 11:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[9], fToLoad);
                break;
            case 12:    //Rozpoczęcie omawiania nagród
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[10], fToLoad);
                UstawIkonkePomocnicza("coinyNagroda", 0, 0);
                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[0].ilośćDanejNagrody = 1;
                PomocniczeFunkcje.mainMenu.UstawButtonNagrody(0, 1);
                break;
            case 13:    //Cud ocalenia
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[11], fToLoad);
                UstawIkonkePomocnicza("cudOcalenia", 0, 0);
                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[1].ilośćDanejNagrody = 1;
                PomocniczeFunkcje.mainMenu.UstawButtonNagrody(1, 1);
                break;
            case 14:    //Dodatkowa nagroda
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[12], fToLoad);
                UstawIkonkePomocnicza("dodatkowaNagroda", 0, 0);
                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[2].ilośćDanejNagrody = 1;
                PomocniczeFunkcje.mainMenu.UstawButtonNagrody(2, 1);
                break;
            case 15:    //Skrócenie czasu skrzynki
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[13], fToLoad);
                UstawIkonkePomocnicza("skrócenieCzasuSkrzynki", 0, 0);
                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[3].ilośćDanejNagrody = 1;
                PomocniczeFunkcje.mainMenu.UstawButtonNagrody(3, 1);
                break;
            case 16:    //Objaśnienie timera
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[14], fToLoad);
                break;
            case 17:
                sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[15], fToLoad);
                break;
            default:
                Debug.Log("Nic nie robie");
                break;
        }
    }
    void Update()
    {
        if (sprawdzajCzyZaliczone)
        {
            switch (idxProgresuSamouczka)
            {
                case 0: //Przesunięcie kamery
                    Vector3 tOcaPos = PomocniczeFunkcje.oCam.transform.position;
                    if (tOcaPos != MoveCameraScript.bazowePolozenieKameryGry
                    && tOcaPos != Vector3.zero)
                    {
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                    }
                    break;
                case 1: //Otwarcie i zamknięcie panelu głównego obiektu
                    if (zmiennaPomocnicza == -1 && PomocniczeFunkcje.mainMenu.OdpalonyPanel)
                        zmiennaPomocnicza = 0;
                    else if (zmiennaPomocnicza == 0 && !PomocniczeFunkcje.mainMenu.OdpalonyPanel)
                    {
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 2: //Próba naprawy budynku
                    if (zmiennaPomocnicza == -1 && PomocniczeFunkcje.mainMenu.OdpalonyPanel)
                    {
                        UstawIkonkePomocnicza("naprawBudynek", 0, 0);
                    }
                    else if (PomocniczeFunkcje.mainMenu.OdpalonyPanel || (!PomocniczeFunkcje.mainMenu.OdpalonyPanel && zmiennaPomocnicza != 1 && sktv.gameObject.activeInHierarchy))
                    {
                        sktv.WyłączObiekt();
                    }
                    if (zmiennaPomocnicza == 1)  //Kliknieto postaw budynek
                    {
                        sktv.WyłączObiekt();
                        zmiennaPomocnicza = -1;
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                    }
                    break;
                case 3:
                    //Przekazanie informacji o panelu
                    sprawdzajCzyZaliczone = false;
                    WywolajProgress();
                    if (PomocniczeFunkcje.mainMenu.OdpalonyPanel)
                    {
                        PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
                        UstawTenDomyslnyButton.UstawAktywnyButton(sips.przyciskDalejButtonu.gameObject);
                    }
                    break;
                case 4:
                    //Odpal panel z budynkami
                    if (PomocniczeFunkcje.mainMenu.OdpalonyPanelBudynków == 0)   //Odpalony panel z wieżami
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                    }
                    break;
                case 5:
                    //Wybierz budynek do postawienia
                    if (zmiennaPomocnicza == 12)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 6:
                    //Postaw budynek
                    if (zmiennaPomocnicza == -3)    //-3 postaw budynek
                    {
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                        WywolajProgress();
                    }
                    break;
                case 7:
                    //Kupno budynku muru
                    if (PomocniczeFunkcje.mainMenu.OdpalonyPanelBudynków == 1)
                    {
                        sktv.WyłączObiekt();
                        sprawdzajCzyZaliczone = false;
                        WywolajProgress();
                    }
                    break;
                case 8:
                    if (zmiennaPomocnicza == 10)
                    {
                        WywolajProgress();
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 9:
                    if(zmiennaPomocnicza == 12) //Kliknięte postaw już murek
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        //sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 10:
                    //Wyłączenie markera przycisku obrotu
                    if (zmiennaPomocnicza == -3)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 11:
                    WywolajProgress();
                    sprawdzajCzyZaliczone = false;
                    break;
                case 12:    //Nagrody
                    if (zmiennaPomocnicza == 2)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 13:
                    if (zmiennaPomocnicza == 3)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 14:
                    if (zmiennaPomocnicza == 4)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;
                case 15:
                    if (zmiennaPomocnicza == 5)
                    {
                        sktv.WyłączObiekt();
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("UI_LicznikCzasu", true);
                    }
                    break;
                case 16:    //Licznik czasu
                    if (zmiennaPomocnicza == -100)  //Wróg został pokonany
                    {
                        WywolajProgress();
                        sprawdzajCzyZaliczone = false;
                        zmiennaPomocnicza = -1;
                    }
                    break;

            }
        }
        if (idxProgresuSamouczka == 16 )    //Na jakim etapie timer ma zacząć być odliczany
        {
            if (symulujManageraUpdate < 3)
                symulujManageraUpdate++;
            else
            {
                PomocniczeFunkcje.managerGryScript.ObslTimerFal();
                symulujManageraUpdate = 0;
            }
        }
    }
    private void UstawIkonkePomocnicza(string nazwaObiektu, float x, float y)//x i y to offsety
    {
        Vector2 p = PomocniczeFunkcje.mainMenu.ZwrocRectTransformObiektu(nazwaObiektu);
        this.sktv.UstawIkone(p.x, p.y, x, y);
        this.sktv.WłączObiekt();
    }
    public void ZaladujText()
    {
        if (plikTekstowySamouczka != null)
        {
            zaladujTextKonkretne = null;
            sbyte jez = PomocniczeFunkcje.mainMenu.lastIdxJezyka;   //0 - Polski, 1 - Angielski, 2 - Ukraiński
            List<string> listaOpisu = new List<string>();
            string fs = plikTekstowySamouczka.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            if (PomocniczeFunkcje.managerGryScript.fontyJezyków != null)
            {
                bool znalazlem = false;
                for (byte i = 0; i < PomocniczeFunkcje.managerGryScript.fontyJezyków.Length; i++)
                {
                    for (byte j = 0; j < PomocniczeFunkcje.managerGryScript.fontyJezyków[i].idxJezyka.Length; j++)
                    {
                        if (PomocniczeFunkcje.managerGryScript.fontyJezyków[i].idxJezyka[j] == jez+1)
                        {
                            fToLoad = PomocniczeFunkcje.managerGryScript.fontyJezyków[i].font;
                            znalazlem = true;
                            break;
                        }
                    }
                    if(znalazlem)
                        break;
                }
            }
            for (ushort i = 0; i < fLines.Length; i++)
            {
                if (fLines[i] == "")
                    continue;
                string[] lot = fLines[i].Split('^');
                listaOpisu.Add(lot[jez]);
            }
            zaladujTextKonkretne = listaOpisu.ToArray();
        }
        else
        {
            Debug.Log("Brak pliku tekstowego");
        }
    }
    public void OpuśćSamouczek(bool res = true)
    {
        ZwróćMiDane();
        if(res)
            PomocniczeFunkcje.mainMenu.ResetSceny(false);
        PomocniczeFunkcje.mainMenu.PrzełączUI(true);
        this.sips.gameObject.SetActive(false);
        this.sktv.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.wróćDoMenu.gameObject.SetActive(true);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("UI_LicznikCzasu", true);
    }
    private void ZwróćMiDane()
    {
        if (byloZaladowane)
        {
            ManagerGryScript mgs = PomocniczeFunkcje.managerGryScript;
            ManagerGryScript.iloscCoinów = tmpHajs;
            mgs.skrzynki = skrzynki;
            mgs.ekwipunekGracza[0].ilośćDanejNagrody = e1;
            if(e1 > 0)
            {
                PomocniczeFunkcje.mainMenu.przyciskiNagród[0].interactable = true;
            }
            mgs.ekwipunekGracza[1].ilośćDanejNagrody = e2;
            if(e2 > 0)
            {
                PomocniczeFunkcje.mainMenu.przyciskiNagród[1].interactable = true;
            }
            mgs.ekwipunekGracza[2].ilośćDanejNagrody = e3;
            if(e3 > 0)
            {
                PomocniczeFunkcje.mainMenu.przyciskiNagród[2].interactable = true;
            }
            mgs.ekwipunekGracza[3].ilośćDanejNagrody = e4;
            if(e4 > 0)
            {
                PomocniczeFunkcje.mainMenu.przyciskiNagród[3].interactable = true;
            }
            for(byte i = 0; i < 4; i++)
            {
                mgs.skrzynki[i] = skrzynki[i];
            }
            mgs.hpIdx = thp;
            mgs.atkIdx = atkIdx;
            mgs.defIdx = defidx;
            byloZaladowane = false;
            PomocniczeFunkcje.mainMenu.AktywujDezaktywujPrzyciskPaneliBudynku(255, true);
            PomocniczeFunkcje.managerGryScript.czasMiędzyFalami = lastTimer;
        }
    }
    public void ZamknijPanel()
    {
        sprawdzajCzyZaliczone = true;
        this.sips.ZamknijPanel();
        if (idxProgresuSamouczka == 17)  //Koniec samouczka
        {
            OpuśćSamouczek();
        }
    }
    public void WyłączVisual()
    {
        this.sktv.WyłączObiekt();
    }
    ///<summary>Sprawdź, czy test zgadza się z określoną wartością.</summary>
    ///<param name="i">Sprawdza czy parametr jest zgodny z aktualnym stanem progresu samouczka.</param>
    public bool CzyZgadzaSięIDXGłówny(sbyte i)
    {
        if(i == idxProgresuSamouczka)
            return true;
        return false;
    }
}
