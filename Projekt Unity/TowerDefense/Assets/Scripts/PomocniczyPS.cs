using UnityEngine;

public class PomocniczyPS : MonoBehaviour
{
    private float actTime;
    private byte idx = 0;
    private ParticleSystem ps = null;

    void Start()
    {
        ps = this.GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if(idx == 5)
        {
            if(actTime < ps.main.duration)
            {
                actTime += Time.deltaTime*5.0f;
            }
            else
            {
                PomocniczeFunkcje.managerGryScript.DodajDoTablicyStackowParticli(ref ps);
                actTime = 0;
                this.gameObject.SetActive(false);
            }
        }
        else
            idx++;
    }
}
