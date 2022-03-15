using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager instance;
    
        [Header("Start")] 
        public GameObject startMenu;
    
        [Header("Join")] 
        public GameObject joinMenu;
        public TMP_Text warningText;
        public TMP_InputField usernameInputField;
        public TMP_InputField ipInputField;

        [Header("Settings")] 
        public GameObject settingsMenu;

        [Header("Connection")] 
        public GameObject connectionMenu;
        public TMP_Text connectionStatusText;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        #region StartMenu

        public void StartToJoin()
        {
            startMenu.SetActive(false);
            joinMenu.SetActive(true);
        }

        public void StartToSettings()
        {
            startMenu.SetActive(false);
            settingsMenu.SetActive(true);
        }

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region JoinMenu

        public void Join()
        {
            if (string.IsNullOrEmpty(usernameInputField.text))
            {
                warningText.text = "Please enter username.";
                return;
            }
            joinMenu.SetActive(false);
            connectionMenu.SetActive(true);
            string[] _ipPort = ipInputField.text.Split(':');
            Client.instance.ConnectToServer(SetConnectionStatus, _ipPort[0], int.Parse(_ipPort[1]));
            warningText.text = "";
        }

        public void JoinToStart()
        {
            joinMenu.SetActive(false);
            startMenu.SetActive(true);
        }

        #endregion

        #region SettingsMenu

        public void SettingsToStart()
        {
            settingsMenu.SetActive(false);
            startMenu.SetActive(true);
        }

        #endregion

        #region ConnectMenu
    
        public void CancelConnection()
        {
            connectionStatusText.text = "";
            connectionMenu.SetActive(false);
            joinMenu.SetActive(true);
        }
    
        private void SetConnectionStatus(string _status)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                connectionStatusText.text = _status;
                if (_status.Equals("Connected successfully"))
                {
                    SceneManager.LoadScene("Game");
                }
            });
        }

        #endregion
    }
}
