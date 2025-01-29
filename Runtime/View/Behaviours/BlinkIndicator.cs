using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class BlinkIndicator : MonoBehaviour
    {
        private readonly Color32 DEFAULT_COLOR = new Color32(131, 130, 144, 255);

        private const float BLINK_DURATION = 0.2f;

        [SerializeField]
        private Color TargetColor;

        [SerializeField]
        private Text TargetText;

        private Image _image;

        private Color _targetColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (TargetText == null)
                TargetText = GetComponentInChildren<Text>();
            Clear();
        }

        private void Start()
        {

        }

        public void Blink()
        {
            StartCoroutine(BlinkColor());
        }

        public void SetCount(int count, bool limit = true)
        {
            if (TargetText != null)
            {
                if (limit)
                    TargetText.text = Mathf.Clamp(count, 0, 99).ToString();
                else
                    TargetText.text = count.ToString();
            }

        }

        public void Clear()
        {
            TargetText.text = "0";
            _targetColor = TargetColor;
        }

        private void Update()
        {
            _image.color = Color.Lerp(_image.color, _targetColor, 0.1f);
        }

        private IEnumerator BlinkColor()
        {
            _targetColor = Color.white;
            yield return new WaitForSeconds(BLINK_DURATION);
            _targetColor = TargetColor;
        }
    }
}