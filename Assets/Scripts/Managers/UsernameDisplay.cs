using TMPro;
using UnityEngine;

namespace Managers
{
    public class UsernameDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        public void SetUsername(string _username)
        {
            text.text = _username;
        }
    }
}