using Networking;
using Terrain;
using UnityEngine;
using NetworkPlayer = Networking.NetworkPlayer;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private World world;
    [SerializeField] private Camera menuCamera;

    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    public Player localPlayer;
    public NetworkPlayer[] players;

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

        players = new NetworkPlayer[50];
    }

    public void SpawnLocalPlayer(Vector3 _pos)
    {
        menuCamera.gameObject.SetActive(false);
        GameObject _player = Instantiate(localPlayerPrefab, _pos, Quaternion.identity);
        localPlayer = _player.GetComponent<Player>();
        _player.GetComponentInChildren<CameraController>().world = world;
        world.gameObject.SetActive(true);
        world.player = _player.GetComponent<PlayerController>();
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _pos)
    {
        GameObject _playerObj = Instantiate(playerPrefab, _pos, Quaternion.identity);
        players[_id] = _playerObj.GetComponent<NetworkPlayer>();
        players[_id].Populate(_id, _username);
    }

    public void DespawnPlayer(int _id)
    {
        players[_id].Despawn();
        players[_id] = null;
    }

    public void GameLoaded()
    {
        ClientSend.GameLoad();
    }

    public void MovePlayer(int _id, Vector3 _pos)
    {
        if (players[_id] != null)
            players[_id].Move(_pos);
    }
}
