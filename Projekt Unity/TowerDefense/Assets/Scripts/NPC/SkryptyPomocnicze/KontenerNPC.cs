using UnityEngine;

public enum Epoki
{
    None = 0,
    EpokaKamienia = 1,
    EpokaStarożytna = 2,
    EpokaŚredniowiecza = 3,
    EpokaNowożytna = 4,
    EpokaWspołczesna = 5,
    EpokaPrzyszła = 7
}
public enum TypNPC
{
    NieWalczy = 0,
    WalczyWZwarciu = 1,
    WalczyNaDystans = 2,
    WalczynaDystansIWZwarciu = 3
}
public enum NastawienieNPC
{
    Przyjazne = 0,  //Należy do gracza
    Wrogie = 1      //Nalezy do hordy
}
public enum TypBudynku
{
    Mur,
    Wieża,
    Akademia,
    Reszta
}
public enum TypAtakuWieży
{
    None = 0,
    jedenTarget = 1,
    wybuch = 2,
    wszyscyWZasiegu = 3
}
public enum DodatkoweWłaściwościWieży
{
    None = 0,
    Spowolnienie = 1
}
public class InformacjeDlaPolWież
{
    public KonkretnyNPCStatyczny wieża;
    public byte odlOdGranicy;
    private byte[] strona;    //-1 nie jest graniczny, 0 - -X, 1 - +X, 2 - -Z, 3 - +Z
    public InformacjeDlaPolWież()
    {

    }
    public InformacjeDlaPolWież(byte _odlOdGranicy, KonkretnyNPCStatyczny _wieża,
    byte[] _strona = null)
    {
        this.odlOdGranicy = _odlOdGranicy;
        this.wieża = _wieża;
        this.strona = _strona;
    }
    ~InformacjeDlaPolWież()
    {

    }
    public bool ZwrócCzyWieżaPosiadaStrone(byte szukanaStrona)
    {
        if (strona != null)
        {
            for (byte i = 0; i < strona.Length; i++)
            {
                if (strona[i] == szukanaStrona)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
public class StrukturaDoPoolowania
{
    public string nazwa;
    public System.Collections.Generic.List<KonkretnyNPCDynamiczny> listaObiektówPoolingu;

    public StrukturaDoPoolowania()
    {

    }
    ~StrukturaDoPoolowania()
    {

    }
    public StrukturaDoPoolowania(string _nazwa)
    {
        this.nazwa = _nazwa;
    }
    public StrukturaDoPoolowania(string _nazwa, System.Collections.Generic.List<KonkretnyNPCDynamiczny> _listaObiektówPoolingu)
    {
        this.nazwa = _nazwa;
        this.listaObiektówPoolingu = _listaObiektówPoolingu;
    }
}
public class StrukturaDrzewa
{
    public StrukturaDrzewa PxPz;
    public StrukturaDrzewa PxMz;
    public StrukturaDrzewa MxPz;
    public StrukturaDrzewa MxMz;
    public Component komponentGałęzi;
    public Vector3 pozycjaGałęzi;
    public StrukturaDrzewa()
    {

    }
    public StrukturaDrzewa(Component _komponent)
    {
        PxPz = null;
        PxMz = null;
        MxPz = null;
        MxMz = null;
        komponentGałęzi = _komponent;
        pozycjaGałęzi = _komponent.transform.position;
    }
    ~StrukturaDrzewa()
    {

    }
}
public class StrukturaBudynkuWTab
{
    public bool czyZablokowany;
    public ushort indexBudynku;
    public UnityEngine.UI.Button przycisk;
    public StrukturaBudynkuWTab(bool _czyZablokowany, ushort _idxBudunkuWTablicyBudynków)
    {
        this.czyZablokowany = _czyZablokowany;
        this.indexBudynku = _idxBudunkuWTablicyBudynków;
    }
    public void DajButton(ref UnityEngine.UI.Button _przycisk)
    {
        przycisk = _przycisk;
        przycisk.onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        PomocniczeFunkcje.spawnBudynki.KliknietyBudynekWPanelu((short)indexBudynku);
    }
}
[System.Serializable]
public class PrzyciskiSkrzynekIReklam
{
    public UnityEngine.UI.Button skrzynkaB;
    public UnityEngine.UI.Button reklamSkrzynkaB;
}
public enum TypPrzedmiotu
{
    Coiny = 0,
    CudOcalenia = 1,
    SkrócenieCzasuDoSkrzynki = 2,
    DodatkowaNagroda = 3
}



