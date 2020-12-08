using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MuzykaScript : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource muzykaTła;
    [Tooltip("Nazwa musi mieć szablon x przy czym x oznacza przeznaczenie clipu zaś y kolejny numer np. Tło oznacza pierwszy utwór przeznaczony dla tła. Typy: Tło, AmbientWGrze, AtakNPCDystans, AtakNPCZwarcie, AtakBJeden, AtakBObszar, AtakBAll, ŚmiercNPC, ŚmiercB, Poruszanie")]
    public StrukturaAudio[] clipyAudio;
    public delegate void UstawGłośność(sbyte wartość);
    public UstawGłośność ustawGłośność;
    private byte[] indeksyMuzyki = null;

    void Awake()
    {
        if (clipyAudio != null && clipyAudio.Length > 0)
        {
            ustawGłośność += UstawGłośnośćTła;
        }
    }
    void Start()
    {
        string lastString = null;
        if (clipyAudio != null && clipyAudio.Length > 0)
        {
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
            WłączWyłączClip(ref muzykaTła, true, "Tło", false);
        }
    }
    public void UstawGłośnośćGry(int val)   //Ta metoda powinna być wywoływana w opcjach aplikacji
    {
        if (val > 100)
        {
            ustawGłośność(100);
        }
        else if (val < 0)
        {
            ustawGłośność(0);
        }
        else
        {
            ustawGłośność((sbyte)val);
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
    public void WłączWyłączClip(string typ, ref AudioSource ado, bool czyOneShoot = false, string nazwaAktualnegoKlipu = "") //Ta metoda pozwala na wybranie klipu z wyłączeniem nazwy aktualnie granej
    {
        ado.clip = ZwróćSzukanyClip(typ, nazwaAktualnegoKlipu);
        if (czyOneShoot)
        {
            ado.PlayOneShot(ado.clip);
        }
        else
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
                    if (indeksyMuzyki[i] == indeksyMuzyki.Length - 1)  //Jeśli typ jest jedyny i ostatni w tablicy
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
                            x = (byte)Random.Range(indeksyMuzyki[i], indeksyMuzyki[i + 1] - 1);
                        } while (clipyAudio[x].clip.name != nazwaAktualnegoKlipu);
                        return clipyAudio[x].clip;
                    }
                }
            }
        }
        return null;
    }
    public void UstawGłośnośćTła(sbyte wartość)
    {
        muzykaTła.volume = wartość / 100.0f;
    }
    public void SortujAlfabetyczniePoNazwie()
    {
        IComparer comp = new SortujWGNazwy();
        System.Array.Sort(clipyAudio, comp);
    }
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
