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
    Reszta
}
public class StrukturaDrzewa
{
    public StrukturaDrzewa lewaGałąź;
    public StrukturaDrzewa prawaGałąź;
    public Component komponentGałęzi;
    public Vector3 pozycjaGałęzi;
    public float odległośćOdPunktu;
    public StrukturaDrzewa()
    {

    }
    public StrukturaDrzewa(Component _komponent, Vector3 _pozycja, float _odległośćOdPunktu)
    {
        lewaGałąź = null;
        prawaGałąź = null;
        komponentGałęzi = _komponent;
        pozycjaGałęzi = _pozycja;
        odległośćOdPunktu = _odległośćOdPunktu;
    }
}


