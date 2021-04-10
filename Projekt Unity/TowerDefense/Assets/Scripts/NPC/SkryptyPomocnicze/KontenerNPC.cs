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
    ///<summary>Stworzenie obiektu Informacji dla wież.</summary>
    ///<param name="_odlOdGranicy">Parametr określający odległość mierzoną w polach między środkowym polem na którym stoi wieża, a polem teraz tworzonym.</param>
    ///<param name="_wieża">Wieża do której przynależy pole.</param>
    ///<param name="_strona">Jeśli odległość od granicy jest różna niż -1 to podawane są tu strony graniczne: (-X) = 0, (+X) = 1, (-Z) = 2, (+Z) = 3.</param>
    public InformacjeDlaPolWież(byte _odlOdGranicy, KonkretnyNPCStatyczny _wieża, byte[] _strona = null)
    {
        this.odlOdGranicy = _odlOdGranicy;
        this.wieża = _wieża;
        this.strona = _strona;
    }
    ~InformacjeDlaPolWież()
    {

    }
    ///<summary>Zwraca informację o tym, czy wieża posiada na tym polu graniczną stronę.</summary>
    ///<param name="szukanaStrona">Wskazanie parametru szukane strony. (-X = 0), (+X = 1), (-Z = 2), (+Z = 3)</param>
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
    public bool CzyAktywny
    {
        get
        {
            return objInstatiate.gameObject.activeInHierarchy;
        }
    }

    ///<summary>Stworzenie obiektu ataku wieży.</summary>
    ///<param name="_docelowyX">Pozycja na osi X celu do którego ma dążyć obiekt ataku.</param>
    ///<param name="_docelowyZ">Pozycja na osi Z celu do którego ma dążyć obiekt ataku.</param>
    ///<param name="_startowyX">Pozycja na osi X startowej pozycji obiektu ataku.</param>
    ///<param name="_startowyY">Pozycja na osi Y startowej pozycji obiektu ataku.</param>
    ///<param name="_startowyZ">Pozycja na osi Z startowej pozycji obiektu ataku.</param>
    ///<param name="_objInstatiate">Referencja komponentu Transform generowanego obiektu ataku.</param>
    public MagazynObiektówAtaków(float _docelowyX, float _docelowyZ, float _startowyX, float _startowyY, float _startowyZ, Transform _objInstatiate)
    {
        dotPos.y = 1.0f;
        sPos = new Vector3(_startowyX, _startowyY, _startowyZ);
        objInstatiate = _objInstatiate;
        ActivateObj(_docelowyX, _docelowyZ);
    }
    ///<summary>Aktualizacja danych o startowej pozycji rozpoczynającej atak.</summary>
    ///<param name="przesuniecieLocalX">Pozycja na osi X włóczni względem rodzica.</param>
    ///<param name="przesuniecieLocalZ">Pozycja na osi Z włóczni względem rodzica.</param>
    public void ResetPos(float przesuniecieLocalX, float przesuniecieLocalZ = 1.0f)
    {
        objInstatiate.localPosition = new Vector3(przesuniecieLocalX, 1.0f, przesuniecieLocalZ);
        sPos = new Vector3(objInstatiate.position.x, objInstatiate.position.y, objInstatiate.position.z);
    }
    ///<summary>Ustaw pozycję obiektu ataku</summary>
    ///<param name="f">Parametr 0-1 określający % położenia między docelową a startową pozycją obiektu ataku.</param>
    ///<param name="naprowadźLook">Czy ustawiać rotację?</param>
    public void SetActPos(float f, bool naprowadźLook = false)
    {
        objInstatiate.position = Vector3.Lerp(dotPos, sPos, f);
        if(naprowadźLook)
        {
            objInstatiate.rotation = Quaternion.LookRotation(dotPos - sPos);
        }
    }
    ///<summary>Aktywacja obiektu ataku</summary>
    ///<param name="x">Pozycja na osi X docelowej pozycji obiektu ataku.</param>
    ///<param name="z">Pozycja na osi Z docelowej pozycji obiektu ataku.</param>
    ///<param name="aktywuj">Czy obiekt ma zostać włączony (tylko w przypadku wież).</param>
    public void ActivateObj(float x, float z, bool aktywuj = true)
    {
        if(aktywuj)
            objInstatiate.gameObject.SetActive(true);
        dotPos.x = x;
        dotPos.z = z;
        if(dotPos.x != sPos.x || dotPos.z != sPos.z)
        {
            objInstatiate.rotation = Quaternion.LookRotation(dotPos - sPos);
        }
    }
    public void PrzełączSkalęLokalZ()
    {
        objInstatiate.localScale = new Vector3(objInstatiate.localScale.x, objInstatiate.localScale.y, objInstatiate.localScale.z * -1);
    }
    ///<summary>Dezaktywacja obiektu ataku.</summary>
    public void DeactivateObj()
    {
        objInstatiate.position = sPos;
        objInstatiate.gameObject.SetActive(false);
    }
    ///<summary>Zaktualizuj startową pozycję dla ammo</summary>
    ///<param name="x">Pozycja jaka ma zostać ustawiona dla ammo po osi OX</param>
    ///<param name="y">Pozycja jaka ma zostać ustawiona dla ammo po osi OY</param>
    ///<param name="z">Pozycja jaka ma zostać ustawiona dla ammo po osi OZ</param>
    public void UpdateSrartPos(float x, float y, float z)
    {
        sPos.x = x;
        sPos.y = y;
        sPos.z = z;
    }
    ///<summary>Ustaw pozycję broni po powrocie dla wrogów.</summary>
    public void BackWeapon()
    {
        objInstatiate.localPosition = Vector3.zero;
        objInstatiate.localEulerAngles = Vector3.zero;
    }
}
public class MagazynWZasięguWieży
{
    private MagazynWZasięguWieży parent = null;
    private MagazynWZasięguWieży child = null;
    private KonkretnyNPCDynamiczny nPCDynamiczny;

