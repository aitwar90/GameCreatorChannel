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
    public string[] zaladujTextKonkretne = null;
    private ushort tmpHajs = 0;
    private Skrzynka[] skrzynki = null;
    private byte e1 = 0, e2 = 0, e3 = 0, e4 = 0;
    private ushort thp = 0, atkIdx = 0, defidx = 0;
    // Start is called before the first frame update
    void Awake()
    {
        if(mssInstance == null)
        {
            mssInstance = this;
            sips = this.GetComponentInChildren<SamouczekInfoPanelScript>();
            sktv = this.GetComponentInChildren<SamouczekKliknijTuVisual>();
            sips.gameObject.SetActive(false);
            sktv.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this);
        }
    }
    public void ŁadujDaneSamouczek()
    {
        this.gameObject.SetActive(true);
        ManagerGryScript mgs = PomocniczeFunkcje.managerGryScript;
        //Stworzenie danych tymczasowych zapisujących stan gracza
        tmpHajs = ManagerGryScript.iloscCoinów;
        skrzynki = mgs.skrzynki;
        e1 = mgs.ekwipunekGracza[0].ilośćDanejNagrody;
        e2 = mgs.ekwipunekGracza[1].ilośćDanejNagrody;
        e3 = mgs.ekwipunekGracza[2].ilośćDanejNagrody;
        e4 = mgs.ekwipunekGracza[3].ilośćDanejNagrody;
        thp = mgs.hpIdx;
        atkIdx = mgs.atkIdx;
        defidx = mgs.defIdx;
        //Przypisanie nowych danych
        ManagerGryScript.iloscCoinów = 30;
        for(byte i = 0; i < mgs.ekwipunekGracza.Length; i++)
        {
            mgs.ekwipunekGracza[i].ilośćDanejNagrody = 0;
        }
        mgs.hpIdx = 0;
        mgs.atkIdx = 0;
        mgs.defIdx = 0;
        sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[0]);
    }
    public void WywolajProgress()
    {
        idxProgresuSamouczka++;
        switch(idxProgresuSamouczka)
        {
            case 1:
            Debug.Log("Przesunąłeś kamerę");
            //sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[1]);
            break;
            default:
            Debug.Log("Nic nie robie");
            break;
        }
    }
    void Update()
    {
        if(sprawdzajCzyZaliczone)
        {
            switch(idxProgresuSamouczka)
            {
                case 0: //Przesunięcie kamery
                Vector3 tOcaPos = PomocniczeFunkcje.oCam.transform.position;
                if(tOcaPos!= MoveCameraScript.bazowePolozenieKameryGry 
                && tOcaPos != Vector3.zero)
                {
                    WywolajProgress();
                    sprawdzajCzyZaliczone = false;
                }
                break;
            }
        }
        if(idxProgresuSamouczka > 3)    //Na jakim etapie timer ma zacząć być odliczany
        {
            if(symulujManageraUpdate < 5)
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
        if(plikTekstowySamouczka != null)
        {
            zaladujTextKonkretne = null;
            sbyte jez = PomocniczeFunkcje.mainMenu.lastIdxJezyka;   //0 - Polski, 1 - Angielski
            List<string> listaOpisu = new List<string>();
            string fs = plikTekstowySamouczka.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            for(ushort i = 0; i < fLines.Length; i++)
            {
                if(fLines[i] == "")
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
    public void OpuśćSamouczek()
    {
        ZwróćMiDane();
        PomocniczeFunkcje.mainMenu.ResetSceny(false);
        PomocniczeFunkcje.mainMenu.PrzełączUI(true);
        this.sips.ZamknijPanel();
        this.sktv.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }
    private void ZwróćMiDane()
    {
        ManagerGryScript mgs = PomocniczeFunkcje.managerGryScript;
        ManagerGryScript.iloscCoinów = tmpHajs;
        mgs.skrzynki = skrzynki;
        mgs.ekwipunekGracza[0].ilośćDanejNagrody = e1;
        mgs.ekwipunekGracza[1].ilośćDanejNagrody = e2;
        mgs.ekwipunekGracza[2].ilośćDanejNagrody = e3;
        mgs.ekwipunekGracza[3].ilośćDanejNagrody = e4;
        mgs.hpIdx = thp;
        mgs.atkIdx = atkIdx;
        mgs.defIdx = defidx;
    }
    public void ZamknijPanel()
    {
        sprawdzajCzyZaliczone = true;
        this.sips.ZamknijPanel();
    }
    public bool PozwólZamknąćPanelBudynków()
    {
        /*
        Należy tu podać indeksy, w których nie można zamknąć panelu z budynkami (gracz ma je postawić lub kupić)
        */
        //if(idxProgresuSamouczka != )
        return true;
    }
}
