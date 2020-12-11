using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button nastepnyPoziom;
    public Button powtorzPoziom;
    public PrzyciskiSkrzynekIReklam[] buttonSkrzynki;
    public Button wróćDoMenu;
    public Button rekZaWyzszaNagrode;
    public Button kup;
    public Button wróc;
    public Button zmianaJezyka;
    public Button rotacjaBudynku;
    public Button odpalPoziom;
    public Button lvlNizej;
    public Button lvlWyzej;
    public Button epokaNizej;
    public Button epokaWyzej;
    #region Akademia
    public Button buttonAZycie;
    public Button buttonAAtak;
    public Button buttonAObrona;
    #endregion
    #region Ekwipunek
    [Tooltip("Przyciski nagród powinny być wpięte w tablicę w kolejności zgodnej z taką jak Ekwipunek Gracza w ManagerGryScript.cs")]
    public Button[] przyciskiNagród;
    #endregion
    #region TextUI
    public Text ilośćCoinów;
    public Text ilośćFal;
    private Text actWybEpoka;
    public InputField poziomWEpoce;
    #endregion
    #region Panel
    public KontenerKomponentów panelDynamiczny;
    public KontenerKomponentów panelStatyczny;
    #endregion
    public Slider sliderDźwięku;
    public static bool czyMenuEnable = true;
    private GameObject menu;
    private GameObject uiGry;
    private GameObject optionsMenu;
    private GameObject poGraj;
    private GameObject reklamyPanel;
    private GameObject ui_down;
    private GameObject goPanel;
    private GameObject uiBudynkiPanel;
    private Button przyciskWznów;
    private Vector3 lastPosCam = Vector3.zero;
    private sbyte wybranaNagroda = -1;
    public sbyte lastIdxJezyka = 0;
    private static MainMenu singelton = null;
    private bool odpalonyPanel = false;
    private RectTransform rectHpBar;
    private ushort[] idxWież = null;
    private ushort[] idxMurów = null;
    private ushort[] idxInne = null;
    private sbyte lastPanelEnabledBuildings = -1;
    public sbyte UstawLubPobierzOstatniIdexJezyka
    {
        get
        {
            return lastIdxJezyka;
        }
        set
        {
            lastIdxJezyka = (sbyte)(value - 1);
            ZmieńJęzyk();
        }
    }
    public bool OdpalonyPanel
    {
        get
        {
            return odpalonyPanel;
        }
        set
        {
            odpalonyPanel = value;
        }
    }
    void Awake()
    {
        if (singelton == null)
        {
            singelton = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        this.poziomWEpoce.characterValidation = InputField.CharacterValidation.Integer;
        menu = this.transform.Find("Menu/MainMenu").gameObject;
        uiGry = this.transform.Find("UIGry").gameObject;
        optionsMenu = this.transform.Find("Menu/OptionsMenu").gameObject;
        poGraj = this.transform.Find("Menu/PoGraj").gameObject;
        reklamyPanel = this.transform.Find("Menu/PanelSkrzynki").gameObject;
        ui_down = uiGry.transform.Find("ui_down").gameObject;
        przyciskWznów = this.transform.Find("Menu/MainMenu/ResumeButton").GetComponent<Button>();
        actWybEpoka = this.transform.Find("Menu/PoGraj/AktualnieWybEpoka").GetComponent<Text>();
        uiBudynkiPanel = uiGry.transform.Find("UI_BudynkiPanel").gameObject;
        rectHpBar = ui_down.transform.Find("DaneGry/PasekZyciaGłównegoBudynku/Green").GetComponent<RectTransform>();
        goPanel = uiGry.transform.Find("GameOver Panel").gameObject;
        epokaNizej.interactable = false;
        epokaWyzej.interactable = false;
        OdpalButtonyAkademii(false);
        UstawPrzyciskObrotu(false);
        przyciskWznów.interactable = false;
        PomocniczeFunkcje.oCam = Camera.main;
        nastepnyPoziom.interactable = false;
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        WłączWyłączPanel(new string[] {goPanel.name, uiBudynkiPanel.name, ui_down.name, uiGry.name, optionsMenu.name,
        reklamyPanel.name, poGraj.name, panelDynamiczny.name, panelStatyczny.name}, false);
    }
    public void OdpalPoScenie(bool czyOdpalamPoScenie)
    {
        if (czyOdpalamPoScenie)
        {
            menu.SetActive(false);
            poGraj.SetActive(true);

            poziomWEpoce.text = PomocniczeFunkcje.odblokowanyPoziomEpoki.ToString();
            actWybEpoka.text = PomocniczeFunkcje.odblokowaneEpoki.ToString();
        }
        else
        {
            menu.SetActive(true);
            poGraj.SetActive(false);
        }
    }
    public void WłączWyłączPanel(string panel, bool czyWłączyć)
    {
        if (panel == menu.name)
        {
            menu.SetActive(czyWłączyć);
        }
        else if (panel == uiGry.name)
        {
            uiGry.SetActive(czyWłączyć);
        }
        else if (panel == optionsMenu.name)
        {
            optionsMenu.SetActive(czyWłączyć);
        }
        else if (panel == poGraj.name)
        {
            poGraj.SetActive(czyWłączyć);
        }
        else if (panel == reklamyPanel.name)
        {
            reklamyPanel.SetActive(czyWłączyć);
        }
        else if (ui_down.name == panel)
        {
            ui_down.SetActive(czyWłączyć);
        }
        else if (goPanel.name == panel)
        {
            goPanel.SetActive(czyWłączyć);
        }
        else if (uiBudynkiPanel.name == panel)
        {
            uiBudynkiPanel.SetActive(czyWłączyć);
        }
    }
    public void WłączWyłączPanel(string[] panel, bool czyWłączyć)
    {
        for (byte i = 0; i < panel.Length; i++)
        {
            if (panel[i] == menu.name)
            {
                menu.SetActive(czyWłączyć);
            }
            else if (panel[i] == uiGry.name)
            {
                uiGry.SetActive(czyWłączyć);
            }
            else if (panel[i] == optionsMenu.name)
            {
                optionsMenu.SetActive(czyWłączyć);
            }
            else if (panel[i] == poGraj.name)
            {
                poGraj.SetActive(czyWłączyć);
            }
            else if (panel[i] == reklamyPanel.name)
            {
                reklamyPanel.SetActive(czyWłączyć);
            }
            else if (ui_down.name == panel[i])
            {
                ui_down.SetActive(czyWłączyć);
            }
            else if (goPanel.name == panel[i])
            {
                goPanel.SetActive(czyWłączyć);
            }
            else if (uiBudynkiPanel.name == panel[i])
            {
                uiBudynkiPanel.SetActive(czyWłączyć);
            }
        }
    }
    private byte DajMiMaxPoziom(string ustawionaEpoka)
    {
        int ustEpoka = int.Parse(ustawionaEpoka);
        if (ustEpoka < 1)
        {
            return 0;
        }
        else
        {
            if (ustEpoka < PomocniczeFunkcje.odblokowaneEpoki)
            {
                return 100;
            }
            else if (ustEpoka == PomocniczeFunkcje.odblokowaneEpoki)
            {
                byte temp = (byte)(PomocniczeFunkcje.odblokowanyPoziomEpoki % 100);
                if (temp == 0)
                {
                    return 1;
                }
                else
                    return temp;
            }
            else
            {
                return 0;
            }
        }
    }
    public void OdpalPoziom()
    {
        byte poziom = (byte)int.Parse(poziomWEpoce.text);
        PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki = poziom;
        poziom = (byte)int.Parse(actWybEpoka.text);
        Epoki e = Epoki.None;
        e += poziom;
        PomocniczeFunkcje.managerGryScript.aktualnaEpoka = e;
        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene(poziom, LoadSceneMode.Additive);
        }
        else
        {
            //Reset scene
            ResetSceny();
        }
        PomocniczeFunkcje.oCam.transform.position = MoveCameraScript.bazowePolozenieKameryGry;
        lastPosCam = MoveCameraScript.bazowePolozenieKameryGry;
        poGraj.SetActive(false);
        PrzełączUI(false);
    }
    public void UstawEpokeWyzejNizej(bool czyWyzej)
    {
        int actEpok = int.Parse(actWybEpoka.text);
        if (czyWyzej)
        {
            if (actEpok < PomocniczeFunkcje.odblokowaneEpoki)
            {
                actEpok++;
                actWybEpoka.text = actEpok.ToString();
                poziomWEpoce.text = DajMiMaxPoziom(actEpok.ToString()).ToString();
            }
        }
        else
        {
            if (actEpok > 1)
            {
                actEpok--;
                actWybEpoka.text = actEpok.ToString();
                poziomWEpoce.text = DajMiMaxPoziom(actEpok.ToString()).ToString();
            }
        }
    }
    public void LVLNizejWyzej(bool czyWyzej)
    {
        int t = int.Parse(poziomWEpoce.text);
        if (czyWyzej)
        {
            t++;
            if (t <= DajMiMaxPoziom(actWybEpoka.text))
            {
                poziomWEpoce.text = t.ToString();
            }
        }
        else
        {
            t--;
            if (t > 0)
            {
                poziomWEpoce.text = t.ToString();
            }
        }
    }
    public void ResetSceny()
    {
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(1);
        PomocniczeFunkcje.managerGryScript.CzyScenaZostałaZaładowana = false;
        SceneManager.LoadSceneAsync((byte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka, LoadSceneMode.Additive);
    }
    public void OptionsMenu(bool actButton)
    {
        menu.SetActive(!actButton);
        optionsMenu.SetActive(actButton);
        if (!actButton)
        {
            PomocniczeFunkcje.ZapisDanychOpcje();
        }
    }
    public void QuitGame()
    {
        PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    public void PrzełączUI(bool aktywujeMenu)
    {
        przyciskWznów.interactable = true;
        czyMenuEnable = aktywujeMenu;
        menu.SetActive(aktywujeMenu);
        uiGry.SetActive(!aktywujeMenu);
        if (aktywujeMenu)
        {
            PomocniczeFunkcje.UstawTimeScale(0);
            lastPosCam = PomocniczeFunkcje.oCam.transform.position;
            PomocniczeFunkcje.oCam.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        }
        else
        {
            PomocniczeFunkcje.UstawTimeScale(1);
            PomocniczeFunkcje.oCam.transform.position = lastPosCam;
        }
    }
    public void SkrzynkaKlik(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KliknietyPrzycisk();
        buttonSkrzynki[idx].skrzynkaB.interactable = false;
    }
    public void KliknietyPrzyciskRewardZPoziomuZReklama()
    {
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        PomocniczeFunkcje.managerGryScript.KliknietyButtonZwiekszeniaNagrodyPoLvlu();
    }
    public void WybierzWybranyPrzedmiot(int nagroda)
    {
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[nagroda].ilośćDanejNagrody > 0)
        {
            wybranaNagroda = (sbyte)nagroda;
        }
        UżyjKlikniętegoPrzedmiotu();
        /*
        if (this.wybórPrzedmiotuZEkwipunku.value > 0)
        {
            wybranyPrzedmiot = (sbyte)(this.wybórPrzedmiotuZEkwipunku.value - 1);   //Konwert na tablice przedmiotów
            użyjPrzedmiotu.gameObject.SetActive(true);
        }
        */
    }
    public void UstawDropDownEkwipunku(ref PrzedmiotScript[] ps)
    {
        for (byte i = 0; i < przyciskiNagród.Length; i++)
        {
            przyciskiNagród[i].interactable = false;
        }
        for (ushort i = 0; i < ps.Length; i++)
        {
            if (ps[i].ilośćDanejNagrody > 0)
            {
                switch (ps[i].typPrzedmiotu)
                {
                    case TypPrzedmiotu.Coiny:
                        przyciskiNagród[0].interactable = true;
                        przyciskiNagród[0].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                        break;
                    case TypPrzedmiotu.CudOcalenia:
                        przyciskiNagród[1].interactable = true;
                        przyciskiNagród[1].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                        break;
                    case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:
                        przyciskiNagród[3].interactable = true;
                        przyciskiNagród[3].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                        break;
                    case TypPrzedmiotu.DodatkowaNagroda:
                        przyciskiNagród[2].interactable = true;
                        przyciskiNagród[2].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                        break;
                }
            }
        }
    }
    public void UżyjKlikniętegoPrzedmiotu()
    {

        if (wybranaNagroda > -1)
        {
            PomocniczeFunkcje.managerGryScript.UzyciePrzedmiotu((byte)wybranaNagroda);
        }
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[wybranaNagroda].ilośćDanejNagrody == 0)
            wybranaNagroda = -1;
    }
    public void UstawButtonNagrody(byte idxButtonaNagrody, ushort ilość)  //Wyszukuje nazwę w opcjach dropdawna
    {
        bool czyAktywuje = (ilość <= 0) ? false : true;
        if (przyciskiNagród[idxButtonaNagrody].interactable != czyAktywuje)
        {
            przyciskiNagród[idxButtonaNagrody].interactable = czyAktywuje;
        }
        przyciskiNagród[idxButtonaNagrody].GetComponentInChildren<Text>().text = ilość.ToString();
    }
    public void KliknąłemReklame(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KlikniętaReklamaButtonSkrzynki((byte)idx);
    }
    public void ZmieńJęzyk()
    {
        lastIdxJezyka++;
        if (lastIdxJezyka < 0 || lastIdxJezyka > 1)  //Tu należy zmienić liczbę jesli dodany zostanie nowy jezyk
            lastIdxJezyka = 0;
        PomocniczeFunkcje.managerGryScript.ZmianaJęzyka((byte)lastIdxJezyka);
    }
    public void ObrótBudynku()
    {
        PomocniczeFunkcje.spawnBudynki.ObróćBudynek();
    }
    public void UstawPrzyciskObrotu(bool wartośćPrzycisku)
    {
        rotacjaBudynku.gameObject.SetActive(wartośćPrzycisku);
    }
    public void KliknalemButtonRozwoju(int indeksButtonu)   //1 - Zycie, 2 - Atak, 3 - Obrona
    {
        PomocniczeFunkcje.managerGryScript.RozwójBudynkow((byte)indeksButtonu);
    }
    public void UstawHPGłównegoPaska(float wartoscX)
    {
        rectHpBar.localScale = new Vector3(wartoscX, 1, 1);
    }
    public void OdpalButtonyAkademii(bool czyOdpalac = true)
    {
        if (buttonAZycie.gameObject.activeInHierarchy != czyOdpalac)
        {
            buttonAZycie.gameObject.SetActive(czyOdpalac);
            buttonAAtak.gameObject.SetActive(czyOdpalac);
            buttonAObrona.gameObject.SetActive(czyOdpalac);
            if (ManagerGryScript.iloscCoinów < 200)
            {
                buttonAZycie.interactable = false;
                buttonAAtak.interactable = false;
                buttonAObrona.interactable = false;
            }
            else
            {
                buttonAZycie.interactable = true;
                buttonAAtak.interactable = true;
                buttonAObrona.interactable = true;
            }
        }
    }
    public void UstawTextUI(string nazwaTekstu, string tekst)
    {
        if (nazwaTekstu == "ilośćCoinów")
        {
            ilośćCoinów.text = tekst;
        }
        else if (nazwaTekstu == "ilośćFal")
        {
            ilośćFal.text = tekst;
        }
    }
    public void UstawPanelUI(string parametry, Vector2 pos, KonkretnyNPCStatyczny knpcs = null)
    {
        if (odpalonyPanel)
        {
            panelDynamiczny.gameObject.SetActive(false);
            panelStatyczny.gameObject.SetActive(false);
            odpalonyPanel = false;
        }
        if (parametry == "")
        {
            return;
        }
        string[] s = parametry.Split('_');
        if (s[0] == "STATYCZNY")
        {
            PanelStatyczny ps = (PanelStatyczny)panelStatyczny;
            ps.KNPCS = knpcs;
            RectTransform r = ps.GetComponent<RectTransform>();
            for (byte i = 1; i < s.Length; i++)
            {
                switch (i)
                {
                    case 1: //Czy odblokować button
                        if (s[i] == "True")  //Odblokuj naprawe budynku
                        {
                            ps.naprawButton.interactable = true;
                        }
                        else
                        {
                            ps.naprawButton.interactable = false;
                        }
                        break;
                    case 2: //Nazwa obiektu
                        ps.nazwaObiektu.text = s[i];
                        break;
                    case 3: //Punkty życia
                        ps.punktyZycia.text = s[i];
                        break;
                    case 4: //Koszt naprawy
                        ps.kosztNaprawy.text = s[i];
                        break;
                    case 5: //Obrażenia
                        ps.obrazenia.text = s[i];
                        break;
                    case 6: //Opis
                        ps.opis.text = s[i];
                        break;
                }
            }
            r.position = pos;
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
        else if (s[0] == "DYNAMICZNY")
        {
            PanelDynamiczny ps = (PanelDynamiczny)panelDynamiczny;
            RectTransform r = ps.GetComponent<RectTransform>();
            for (byte i = 1; i < s.Length; i++)
            {
                switch (i)
                {
                    case 1: //Nazwa obiektu
                        ps.nazwaObiektu.text = s[i];
                        break;
                    case 2: //Punkty życia
                        ps.punktyZycia.text = s[i];
                        break;
                    case 3: //Obrażenia
                        ps.obrazenia.text = s[i];
                        break;
                }
            }
            r.position = pos;
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
    }
    public void WłączWyłączPanelBudynków(int idx)
    {
        if (idx == 0)    //Panel z wieżami
        {
            if(idxWież == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 0)
            {
                if (lastPanelEnabledBuildings == 1)
                    EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
                else if (lastPanelEnabledBuildings == 2)
                    EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxWież, true);
            lastPanelEnabledBuildings = 0;
        }
        else if (idx == 1)   //Panel z murkami
        {
            if(idxMurów == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 1)
            {
                if (lastPanelEnabledBuildings == 0)
                    EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
                else if (lastPanelEnabledBuildings == 2)
                    EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxMurów, true);
            lastPanelEnabledBuildings = 1;
        }
        else if (idx == 2)    //Panel z innymi budynkami
        {
            if(idxInne == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 2)
            {
                if (lastPanelEnabledBuildings == 1)
                    EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
                else if (lastPanelEnabledBuildings == 0)
                    EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxInne, true);
            lastPanelEnabledBuildings = 2;
        }
        else    //Wyłącz panel
        {
            if (lastPanelEnabledBuildings == 1)
                EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
            else if (lastPanelEnabledBuildings == 0)
                EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
            else if (lastPanelEnabledBuildings == 2)
                EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            WłączWyłączPanel("UI_BudynkiPanel", false);
            PomocniczeFunkcje.UstawTimeScale(1);
            lastPanelEnabledBuildings = -1;
        }
    }
    private void EnDisButtonsOfBuildingsInPanel(ref ushort[] tabOfBuildToChange, bool willEnable = false)
    {
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        for (ushort i = 0; i < tabOfBuildToChange.Length; i++)
        {
            tab[tabOfBuildToChange[i]].przycisk.gameObject.SetActive(willEnable);
        }
    }
    public void WygenerujIPosortujTablice()
    {
        Button b = Resources.Load<Button>("UI/PrzyciskBudynku");
        Transform tr = GameObject.Find("Canvas/UIGry/UI_BudynkiPanel/RodzicButtonów").transform;
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        List<ushort> murki = null;
        List<ushort> wieże = null;
        List<ushort> inne = null;
        for (ushort i = 0; i < tab.Length; i++)
        {
            Button tb = GameObject.Instantiate(b);
            tb.transform.SetParent(tr);
            tab[i].DajButton(ref tb);
            if (tab[i].czyZablokowany)
            {
                tb.interactable = false;
            }
            switch (PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[tab[i].indexBudynku].GetComponent<KonkretnyNPCStatyczny>().typBudynku)
            {
                case TypBudynku.Mur:
                    if (murki == null)
                        murki = new List<ushort>();
                    murki.Add(i);
                    break;
                case TypBudynku.Wieża:
                    if (wieże == null)
                        wieże = new List<ushort>();
                    wieże.Add(i);
                    break;
                default:
                    if (inne == null)
                        inne = new List<ushort>();
                    inne.Add(i);
                    break;
            }
            tb.gameObject.SetActive(false);
            if (murki != null)
                idxMurów = murki.ToArray();
            if (wieże != null)
                idxWież = wieże.ToArray();
            if (inne != null)
                idxInne = inne.ToArray();
        }
    }
    public void UstawGłośność()
    {
        PomocniczeFunkcje.muzyka.UstawGłośnośćGry(sliderDźwięku.value);
    }
    public void WyłączPanelBudynków()
    {
        WłączWyłączPanelBudynków(-1);
        PomocniczeFunkcje.spawnBudynki.AktIdxBudZab = -1;
        WłączWyłączPanel(uiBudynkiPanel.name, false);
    }
    public void WłWyłPanelReklam(bool czyWłPanel)
    {
        menu.SetActive(!czyWłPanel);
        reklamyPanel.SetActive(czyWłPanel);
    }
}
