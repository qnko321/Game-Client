using Managers;
using UnityEngine;

namespace Networking
{
    public class NetworkPlayer : MonoBehaviour
    {
        public UsernameDisplay usernameDisplay;
        
        public int id;
        public string username;
        private Transform _trans;
        public void Populate(int _id, string _username)
        {
            id = _id;
            username = _username;
            usernameDisplay.SetUsername(_username);
        }
        
        private void Awake()
        {
            _trans = transform;
        }

        public void Despawn()
        {
            Destroy(gameObject);
        }

        public void Move(Vector3 _pos)
        {
            _trans.position = _pos;
        }
    }
}