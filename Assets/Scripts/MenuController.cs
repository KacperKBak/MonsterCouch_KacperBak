using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour, ICancelHandler
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject MainMenuFirstFocus;
    [SerializeField] private GameObject SettingsFirstFocus;
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private EventSystem EventSystem;
    
    private MenuState currentState = MenuState.MainMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ShowMainMenu();
    }

    public void SettingsButtonPressed()
    {
        ShowSettingsMenu();
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    public void BackButtonPressed()
    {
        OnCancel(null);
    }
    
    public void OnCancel(BaseEventData eventData)
    {
        switch (currentState)
        {
            case MenuState.MainMenu:
                EventSystem.SetSelectedGameObject(ExitButton);
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
        EventSystem.SetSelectedGameObject(SettingsFirstFocus);
        currentState = MenuState.SettingsMenu;
    }

    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        EventSystem.SetSelectedGameObject(MainMenuFirstFocus);
        currentState = MenuState.MainMenu;
    }

    private enum MenuState
    {
        MainMenu,
        SettingsMenu
    }
}
