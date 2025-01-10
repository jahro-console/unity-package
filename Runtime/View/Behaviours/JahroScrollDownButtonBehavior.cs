using UnityEngine;

namespace Jahro.View
{
    internal class JahroScrollDownButtonBehavior : MonoBehaviour
    {
        public JahroScrollView ScrollView;

        private void Start()
        {
            ScrollView.OnAutoscrollStateChanged += OnAutoscrollStateChanged;
            gameObject.SetActive(false);
        }

        private void OnAutoscrollStateChanged(bool obj)
        {
            gameObject.SetActive(!obj);
        }
    }
}