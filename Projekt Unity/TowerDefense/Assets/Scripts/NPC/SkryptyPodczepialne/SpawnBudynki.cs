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
    private Vector3 posClick = Vector3.zero;
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
    void Start()
    {
        //Postaw Target
        aktualnyObiekt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        aktualnyObiekt.AddComponent<BoxCollider>();
        aktualnyObiekt.name = "Target";
        aktualnyObiekt.AddComponent<KonkretnyNPCStatyczny>();
        aktualnyObiekt.layer = 8;
        knpcs = aktualnyObiekt.GetComponent<KonkretnyNPCStatyczny>();
        PomocniczeFunkcje.celWrogów = knpcs;
        ZatwierdzenieBudynku(true);
    }
    void FixedUpdate()
    {
        if (aktualnyObiekt != null)
        {
            posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);
        }
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
                if (aktualnyObiekt.transform.position != posClick)
                {
                    aktualnyObiekt.transform.position = posClick;
                    ostatniaPozycjaKursora = posClick;
                }
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
    private void ZatwierdzenieBudynku(bool czyTarget = false)
    {
        if (czyTarget)
            posClick = new Vector3(250f, 0.5f, 250f);
        if (aktualnyObiekt != null || czyTarget)
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
            aktualnyObiekt.transform.SetParent(rodzicBudynkow);
            //Ustawienie skryptu KonkretnyNPCStatyuczny
            knpcs.InicjacjaBudynku();
            PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcs, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
            //Ustawienie materiału
            if (!czyTarget)
                materialWybranegoBudynku.color = kolorOrginału;

            if (PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek != null)
                PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek();
            //Teraz nalezy umieścić budynek w odpowiednim miejscu tablicy PomocniczeFunkcje.tablicaWież
            byte[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(posClick);
            byte s = (byte)(knpcs.zasięgAtaku / 5);
            int k = 0;
            while (s > 0)  //Ustawienie budynku na planszy aby wieża mogła stwierdzić że może atakować
            {
                for (int x = temp[0] - k; x <= temp[0] + k; x++)
                {
                    for (int z = temp[1] - k; z <= temp[1] + k; z++)
                    {
                        if (x == temp[0] - k || x == temp[0] + k || 
                        z == temp[1] - k || z == temp[1] + k)
                        {
                            if (PomocniczeFunkcje.tablicaWież[x, z] == null)
                            {
                                PomocniczeFunkcje.tablicaWież[x, z] = new List<InformacjeDlaPolWież>();
                            }
                            PomocniczeFunkcje.tablicaWież[x, z].Add(new InformacjeDlaPolWież(s, knpcs));
                        }
                    }
                }
                k++;
                s--;
            }
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
            PostawBudynek(ref wszystkieBudynki[index], posClick, Quaternion.identity);
            ostatniaPozycjaKursora = posClick;
        }
        dropdawn.value = 0;
    }
    private bool CzyMogęPostawićBudynek(Vector3 sugerowanaPozycja)
    {
        KonkretnyNPCStatyczny najbliższyBudynek = PomocniczeFunkcje.WyszukajWDrzewie(ref PomocniczeFunkcje.korzeńDrzewaPozycji, sugerowanaPozycja) as KonkretnyNPCStatyczny;
        if (Mathf.Abs(sugerowanaPozycja.x - najbliższyBudynek.transform.position.x) < najbliższyBudynek.granicaX + knpcs.granicaX &&
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
