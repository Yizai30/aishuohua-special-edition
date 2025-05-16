using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace MobileNumericKeypad
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225953")]
    public class Keypad : MonoBehaviour
    {
        [SerializeField]
        private GameObject placeholder, stick;

        [SerializeField]
        private KeypadBG keypadBG;

        [SerializeField]
        private Text text;

        [SerializeField]
        private int maxLength;

        [SerializeField]
        private UnityEvent onClickSearch;

        private Coroutine stickAnim;

        private const string separator = ",";

        /// <summary>
        /// Performs the actions that you specified in the "onClickSearch" field in the Unity editor
        /// </summary>
        public void Search()
        {
            if (onClickSearch != null)
                onClickSearch.Invoke();
            text.text = string.Empty;
            placeholder.SetActive(true);
        }

        /// <summary>
        /// Adds a digit to a number
        /// </summary>
        /// <param name="value"></param>
        public void Write(int value)
        {
            print("write" + value);
            placeholder.SetActive(false);
            if (stickAnim != null)
                StopCoroutine(stickAnim);
            stick.SetActive(false);
            if (text.text.Replace(separator, string.Empty).Length < maxLength)
            {
                text.text += value;
                //if (text.text[0] == '0' && text.text.Length == 2)
                //    text.text = text.text[1].ToString();
                //else
                    //text.text = GetBeautifulNumber(text.text);
                   // text.text = text.text;
            }
        }

        /// <summary>
        /// Removes the last character
        /// </summary>
        public void Remove()
        {
            if (text.text == string.Empty)
                return;
            //string number = text.text.Replace(separator, string.Empty);
            string number = text.text;
            //text.text = GetBeautifulNumber(number.Substring(0, number.Length - 1));
            text.text = number.Substring(0, number.Length - 1);
            if (text.text == string.Empty)
                placeholder.SetActive(true);
        }

        /// <summary>
        /// Performs actions after pressing the Enter button
        /// </summary>
        public void Enter()
        {
            keypadBG.OnPointerDown(null);
        }

        /// <summary>
        /// Adds separators to the number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string GetBeautifulNumber(string number)
        {
            number = number.Replace(separator, string.Empty);
            string invertStr = string.Empty;
            for (int i = number.Length - 1; i >= 0; i--)
                invertStr += number[i];
            invertStr = string.Empty;
            for (int i = 0; i < number.Length; i++)
            {
                if (i % 3 == 0 && i > 0)
                    invertStr += separator;
                invertStr += number[number.Length - 1 - i];
            }
            number = string.Empty;
            for (int i = invertStr.Length - 1; i >= 0; i--)
                number += invertStr[i];
            return number;
        }

        /// <summary>
        /// Flashing the stick
        /// </summary>
        /// <returns></returns>
        private IEnumerator StickAnim()
        {
            stick.SetActive(true);
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return waitForSeconds;
                stick.SetActive(!stick.activeSelf);
            }
        }

        /// <summary>
        /// Start flashing the stick
        /// </summary>
        private void OnEnable()
        {
            if (text.text == string.Empty)
                stickAnim = StartCoroutine(StickAnim());
        }

        /// <summary>
        /// Stop flashing the stick
        /// </summary>
        private void OnDisable()
        {
            if (stickAnim != null)
                StopCoroutine(stickAnim);
        }

        /// <summary>
        /// Closing the keypad when pressing the back button on the phone
        /// </summary>
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
                keypadBG.OnPointerDown(null);
        }
    }
}