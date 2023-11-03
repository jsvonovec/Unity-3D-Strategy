using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderValueOutput : MonoBehaviour
{
    public List<TMPro.TMP_Text> texts;
    public TMPro.TMP_InputField inputField;
    public UnityEngine.UI.Slider slider;

    // Update is called once per frame
    public void OutputValue()
    {
        if (slider == null)
            slider = GetComponent<UnityEngine.UI.Slider>();

        foreach (TMPro.TMP_Text t in texts)
        {
            t.text = "";
            t.text = slider.value.ToString();
        }

        
        inputField.text = slider.value.ToString();
        
    }

    public void InputValue()
    {
        if (slider == null)
            slider = GetComponent<UnityEngine.UI.Slider>();

        int value;
        if (!int.TryParse(inputField.text, out value))
            value = 0;

        value = (int)Mathf.Clamp(value, slider.minValue, slider.maxValue);

        inputField.text = value.ToString();
        slider.value = value;
    }
}
