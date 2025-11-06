using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MonsterCouch.UI
{
    public sealed class MenuController : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private GameObject mainMenu = null!;
        [SerializeField] private GameObject settingsMenu = null!;
        [SerializeField] private GameObject mainMenuFirstFocus = null!;
        [SerializeField] private GameObject settingsFirstFocus = null!;
        [SerializeField] private GameObject exitButton = null!;
        [SerializeField] private EventSystem eventSystem = null!;
        [SerializeField] private InputActionReference cancelAction = null!;

        private InputAction? _cancelInputAction;
        private MenuState _currentState = MenuState.MainMenu;

        private void Awake()
        {
            if (eventSystem == null)
            {
                eventSystem = EventSystem.current
                    ?? throw new InvalidOperationException("EventSystem reference is required.");
            }

            _cancelInputAction = cancelAction != null ? cancelAction.action : null;
        }

        private void OnEnable()
        {
            if (_cancelInputAction != null)
            {
                _cancelInputAction.performed += HandleCancelPerformed;
                _cancelInputAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (_cancelInputAction != null)
            {
                _cancelInputAction.performed -= HandleCancelPerformed;
                _cancelInputAction.Disable();
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        public void SettingsButtonPressed() => ShowSettingsMenu();

        public void PlayButtonPressed()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                Debug.LogError("Game scene name is not configured.");
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        public void QuitButtonPressed() => Application.Quit();

        public void BackButtonPressed() => HandleCancel();

        public void OnCancel(BaseEventData eventData) => HandleCancel();

        private void HandleCancelPerformed(InputAction.CallbackContext context) => HandleCancel();

        private void HandleCancel()
        {
            switch (_currentState)
            {
                case MenuState.MainMenu:
                    SetSelectedGameObject(exitButton);
                    break;
                case MenuState.SettingsMenu:
                    ShowMainMenu();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowSettingsMenu()
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);
            SetSelectedGameObject(settingsFirstFocus);
            _currentState = MenuState.SettingsMenu;
        }

        private void ShowMainMenu()
        {
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
            SetSelectedGameObject(mainMenuFirstFocus);
            _currentState = MenuState.MainMenu;
        }

        private enum MenuState
        {
            MainMenu,
            SettingsMenu
        }

        private void SetSelectedGameObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(target);
        }
    }
}
