using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCClass : MonoBehaviour
{
    #region Zmienne publiczne
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
    public float szybkośćAtaku = 3.0f;
    #endregion

    #region Zmienny prywatne

    #endregion

    #region Zmienne chronione
    public short aktualneŻycie = 0;
    public NastawienieNPC nastawienieNPC;
    protected Renderer mainRenderer;
    protected float aktualnyReuseAtaku = 0.0f;
    protected Queue<NPCClass> kolejkaAtaku = null;
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
    #endregion
    void Awake()
    {
        mainRenderer = this.transform.GetComponent<Renderer>();
        this.aktualneŻycie = this.maksymalneŻycie;
    }
    void OnGUI()
    {
        RysujHPBar();
    }
    void Update()
    {
        if (aktualneŻycie <= 0)
        {
            UsuńJednostkę();
        }
    }
    protected abstract void RysujHPBar();
    protected abstract void UsuńJednostkę();
    protected IEnumerator SkasujObject(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
    public virtual void ZmianaHP(short deltaHP)
    {
        this.aktualneŻycie += deltaHP;
        if (aktualneŻycie > maksymalneŻycie)
            aktualneŻycie = maksymalneŻycie;
        else if (aktualneŻycie < 0)
            aktualneŻycie = 0;
    }
    protected void DodajNPCDoKolejkiAtaku(ref NPCClass knpcs)
    {
        if (kolejkaAtaku == null)
        {
            kolejkaAtaku = new Queue<NPCClass>();
        }
        kolejkaAtaku.Enqueue(knpcs);
    }
    protected void OdepnijOdKolejkiAtaku(ref NPCClass knpcs)
    {
        if(kolejkaAtaku == null)
        {
            return;
        }
        ushort i = 0;
        while(kolejkaAtaku.Count > i)
        {
            NPCClass temp = kolejkaAtaku.Dequeue() as NPCClass;
            if(temp != knpcs)
            {
                kolejkaAtaku.Enqueue(temp);
            }
            i++;
        }
    }
}
