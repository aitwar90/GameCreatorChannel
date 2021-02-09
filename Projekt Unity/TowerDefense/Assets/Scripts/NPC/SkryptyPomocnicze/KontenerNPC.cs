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
public class StrukturaDrzewa : IteratorForTreeBuildings
{
    public StrukturaDrzewa PxPz;
    public StrukturaDrzewa PxMz;
    public StrukturaDrzewa MxPz;
    public StrukturaDrzewa MxMz;
    public Component komponentGałęzi;
    public Vector3 pozycjaGałęzi;
    private byte lastLeaf = 0;
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
    /*
    Obsługa iteratora po drzewie pozycji
    */
    public StrukturaDrzewa GetNextLeaf()
    {
        StrukturaDrzewa tSD = null;
        switch (lastLeaf)
        {
            case 0:
                tSD = PxPz;
                break;
            case 1:
                tSD = PxMz;
                break;
            case 2:
                tSD = MxPz;
                break;
            case 3:
                tSD = MxMz;
                break;
        }
        lastLeaf++;
        return tSD;
    }
    public bool HasMore()
    {
        if (lastLeaf == 4)
            return false;
        return true;
    }
    /*
    Metoda leci po wszystkich elementach drzewa
    */
    public void ExecuteAll(byte doSmoething = 0)
    {
        do
        {
            StrukturaDrzewa tSD = GetNextLeaf();
            if (tSD != null)
            {
                tSD.ExecuteAll(doSmoething);
            }
        }
        while (HasMore());
        if (ReferenceEquals(komponentGałęzi.GetType(), typeof(KonkretnyNPCStatyczny)))
        {
            KonkretnyNPCStatyczny knpcs = (KonkretnyNPCStatyczny)komponentGałęzi;
            switch (doSmoething)
            {
                case 0: //Heal me
                knpcs.HealMe();
                break;
                case 1: //UpgradeMe HP
                knpcs.UpgradeMe(0);
                break;
                case 2: //UpgradeMe Atak
                knpcs.UpgradeMe(1);
                break;
                case 3: //UpgradeMe Defence
                knpcs.UpgradeMe(1);
                break;
            }
        }
    }
}
public interface IteratorForTreeBuildings   //Interfejs służący do iteratora po drzewie pozycji budynków
{
    StrukturaDrzewa GetNextLeaf();
    bool HasMore();
    void ExecuteAll(byte doSmoething);  //Określenie metody jaka ma zostać wywołana
}
public class MagazynObiektówAtaków
{
    private Vector3 dotPos;
    private Vector3 sPos;
    private Transform objInstatiate;

    public MagazynObiektówAtaków(float _docelowyX, float _docelowyZ, float _startowyX, float _startowyY, float _startowyZ, Transform _objInstatiate)
    {
        dotPos.y = 0.25f;
        sPos = new Vector3(_startowyX, _startowyY, _startowyZ);
        objInstatiate = _objInstatiate;
        ActivateObj(_docelowyX, _docelowyZ);
    }
    public void SetActPos(float f)
    {
        objInstatiate.position = Vector3.Lerp(dotPos, sPos, f);
    }
    public void ActivateObj(float x, float z)
    {
        objInstatiate.gameObject.SetActive(true);
        dotPos.x = x;
        dotPos.z = z;
        objInstatiate.rotation = Quaternion.LookRotation(dotPos);
    }
    public void DeactivateObj()
    {
        objInstatiate.position = sPos;
        objInstatiate.gameObject.SetActive(false);
    }
}
[System.Serializable]
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
public interface ICzekajAz
{
    void MetodaDoOdpaleniaPoWyczekaniu();
    System.Collections.IEnumerator CzekajAz();
}



