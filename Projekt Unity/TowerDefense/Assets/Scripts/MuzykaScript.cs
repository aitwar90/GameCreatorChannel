using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEngine.Events;
#endif
public class MuzykaScript : MonoBehaviour, ICzekajAz
{
    // Start is called before the first frame update
    public static MuzykaScript singleton = null;
    public AudioSource muzykaTła;
    [Tooltip("Nazwa musi mieć szablon x_y_z przy czym x oznacza przeznaczenie clipu zaś y epokę. Tło oznacza pierwszy utwór przeznaczony dla tła. Typy: Tło, AmbientWGrze, AtakNPCDystans, AtakNPCZwarcie, AtakBJeden, AtakBObszar, AtakBAll, ŚmiercNPC, ŚmiercB, Poruszanie, Idle, TrafienieB, TrafienieNPC, PostawB. Przykład AtakBObszar_EpokaKamienia_Kamień (W przypadku tła i ambient nie ma specjalnych rodzajów)")]
    public StrukturaAudio[] clipyAudio;
    public delegate void UstawGłośność(float wartość);
    public UstawGłośność ustawGłośność;
    public float aktValVolume = 0.0f;
    private byte[] indeksyMuzyki = null;
    public float ZwrócVol
    {
        get
        {
            return aktValVolume;
        }
    }
    void Awake()
    {
        if (singleton == null)
        {
            if (clipyAudio != null && clipyAudio.Length > 0)
            {
                ustawGłośność += UstawGłośnośćTła;
            }
            singleton = this;
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        string lastString = null;
        if (clipyAudio != null && clipyAudio.Length > 0)
        {
            muzykaTła.ignoreListenerPause = true;
            muzykaTła.ignoreListenerVolume = true;
            lastString = clipyAudio[0].nazwa;
            List<byte> tByte = new List<byte>();
            tByte.Add(0);
            for (byte i = 1; i < clipyAudio.Length; i++)
            {
                if (clipyAudio[i].nazwa != lastString)
                {
                    lastString = clipyAudio[i].nazwa;
                    tByte.Add(i);
                }
            }
            indeksyMuzyki = tByte.ToArray();
            WłączWyłączClip(ref muzykaTła, true, PomocniczeFunkcje.TagZEpoka("Tło", Epoki.None), false);
        }
    }
    public void UstawGłośnośćGry(float val)   //Ta metoda powinna być wywoływana w opcjach aplikacji
    {
        aktValVolume = val;
        if (ustawGłośność == null)
        {
            StartCoroutine(CzekajAz());
        }
        else
        {
            ustawGłośność(aktValVolume);
        }
    }
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        ustawGłośność(aktValVolume);
    }
    public IEnumerator CzekajAz()
    {
        yield return new WaitUntil(() => this.ustawGłośność != null);
        MetodaDoOdpaleniaPoWyczekaniu();
        yield return null;
    }
    public void WłączWyłączClip(bool czyWłączyć, string typ = "", bool czyOneShoot = false)
    {
        if (!czyWłączyć)
        {
            muzykaTła.Stop();
        }
        else
        {
            WłączWyłączClip(typ, ref muzykaTła, czyOneShoot);
        }
    }
    public void WłączWyłączClip(ref AudioSource ado, bool czyWłączyć, string typ = "", bool czyOneShoot = false)    //Ta metoda obsługuje całe audio
    {
        if (!czyWłączyć)
        {
            ado.Stop();
        }
        else
        {
            WłączWyłączClip(typ, ref ado, czyOneShoot);
        }
    }
    public void WłączTymczasowyClip(string typ, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(ZwróćSzukanyClip(typ), pos, aktValVolume);
    }
    public void WłączWyłączClip(string typ, ref AudioSource ado, bool czyOneShoot = false, string nazwaAktualnegoKlipu = "") //Ta metoda pozwala na wybranie klipu z wyłączeniem nazwy aktualnie granej
    {
        ado.clip = ZwróćSzukanyClip(typ, nazwaAktualnegoKlipu);
        if (czyOneShoot && ado.clip != null)
        {
            ado.PlayOneShot(ado.clip);
        }
        else if (ado.clip != null)
        {
            ado.Play();
        }
    }
    private AudioClip ZwróćSzukanyClip(string typ, string nazwaAktualnegoKlipu = "")   //Funkcja zwraca klip w zależności od podanego w parametrze typu
    {
        if (indeksyMuzyki != null)
        {
            for (byte i = 0; i < indeksyMuzyki.Length; i++)
            {
                if (clipyAudio[indeksyMuzyki[i]].nazwa == typ)
                {
                    if (indeksyMuzyki[i] == indeksyMuzyki[indeksyMuzyki.Length - 1])  //Jeśli typ jest jedyny i ostatni w tablicy
                    {
                        return clipyAudio[indeksyMuzyki[i]].clip;
                    }
                    else if (indeksyMuzyki[i + 1] - indeksyMuzyki[i] == 1) //Jeśli typ jest jedyny
                    {
                        return clipyAudio[indeksyMuzyki[i]].clip;
                    }
                    else    //Obsługa domyślna
                    {
                        byte x = indeksyMuzyki[i];
                        do
                        {
                            x = (byte)Random.Range(indeksyMuzyki[i], indeksyMuzyki[i + 1]);
                        } while (clipyAudio[x].clip.name == nazwaAktualnegoKlipu);
                        return clipyAudio[x].clip;
                    }
                }
            }
        }
        return null;
    }
    public void UstawGłośnośćTła(float wartość)
    {
        muzykaTła.volume = wartość;
    }
    public void SortujAlfabetyczniePoNazwie()
    {
        IComparer comp = new SortujWGNazwy();
        System.Array.Sort(clipyAudio, comp);
    }
    public void DodajMnieJakoListenerClick()
    {
        WłączTymczasowyClip("KlikUI", muzykaTła.transform.position);
    }
    #if UNITY_EDITOR
    public void DodajDoButtonów()
    {
        UnityEngine.UI.Button[] btn = FindObjectsOfType(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button[];
        for (ushort i = 0; i < btn.Length; i++)
        {
            UnityEvent ue = btn[i].onClick;
            UnityAction action = new UnityAction(DodajMnieJakoListenerClick);
            int ilCout = ue.GetPersistentEventCount();
            bool znalazlem = false;
            for(byte j = 0; j < ilCout; j++)
            {
                if(ue.GetPersistentMethodName(j) == "DodajMnieJakoListenerClick")
                {
                    znalazlem = true;
                    break;
                }
            }
            if(!znalazlem)
                UnityEventTools.AddPersistentListener(ue, action);
        }
    }
    public void UsunZButtonow()
    {
        UnityEngine.UI.Button[] btn = FindObjectsOfType(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button[];
        for (ushort i = 0; i < btn.Length; i++)
        {
            UnityEvent ue = btn[i].onClick;
            UnityAction action = new UnityAction(DodajMnieJakoListenerClick);
            int ilCout = ue.GetPersistentEventCount();
            for(sbyte j = (sbyte)(ilCout-1); j >= 0; j--)
            {
                if(ue.GetPersistentMethodName(j) == "DodajMnieJakoListenerClick")
                {
                    UnityEventTools.RemovePersistentListener(ue, j);
                }
            }
        }
    }
    #endif
}
[System.Serializable]
public struct StrukturaAudio
{
    public string nazwa;
    public AudioClip clip;
}
public class SortujWGNazwy : IComparer
{
    int IComparer.Compare(object x, object y)
    {
        StrukturaAudio sa = (StrukturaAudio)x;
        x = sa.nazwa;
        sa = (StrukturaAudio)y;
        y = sa.nazwa;
        return ((new CaseInsensitiveComparer()).Compare(x, y));
    }
}
