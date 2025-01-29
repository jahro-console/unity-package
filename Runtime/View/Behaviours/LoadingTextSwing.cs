using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{

    internal class LoadingTextSwing : MonoBehaviour
    {
        private Text textLabel;

        private void Awake()
        {
            textLabel = GetComponent<Text>();
            textLabel.text = "Loading...";
        }

        private void OnEnable()
        {
            StartCoroutine(Swing());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator Swing()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                string text = textLabel.text;
                if (text.EndsWith("..."))
                {
                    text = "Loading";
                }
                else
                {
                    text += ".";
                }
                textLabel.text = text;
            }
        }
    }
}