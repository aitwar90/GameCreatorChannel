using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCClass : MonoBehaviour
{
    #region Zmienne publiczne
    [Tooltip("Nazwa jednostki")]
    public string nazwa = "";
    [Tooltip("Maksymalne życie jednostki")]
    public short maksymalneŻycie = 100;
    [Tooltip("Informacja o tym, czy jednostka walczy w zwarciu czy na odległość, jeśli w ogóle")]
    public TypNPC typNPC;
    [Tooltip("Epoka z której jednostka pochodzi")]
    public Epoki epokaNPC;
    [Tooltip("Koszt danej jednostki")]
    public ushort kosztJednostki = 1;
    [Tooltip("Obrażenia jakie jednostka zadaje w walce")]
    public byte zadawaneObrażenia = 10;
    [Tooltip("Zasięg ataku jednostki")]
    public byte zasięgAtaku = 0;
    [Tooltip("Czas między kolejnymi tikami ataku npc")]
    public float szybkośćAtaku = 0.5f;
    [Tooltip("Mnożnik otrzymywanych obrażeń przez jednostkę")]
    public float modyfikatorOtrzymywanychObrażeń = 1.0f;
    [Tooltip("Mnożnik zadawanych obrażeń przez jednostkę")]
    public float modyfikatorZadawanychObrażeń = 1.0f;
    public NPCClass cel = null;
    public AudioSource odgłosyNPC = null;
    [Tooltip("Tag rodzaj do dźwięków jest dokładniejszym określeniem jaki rodzaj powinien zostać odtworzony z bazy dźwięków. Przykład _łuk")]
    public string tagRodzajDoDźwięków;
    [Tooltip("Poziom na którym budynek może zostać odblokowany, lub wróg móc pojawić")]
    public byte poziom = 1;

    #endregion

    #region Zmienny prywatne

    #endregion

    #region Zmienne chronione
    [SerializeField] private short aktualneŻycie = 32767;
    public NastawienieNPC nastawienieNPC;
    protected Renderer mainRenderer;
    protected float aktualnyReuseAtaku = 0.0f;
    protected bool nieŻyję = false;
    #endregion

    #region Getery i setery
    public NastawienieNPC NastawienieNonPlayerCharacter
    {
        get
        {
            return nastawienieNPC;
        }
        set
        {
            this.nastawienieNPC = value;
        }
    }
    public short AktualneŻycie
    {
        get
        {
            return aktualneŻycie;
        }
        set
        {
            aktualneŻycie = value;
        }
    }
    public bool NieŻyję
    {
        get
        {
            return nieŻyję;
        }
        set
        {
            nieŻyję = value;
        }
    }
    #endregion
    protected virtual void Awake()
    {
        if (mainRenderer == null)
            mainRenderer = this.transform.GetComponent<Renderer>();
        if (mainRenderer == null)
        {
            mainRenderer = this.GetComponentInChildren<Renderer>();
        }
        if (odgłosyNPC == null)
        {
            odgłosyNPC = this.gameObject.AddComponent<AudioSource>();
        }
        UstawGłośnośćNPC(PomocniczeFunkcje.muzyka.ZwrócVol);
        PomocniczeFunkcje.muzyka.ustawGłośność += UstawGłośnośćNPC;
    }
    void Update()
    {
        if (aktualneŻycie == 0 && nieŻyję)
        {
            UsuńJednostkę();
        }
        else
        {
            UpdateMe();
        }
    }
    ///<summary>Odświeża pasek HP NPC.</summary>
    protected abstract void RysujHPBar();
    ///<summary>Obsługuje usunięcie NPC. Zwalnia pamięć i usuwa obiekt lub dodaje go do pullingu.</summary>
    protected abstract void UsuńJednostkę();
    ///<summary>Odpala i ustawia panel obiektu NPC (Panel Statyczny, Dynamiczny i Info o budynku w panelu Budynków).</summary>
    ///<param name="pos">Pozycja, gdzie ma zostać odpalony panel z budynkami. Podanie jako parametr Vector2.negativeInfinity odpala Panel z budynkami.</param>
    public abstract void UstawPanel(Vector2 pos);
    protected virtual IEnumerator SkasujObject(float time)
    {
        yield return new WaitForSeconds(time);
        if (this.odgłosyNPC.isPlaying)
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref odgłosyNPC, false);
        Destroy(this.gameObject);
    }
    ///<summary>Zmienia wartość HP obiektu.</summary>
    ///<param name="deltaHP">Wartość określająca zmianę NPC. Parametr mniejszy od 0 leczy jednostkę.</param>
    public virtual void ZmianaHP(short deltaHP)
    {
        if (!nieŻyję)
        {
            if (deltaHP < 0)
            {
                deltaHP = (short)Mathf.CeilToInt(deltaHP * modyfikatorOtrzymywanychObrażeń);
            }
            this.aktualneŻycie -= deltaHP;
            if (aktualneŻycie > maksymalneŻycie)
                aktualneŻycie = maksymalneŻycie;
            if (aktualneŻycie <= 0)
            {
                aktualneŻycie = 0;
                nieŻyję = true;
            }
            RysujHPBar();
        }
    }
    ///<summary>Wywołanie metody obsługuje atak jednostki.</summary>
    ///<param name="wZwarciu">Czy NPC atakuje w zwarciu.</param>
    public abstract void Atakuj(bool wZwarciu);
    ///<summary>Odbija obrażenia w atakującego.</summary>
    public virtual byte ZwrócOdbiteObrażenia()
    {
        return 0;
    }
    ///<summary>Wylicza średnią granicę wielkości obiektu i zwraca ją.</summary>
    public virtual float PobierzGranice()
    {
        return 0.2f;
    }
    protected virtual void UpdateMe()
    {

    }
    ///<summary>Wywołanie metody wymusza ponowne wyliczenie ścieżki agenta dla jednostki.</summary>
    ///<param name="taWiezaPierwszyRaz">Czy wymuszenie ścieżki jest spowodowane postawieniem przez gracza budynku.</param>
    public virtual void ResetujŚciezkę(KonkretnyNPCStatyczny taWiezaPierwszyRaz = null)
    {

    }
    #region Ustawianie zmiennych w animatorze
    ///<summary>Prześlij parametry potrzebne do obsługi animacji do Unity.</summary>
    ///<param name="paramsT">Tablica nazw parametrów, które mają ulec zmianie.</param>
    ///<param name="values">Tablica wartości dla parametrów.</param>
    public void ObsluzAnimacje(string[] paramsT, bool[] values)
    {
        if (ReferenceEquals(this.GetType(), typeof(KonkretnyNPCStatyczny)))
        {
            KonkretnyNPCDynamiczny knpcd = (KonkretnyNPCDynamiczny)this;
            Animator anim = null;
            anim = knpcd.GetAnimator;
            if (anim != null)
            {
                for (byte i = 0; i < paramsT.Length; i++)
                {
                    anim.SetBool(paramsT[i], values[i]);
                }
            }
            else
            {
                Debug.Log("Animator nie został załadowany");
            }
        }
        else
        {
            Debug.Log("Nie odnalazlem typu");
        }
    }
    ///<summary>Prześlij parametry potrzebne do obsługi animacji do Unity.</summary>
    ///<param name="param">Nazwa parametru, który ma ulec zmianie.</param>
    ///<param name="value">Wartość, na jaką ma zostać zmieniony parametr.</param>
    public void ObsluzAnimacje(string param, bool value)
    {
        if (this.GetType() == typeof(KonkretnyNPCDynamiczny))
        {
            //if (ReferenceEquals(this.GetType(), typeof(KonkretnyNPCDynamiczny)))
            //{
            KonkretnyNPCDynamiczny knpcd = (KonkretnyNPCDynamiczny)this;
            Animator anim = null;
            anim = knpcd.GetAnimator;

            if (anim != null)
            {
                anim.SetBool(param, value);
                if (param == "isDeath")
                {
                    UstawMiWartośćParametru(0, value);
                }
                else if (param == "haveTarget")
                {
                    UstawMiWartośćParametru(1, value);
                }
                else if (param == "inRange")
                {
                    UstawMiWartośćParametru(2, value);
                }
            }
            else
            {
                Debug.Log("Animator nie został zadałdoany");
            }
        }
        else
        {
            Debug.Log("Nie odnalazlem typu");
        }
    }
    ///<summary>Prześlij parametry potrzebne do obsługi animacji do Unity.</summary>
    ///<param name="anima">Animator komponent, w którym mają być modyfikowane parametry.</param>
    ///<param name="param">Nazwa parametru, który ma ulec zmianie.</param>
    ///<param name="value">Wartość, na jaką ma zostać zmieniony parametr.</param>
    public void ObsluzAnimacje(ref Animator anima, string param, bool value)
    {
        if (anima != null)
        {
            anima.SetBool(param, value);
            if (param == "isDeath")
            {
                UstawMiWartośćParametru(0, value);
            }
            else if (param == "haveTarget")
            {
                UstawMiWartośćParametru(1, value);
            }
            else if (param == "inRange")
            {
                UstawMiWartośćParametru(2, value);
            }
        }
        else
        {
            Debug.Log("Animator nie został zadałdoany");
        }
    }
    ///<summary>Prześlij parametry potrzebne do obsługi animacji do Unity.</summary>
    ///<param name="anima">Animator komponent, w którym mają być modyfikowane parametry.</param>
    ///<param name="param">Nazwa parametru, który ma ulec zmianie.</param>
    ///<param name="value">Wartość, na jaką ma zostać zmieniony parametr.</param>
    public void ObsluzAnimacje(ref Animator anima, string param, int value)
    {
        if (anima != null)
        {
            anima.SetInteger(param, value);
        }
        else
        {
            Debug.Log("Animator nie został zadałdoany");
        }
    }
    ///<summary>Prześlij parametry potrzebne do obsługi animacji do Unity.</summary>
    ///<param name="anima">Animator komponent, w którym mają być modyfikowane parametry.</param>
    ///<param name="param">Nazwa parametru, który ma ulec zmianie.</param>
    ///<param name="value">Wartość, na jaką ma zostać zmieniony parametr.</param>
    public void ObsluzAnimacje(ref Animator anima, string param, float value)
    {
        if (anima != null)
        {
            anima.SetFloat(param, value);
        }
        else
        {
            Debug.Log("Animator nie został zadałdoany");
        }
    }
    ///<summary>Ustaw opis do panelu budynku w przypadku zmiany języka przez gracza.</summary>
    ///<param name="coZmieniam">Określa co ma zostać zmienione.</param>
    ///<param name="podmianaWartosci">Wartość ma zostać przypisana dla podanego parametru coZmieniam.</param>
    public virtual void UstawJezykNPC(string coZmieniam, string podmianaWartosci)
    {
        if (coZmieniam == "nazwa")
        {
            this.nazwa = podmianaWartosci;
            return;
        }
    }
    ///<summary>Ustaw wartość głośności AudioSource NPC.</summary>
    ///<param name="wartość">Wartość zostanie przypisana do AudioSource NPC (0-1).</param>
    public void UstawGłośnośćNPC(float wartość)
    {
        odgłosyNPC.volume = wartość;
    }
    ///<summary>Zwróć stan parametru z AnimatorController podanego w parametrze.</summary>
    ///<param name="i">Parametr, którego wartość ma zostać zwrócona: (0-isDeath, 1-haveTarget, 2-inRange).</param>
    public virtual sbyte ZwróćMiWartośćParametru(byte i)
    {
        return -1;  //Zwrócenie parametru z animationController
    }
    ///<summary>Ustawia wartość parametru w AnimatorController.</summary>
    ///<param name="parametr">Parametr, którego wartość ma zostać przypisana: (0-isDeath, 1-haveTarget, 2-inRange).</param>
    ///<param name="value">Przypisanie tej wartości zadanemu parametrowi.</param>
    protected virtual void UstawMiWartośćParametru(byte parametr, bool value)
    {
        //Ustawienie parametru w animation controller
    }
    #endregion

}