    ///<summary>Stworzenie obiektu korzenia struktury.</summary>
    ///<param name="_knpcd">Referencja korzenia tworzonej struktury.</param>
    public MagazynWZasięguWieży(ref KonkretnyNPCDynamiczny _knpcd)
    {
        parent = null;
        child = null;
        nPCDynamiczny = _knpcd;
    }
    ///<summary>Stworzenie obiektu struktury (nie roota).</summary>
    ///<param name="_parent">Rodzic tworzonego obiektu.</param>
    ///<param name="_knpcd">Referencja korzenia tworzonej struktury.</param>
    private MagazynWZasięguWieży(MagazynWZasięguWieży _parent, KonkretnyNPCDynamiczny _knpcd)
    {
        parent = _parent;
        nPCDynamiczny = _knpcd;
        _parent.child = this;
    }
    ~MagazynWZasięguWieży() { }
    ///<summary>Usunięcie ze struktury danego w parametrze NPC celów wieży.</summary>
    ///<param name="npc">Referencja kasowanego obiektu ze struktury.</param>
    public MagazynWZasięguWieży DeleteMe(ref NPCClass npc)
    {
        if (ReferenceEquals(npc.GetType(), typeof(KonkretnyNPCDynamiczny)))
        {
            KonkretnyNPCDynamiczny nKnpcd = (KonkretnyNPCDynamiczny)npc;
            if (this.nPCDynamiczny == nKnpcd)
            {
                if (this.parent == null) //To jest root
                {
                    if (this.child != null)
                    {
                        this.child.parent = null;
                        return this.child;
                    }
                    else
                    {
                        return this;
                    }
                }
                else    //To nie jest root
                {
                    if (this.child != null)
                    {
                        this.child.parent = this.parent;
                        this.parent.child = this.child;
                    }
                    else
                    {
                        this.parent.child = null;
                    }
                    return null;
                }
            }
            else    //To nie jest ten node
            {
                if (this.child != null)
                    this.child.DeleteMe(ref npc);
                else
                    return null;
            }
        }
        else
        {
            return null;
        }
        return null;
    }
    ///<summary>Dodaj do struktury Konkretny NPC Dynamiczny jako cele wieży.</param>
    ///<param name="knpcd">Referencja dodawanego obiektu do struktury.</param>
    public void AddMagazyn(ref KonkretnyNPCDynamiczny knpcd)
    {
        if (knpcd == this.nPCDynamiczny) //Jestem już w kolekcji
        {
            return;
        }
        if (this.child == null)
        {
            this.child = new MagazynWZasięguWieży(this, knpcd);
        }
        else
        {
            this.child.AddMagazyn(ref knpcd);
        }
    }
    ///<summary>Zwróć ze struktury ilość obiektów celów dostępnych dla wieży.</summary>
    /// <param name="ilość">Ilość zwracanych obiektów ze struktury.</param>
    public KonkretnyNPCDynamiczny[] ZwróćMiKonkretneNpc(byte ilość = 1)
    {
        byte tIlosc = 0;
        System.Collections.Generic.Stack<KonkretnyNPCDynamiczny> stos = new System.Collections.Generic.Stack<KonkretnyNPCDynamiczny>();
        MagazynWZasięguWieży tMag = this;
        while (tIlosc < ilość)
        {
            tIlosc++;
            stos.Push(tMag.nPCDynamiczny);
            if (tMag.child != null)
            {
                tMag = tMag.child;
            }
            else
            {
                break;
            }
        }
        return stos.ToArray();
    }
}
[System.Serializable]
public class StrukturaBudynkuWTab
{
    public bool czyZablokowany;
    public ushort indexBudynku;
    public UnityEngine.UI.Button przycisk;

