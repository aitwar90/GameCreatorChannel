using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MuzykaScript : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource muzykaTła;
    [Tooltip("Nazwa musi mieć szablon x_y przy czym x oznacza przeznaczenie clipu zaś y kolejny numer np. Tło_1 oznacza pierwszy utwór przeznaczony dla tła. Typy: Tło, AmbientWGrze, AtakNPCDystans, AtakNPCZwarcie, AtakBJeden, AtakBObszar, AtakBAll, ŚmiercNPC, ŚmiercB, Poruszanie")]
    public StrukturaAudio[] clipyAudio;
    public delegate void UstawGłośność(sbyte wartość);
    public UstawGłośność ustawGłośność;

    void Awake()
    {
        if (clipyAudio != null && clipyAudio.Length > 0)
        {
            ustawGłośność += UstawGłośnośćTła;
            muzykaTła.clip = clipyAudio[0].clip;
            muzykaTła.Play();
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
    /*
    public AudioClip ZwróćSzukanyClip(string typ)
    {

    }
    */
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
