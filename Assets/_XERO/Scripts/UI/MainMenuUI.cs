using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Main Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject startGamePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitPanel;

    [Header("Start Game Panel")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button startGamePanelBackButton;

    [Header("Quit Panel")]
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;

    [Header("Settings Buttons")]
    [SerializeField] private Button settingsVolumeButton;
    [SerializeField] private Button settingsGraphicsButton;
    [SerializeField] private Button settingsGameButton;
    [SerializeField] private Button settingsControlsButton;
    [SerializeField] private Button settingsPanelBackButton;

    [Header("Settings Sub Panel Back Buttons")]
    [SerializeField] private Button[] settingsSubPanelsBackButtons;

    [Header("Settings Sub Panels")]
    [SerializeField] private GameObject settingsVolumePanel;
    [SerializeField] private GameObject settingsGraphicsPanel;
    [SerializeField] private GameObject settingsGamePanel;
    [SerializeField] private GameObject settingsControlsPanel;

    [Header("Events")]
    public UnityEvent OnNewGameClicked;

    private void Awake()
    {
        RegisterButtonEvents();
    }

    private void Start()
    {
        OpenMainMenuPanel();
    }

    private void OnDestroy()
    {
        UnregisterButtonEvents();
    }

    private void RegisterButtonEvents()
    {
        startButton.onClick.AddListener(OpenStartGamePanel);
        settingsButton.onClick.AddListener(OpenSettingsPanel);
        quitButton.onClick.AddListener(OpenQuitPanel);

        newGameButton.onClick.AddListener(NewGame);
        startGamePanelBackButton.onClick.AddListener(OpenMainMenuPanel);

        quitYesButton.onClick.AddListener(QuitGame);
        quitNoButton.onClick.AddListener(OpenMainMenuPanel);

        settingsPanelBackButton.onClick.AddListener(OpenMainMenuPanel);

        settingsVolumeButton.onClick.AddListener(OpenSettingsVolumePanel);
        settingsGraphicsButton.onClick.AddListener(OpenSettingsGraphicsPanel);
        settingsGameButton.onClick.AddListener(OpenSettingsGamePanel);
        settingsControlsButton.onClick.AddListener(OpenSettingsControlsPanel);

        foreach (Button backButton in settingsSubPanelsBackButtons)
        {
            if (backButton != null)
                backButton.onClick.AddListener(OpenSettingsPanel);
        }
    }

    private void UnregisterButtonEvents()
    {
        startButton.onClick.RemoveListener(OpenStartGamePanel);
        settingsButton.onClick.RemoveListener(OpenSettingsPanel);
        quitButton.onClick.RemoveListener(OpenQuitPanel);

        newGameButton.onClick.RemoveListener(NewGame);
        startGamePanelBackButton.onClick.RemoveListener(OpenMainMenuPanel);

        quitYesButton.onClick.RemoveListener(QuitGame);
        quitNoButton.onClick.RemoveListener(OpenMainMenuPanel);

        settingsPanelBackButton.onClick.RemoveListener(OpenMainMenuPanel);

        settingsVolumeButton.onClick.RemoveListener(OpenSettingsVolumePanel);
        settingsGraphicsButton.onClick.RemoveListener(OpenSettingsGraphicsPanel);
        settingsGameButton.onClick.RemoveListener(OpenSettingsGamePanel);
        settingsControlsButton.onClick.RemoveListener(OpenSettingsControlsPanel);

        foreach (Button backButton in settingsSubPanelsBackButtons)
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OpenSettingsPanel);
        }
    }

    public void OpenMainMenuPanel()
    {
        CloseAllPanels();
        mainMenuPanel.SetActive(true);
    }

    private void OpenStartGamePanel()
    {
        CloseAllPanels();
        startGamePanel.SetActive(true);
    }

    private void OpenSettingsPanel()
    {
        CloseAllPanels();
        CloseAllSettingsSubPanels();

        settingsPanel.SetActive(true);
    }

    private void OpenQuitPanel()
    {
        CloseAllPanels();
        quitPanel.SetActive(true);
    }

    private void OpenSettingsVolumePanel()
    {
        OpenSettingsSubPanel(settingsVolumePanel);
    }

    private void OpenSettingsGraphicsPanel()
    {
        OpenSettingsSubPanel(settingsGraphicsPanel);
    }

    private void OpenSettingsGamePanel()
    {
        OpenSettingsSubPanel(settingsGamePanel);
    }

    private void OpenSettingsControlsPanel()
    {
        OpenSettingsSubPanel(settingsControlsPanel);
    }

    private void OpenSettingsSubPanel(GameObject panelToOpen)
    {
        CloseAllSettingsSubPanels();

        settingsPanel.SetActive(false);

        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }

    private void CloseAllPanels()
    {
        mainMenuPanel.SetActive(false);
        startGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        quitPanel.SetActive(false);

        CloseAllSettingsSubPanels();
    }

    private void CloseAllSettingsSubPanels()
    {
        settingsVolumePanel.SetActive(false);
        settingsGraphicsPanel.SetActive(false);
        settingsGamePanel.SetActive(false);
        settingsControlsPanel.SetActive(false);
    }

    private void NewGame()
    {
        OnNewGameClicked?.Invoke();

        Debug.Log("New Game Started");
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
