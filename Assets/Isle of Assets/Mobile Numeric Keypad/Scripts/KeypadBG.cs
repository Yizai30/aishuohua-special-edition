using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MobileNumericKeypad
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225953")]
    public class KeypadBG : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private UnityEvent events;

        /// <summary>
        /// Perform actions when clicking on the area behind the keypad
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            events?.Invoke();
        }
    }
}