using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public class UIResourceValue : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Image iconImage;

        public void SetValue(ResourceValue resourceValue)
        {
            if (valueText != null)
            {
                valueText.text = resourceValue.Value.ToString();
            }
        }
    }
}