﻿using System.Collections;
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
        if(mainRenderer == null)
        {
            mainRenderer = this.GetComponentInChildren<Renderer>();
        }
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
    protected abstract void RysujHPBar();
    protected abstract void UsuńJednostkę();
    protected virtual IEnumerator SkasujObject(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
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
    public abstract void Atakuj(bool wZwarciu);

    public virtual byte ZwrócOdbiteObrażenia()
    {
        return 0;
    }
    public virtual float PobierzGranice()
    {
        return 0.2f;
    }
    protected virtual void UpdateMe()
    {

    }
    public virtual void ResetujŚciezkę(KonkretnyNPCStatyczny taWiezaPierwszyRaz = null)
    {

    }
    #region Ustawianie zmiennych w animatorze
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
                Debug.Log("Animator nie został zadałdoany");
            }
        }
        else
        {
            Debug.Log("Nie odnalazlem typu");
        }
    }
    public void ObsluzAnimacje(string param, bool value)
    {
        if (ReferenceEquals(this.GetType(), typeof(KonkretnyNPCDynamiczny)))
        {
            KonkretnyNPCDynamiczny knpcd = (KonkretnyNPCDynamiczny)this;
            Animator anim = null;
            anim = knpcd.GetAnimator;
            if (anim != null)
            {
                anim.SetBool(param, value);
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
    public void ObsluzAnimacje(ref Animator anima, string param, bool value)
    {
        if (anima != null)
        {
            anima.SetBool(param, value);
        }
        else
        {
            Debug.Log("Animator nie został zadałdoany");
        }
    }
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
    #endregion

}
