using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;

namespace JahroConsole
{
  public class JahroEventSystemController : MonoBehaviour
  {
    private List<EventSystem> _eventSystems = new List<EventSystem>();

#if ENABLE_LEGACY_INPUT_MANAGER
    private StandaloneInputModule _jahroInputModule;
#endif
#if ENABLE_INPUT_SYSTEM
    private InputSystemUIInputModule _jahroUIInputModule;
#endif

    private EventSystem _eventSystem;

    private bool _handled;

    public bool Handled { get => _handled; set => _handled = value; }

    private void Awake()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
      _jahroInputModule = gameObject.GetComponent<StandaloneInputModule>();
#endif

#if ENABLE_INPUT_SYSTEM
      _jahroUIInputModule = gameObject.GetComponent<InputSystemUIInputModule>();
#endif

      _eventSystem = gameObject.GetComponent<EventSystem>();
      ProcessEventSystems();
    }

    private void OnEnable()
    {
      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
      Handled = false;
      ProcessEventSystems();
    }

    private void ProcessEventSystems()
    {
      _eventSystems = FindObjectsOfType<EventSystem>().ToList();

#if ENABLE_LEGACY_INPUT_MANAGER
      HandleWithOldInputSystem();
      Handled = true;
#endif

#if ENABLE_INPUT_SYSTEM
      if (_handled)
        return;

      HandleWithNewInputSystem();
#endif
    }

    private void HandleWithOldInputSystem()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
      if (_eventSystems.Count == 0)
      {
        _eventSystem = gameObject.AddComponent<EventSystem>();
        _jahroInputModule = gameObject.AddComponent<StandaloneInputModule>();
      }
      else if (_eventSystems.Count > 1 && _jahroInputModule != null && _eventSystem != null)
      {
        Destroy(_jahroInputModule);
        Destroy(_eventSystem);
      }
#endif
    }

    private void HandleWithNewInputSystem()
    {
#if ENABLE_INPUT_SYSTEM
      if (_eventSystems.Count == 0)
      {
        _eventSystem = gameObject.AddComponent<EventSystem>();
        _jahroUIInputModule = gameObject.AddComponent<InputSystemUIInputModule>();
      }
      else if (_eventSystems.Count > 1 && _jahroUIInputModule != null && _eventSystem != null)
      {
        Destroy(_jahroUIInputModule);
        Destroy(_eventSystem);
      }
#endif
    }

    private void OnDisable()
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
    }
  }
}