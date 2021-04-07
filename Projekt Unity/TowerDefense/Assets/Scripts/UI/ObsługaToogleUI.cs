using UnityEngine;
using UnityEngine.UI;
public class ObsługaToogleUI : MonoBehaviour
{
    public Image obrazekCheckMark;
    public void WłWyłCheckmark()
    {
        obrazekCheckMark.gameObject.SetActive(this.GetComponent<Toggle>().isOn);// = this.GetComponent<Toggle>().isOn;
        //Debug.Log("Ustawiłem obrazek na "+obrazekCheckMark.enabled.ToString());
    }
}
