using UnityEngine;
using UnityEngine.EventSystems;
public class OdpalParticle : MonoBehaviour
{
    private EventSystem eventSystem;
    private bool czyOdpalony = false;
    private GameObject pSystem;
    // Start is called before the first frame update
    void Start()
    {
        eventSystem = PomocniczeFunkcje.eSystem;
        pSystem = this.GetComponentInChildren<ParticleSystem>().gameObject;
        if(pSystem == null)
            Debug.Log(this.gameObject.name);
        pSystem.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!czyOdpalony)
        {
            if(this.gameObject == eventSystem.currentSelectedGameObject)
            {
                czyOdpalony = true;
                pSystem.SetActive(true);
            }
        }
        else
        {
            if(this.gameObject != eventSystem.currentSelectedGameObject)
            {
                czyOdpalony = false;
                pSystem.SetActive(false);
            }
        }
    }
}