    ///<summary>Konstruktor StrukturaBudynkuWTab (określa indeks budynku w tablicy wszystkieBudynki oraz czy budynek jest zablokowany).</summary>
    ///<param name="_czyZablokowany">Czy budynek o zadanym indeksie w tablicy jest zablokowany.</param>
    ///<param name="_idxBudunkuWTablicyBudynków">Indeks budynku w tablicy wszystkieBudynki w SpawnBudynki.cs.</param>
    public StrukturaBudynkuWTab(bool _czyZablokowany, ushort _idxBudunkuWTablicyBudynków)
    {
        this.czyZablokowany = _czyZablokowany;
        this.indexBudynku = _idxBudunkuWTablicyBudynków;
    }
    ///<summary>Metoda ustawia przycisk w panelu budynków i dodaje metodę do obsługi generowanego przycisku.</summary>
    ///<param name="_przycisk">Wygenerowany przycisk do przypisania (RodzicButtonów).</param>
    public void DajButton(ref UnityEngine.UI.Button _przycisk)
    {
        przycisk = _przycisk;
        przycisk.onClick.AddListener(OnClick);
    }
    ///<summary>Metoda przypisywana do przycisku w Panelu Budynków.</summary>
    void OnClick()
    {
        bool f = PomocniczeFunkcje.spawnBudynki.KliknietyBudynekWPanelu((short)indexBudynku);
        if(f)   //Przycisk do ponownego postawienia buttonu
        {
            PomocniczeFunkcje.mainMenu.ostatniStawianyBudynekButton.GetComponent<ObsłużPrzyciskOstatniegoStawianegoBudynku>().PrzypiszDanePrzyciskowi((short)indexBudynku, 
            PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[indexBudynku].GetComponent<KonkretnyNPCStatyczny>().kosztJednostki, 
            przycisk.image.sprite);
        }
    }
    ///<summary>Metoda uznaje że tren przycisk został kliknięty.</summary>
    public void Aktywuj()
    {
        OnClick();
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
[System.Serializable]
public struct FontDlaJezyków
{
    public Font font;
    public byte[] idxJezyka;  //1-Polski, 2-Angielski, 3-Ukraiński
}



