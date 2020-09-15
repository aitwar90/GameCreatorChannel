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


