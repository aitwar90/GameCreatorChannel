using UnityEngine;
using UnityEngine.UI;
public class CzyJestemWyłączonyIZaznaczony : MonoBehaviour
{
    private Button ja;
    private Image image;
    private bool czyOznaczonyJestem = false;
    // Start is called before the first frame update
    void Start()
    {
        ja = this.GetComponent<Button>();
        image = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.ja.interactable)
        {
            if (!czyOznaczonyJestem)
            {
                if(PomocniczeFunkcje.eSystem.currentSelectedGameObject == this.gameObject)
                {
                    czyOznaczonyJestem = true;
                    image.color = this.ja.colors.selectedColor;
                }
            }
            else
            {
                if(PomocniczeFunkcje.eSystem.currentSelectedGameObject != this.gameObject)
                {
                    czyOznaczonyJestem = false;
                    image.color = this.ja.colors.normalColor;
                }
            }
        }
    }
}
