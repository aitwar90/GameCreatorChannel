using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBudynki : MonoBehaviour
{
    #region Zmienne publiczne
    public GameObject[] wszystkieBudynki;
    public Transform rodzicBudynkow = null;

    //UI _ Canvas
    public UnityEngine.UI.Dropdown dropdawn;
    #endregion
    #region Zmienne prywatne
    private Material materialWybranegoBudynku = null;
    private Color kolorOrginału;
    private KonkretnyNPCStatyczny knpcs = null;
    private GameObject aktualnyObiekt = null;
    private Vector3 ostatniaPozycjaKursora = Vector3.zero;
    #endregion

    void Awake()
    {
        if (rodzicBudynkow == null)
        {
            GameObject go = new GameObject("Rodzic Budynków");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            rodzicBudynkow = go.transform;
        }
        List<string> wszystkieBudynkiList = new List<string>();
        wszystkieBudynkiList.Add("None");
        for (byte i = 0; i < wszystkieBudynki.Length; i++)
        {
            wszystkieBudynkiList.Add(wszystkieBudynki[i].name);
        }
        this.dropdawn.AddOptions(wszystkieBudynkiList);
    }
    void LateUpdate()
    {
        if (aktualnyObiekt != null)
        {
            if (Input.GetMouseButtonDown(2))
            {
                ResetWybranegoObiektu();
            }
            else
            {
                Vector3 posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);
                aktualnyObiekt.transform.position = posClick;
#if UNITY_STANDALONE
                float numer = Input.GetAxisRaw("Mouse ScrollWheel");
                if (numer != 0)
                {
                    numer = (numer < 0) ? -1 : 1;
                    Vector3 vet = aktualnyObiekt.transform.rotation.eulerAngles;
                    vet.y += numer * 45;
                    aktualnyObiekt.transform.rotation = Quaternion.Euler(vet);
                }
#endif
                ostatniaPozycjaKursora = posClick;
            }
        }
        if (aktualnyObiekt != null && CzyMogęPostawićBudynek(aktualnyObiekt.transform.position))   //Jeśli chcesz postawić dany budynek to
        {
#if UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                ZatwierdzenieBudynku();
            }
#endif
#if UNITY_ANDROID
            if(Input.touchCout > 0 && wybranyBudynek != null)
            {
                ZatwierdzenieBudynku();
            }
#endif
        }
    }
    public void PostawBudynek(ref GameObject obiektDoRespawnu, Vector3 pos, Quaternion rotation)
    {
        aktualnyObiekt = Instantiate(obiektDoRespawnu, pos, rotation);
        materialWybranegoBudynku = aktualnyObiekt.GetComponent<Renderer>().material;
        if (materialWybranegoBudynku != null)
        {
            kolorOrginału = materialWybranegoBudynku.color;
            materialWybranegoBudynku.color = Color.red;
        }
        knpcs = aktualnyObiekt.GetComponent<KonkretnyNPCStatyczny>();
    }
    private void ZatwierdzenieBudynku()
    {
        Vector3 posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);
        if (aktualnyObiekt != null)
        {
            if (TypBudynku.Mur == knpcs.typBudynku)
            {
                posClick = WyrównajSpawn(posClick);
            }
            // Pobieranie niezbędnych danych
            // Ustawienie wszystkich danych po postawieniu budynku
                //Ustawienia obiektu
            knpcs.NastawienieNonPlayerCharacter = NastawienieNPC.Przyjazne;
            aktualnyObiekt.transform.position = posClick;
            aktualnyObiekt.tag = "Budynek";
            aktualnyObiekt.isStatic = true;
            aktualnyObiekt.transform.SetParent(rodzicBudynkow);
                //Ustawienie skryptu KonkretnyNPCStatyuczny
            knpcs.InicjacjaBudynku();
            PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcs, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
                //Ustawienie materiału
            materialWybranegoBudynku.color = kolorOrginału;
            
            // Kasowanie ustawień potrzebnych do postawienia budynku
            materialWybranegoBudynku = null;
            knpcs = null;
            aktualnyObiekt = null;
        }
    }
    public void WybierzBudynekDoPostawienia()  //Wybór obiektu budynku do postawienia
    {
        short index = (short)(this.dropdawn.value - 1);
        if (index > -1 && aktualnyObiekt == null)
        {
            Vector3 posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);

            PostawBudynek(ref wszystkieBudynki[index], posClick, Quaternion.identity);
            ostatniaPozycjaKursora = posClick;
        }
        dropdawn.value = 0;
    }
    private bool CzyMogęPostawićBudynek(Vector3 sugerowanaPozycja)
    {
        KonkretnyNPCStatyczny najbliższyBudynek = PomocniczeFunkcje.WyszukajWDrzewie(ref PomocniczeFunkcje.korzeńDrzewaPozycji, sugerowanaPozycja) as KonkretnyNPCStatyczny;
        if(Mathf.Abs(sugerowanaPozycja.x - najbliższyBudynek.transform.position.x) < najbliższyBudynek.granicaX + knpcs.granicaX &&
        Mathf.Abs(sugerowanaPozycja.z - najbliższyBudynek.transform.position.z) < najbliższyBudynek.granicaZ + knpcs.granicaZ)
        {
            materialWybranegoBudynku.color = Color.red;
            return false;
        }
        if (materialWybranegoBudynku != null)
            materialWybranegoBudynku.color = Color.green;
        return true;
    }
    private void ResetWybranegoObiektu()    //Resetuje ustawienie wybranego budynku
    {
        materialWybranegoBudynku = null;
        knpcs = null;
        aktualnyObiekt = null;
        Destroy(aktualnyObiekt);
    }
    private Vector3 WyrównajSpawn(Vector3 sugerowanePolozenie)
    {
        sugerowanePolozenie.x = Mathf.RoundToInt(sugerowanePolozenie.x);
        sugerowanePolozenie.z = Mathf.RoundToInt(sugerowanePolozenie.z);
        return sugerowanePolozenie;
    }
}
