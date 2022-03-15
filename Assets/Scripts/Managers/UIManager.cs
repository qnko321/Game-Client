using Inventory;
using Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        [Header("References")] 
        public InventoryMenu inventoryMenu;
        public DebugMenu debugMenu;
        
        [Header("Input")] 
        [SerializeField] private InputActionReference pauseInput;

        [Header("Pause")] 
        public GameObject pauseMenu;
    
        [SerializeField] private bool isPaused;

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

        private void OnEnable()
        {
            pauseInput.ToInputAction().started += PauseInputListener;
        }

        private void OnDisable()
        {
            pauseInput.ToInputAction().started -= PauseInputListener;
        }

        #region Input

        private void PauseInputListener(InputAction.CallbackContext _ctx)
        {
            if (_ctx.started)
                TogglePause();
        }

        #endregion


        #region Pause

        private void Pause()
        {
            pauseMenu.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Resume()
        {
            pauseMenu.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void TogglePause()
        {
            if (isPaused)
                Resume();
            else
                Pause();
            isPaused = !isPaused;
        }

        #endregion

        public void Disconnect()
        {
            Client.instance.Disconnect();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
