using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCClass : MonoBehaviour
{
    #region Zmienne publiczne
    [Tooltip("Maksymalne życie jednostki")]
    public ushort maksymalneŻycie = 100;
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
    #endregion

    #region Zmienny prywatne

    #endregion

    #region Zmienne chronione
    protected ushort aktualneŻycie = 0;
    public NastawienieNPC nastawienieNPC;
    protected Renderer mainRenderer;
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
    public virtual void OtrzymujeObrażenia(short ilosćObrażeń)
    {
        
    }

}
