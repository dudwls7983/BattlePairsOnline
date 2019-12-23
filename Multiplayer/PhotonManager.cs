using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;
    
    public int selectedSpriteIndex; // 유저가 선택한 플레이어 이미지
    public int playerIndex; // 유저의 플레이어 인덱스 값

    // 서버와 연결 중일 때만 시작 버튼을 누를 수 있다.
    public UnityEngine.UI.Button startButton;

    public List<RoomInfo> roomList;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void CreateManager()
    {
        GameObject gameObject = new GameObject("PhotonManager");
        gameObject.AddComponent<PhotonManager>();
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
    }

    #region PhotonCallBack

    public override void OnConnectedToMaster()
    {
        SceneManager.MainScene_UpdateState("Connected");

        Debug.Log("Connected to " + PhotonNetwork.CloudRegion.ToUpper() + " server");

        PhotonNetwork.AutomaticallySyncScene = false;
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.Log("Disconnected from server. " + cause);
        SceneManager.MainScene_UpdateState("Disconnected");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("2.Battle");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("1.Main");
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player player)
    {
        Debug.Log("some player left the room");
        foreach (var p in PhotonNetwork.PlayerList)
        {
            PhotonNetwork.RemoveRPCs(p);
        }

        PairManager.S.EndGame(PairManager.S.players[playerIndex].GetPhotonView().ViewID, false);
        PairManager.S.photonView.RPC("ClearPlayerList", RpcTarget.All);
    }

    #endregion

    #region InGameCallback
    public void OnEnteredGame()
    {
        Debug.Log("Entered Game");

        GameObject.Find("Loading").SetActive(false);

        GameObject player1Exist = GameObject.Find("Player1");
        GameObject player2Exist = GameObject.Find("Player2");

        Vector3 position = new Vector3(-13.8f, 5.4f, 0);
        Vector3 healthPosition = new Vector3(-12.55f, 1.4f, 0);
        if (player1Exist)
        {
            position *= -1;
            healthPosition = new Vector3(12.35f, -1.2f, 0);
        }

        GameObject playerObject = PhotonNetwork.Instantiate("_Prefabs/Player", position, Quaternion.identity);
        GameObject playerHealth = PhotonNetwork.Instantiate("_Prefabs/HP", healthPosition, Quaternion.identity);
        
        if (player1Exist)
        {
            player2Exist = playerObject;
            playerIndex = 1;
            playerObject.GetPhotonView().RPC("RPC_SetupPlayer", RpcTarget.All, 1, "Player2", playerHealth.GetPhotonView().ViewID, selectedSpriteIndex);
            playerHealth.GetPhotonView().RPC("RPC_SetupPlayerHealth", RpcTarget.All, "P2_HP");
            GameObject.Find("up").SetActive(false);
        }
        else
        {
            player1Exist = playerObject;
            playerIndex = 0;
            playerObject.GetPhotonView().RPC("RPC_SetupPlayer", RpcTarget.All, 0, "Player1", playerHealth.GetPhotonView().ViewID, selectedSpriteIndex);
            playerHealth.GetPhotonView().RPC("RPC_SetupPlayerHealth", RpcTarget.All, "P1_HP");
            GameObject.Find("down").SetActive(false);
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PairManager.S.photonView.RPC("PlayBellSound", RpcTarget.All);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PairManager.S.photonView.RPC("AddPlayerToList", RpcTarget.All, player1Exist.name);
            PairManager.S.photonView.RPC("AddPlayerToList", RpcTarget.All, player2Exist.name);

            Debug.Log("start game");
            PairManager.S.photonView.RPC("StartGame", RpcTarget.MasterClient);
        }
    }
    #endregion

    #region RPCFunction
    #endregion

    void CreateRoom()
    {
        int randomNumber = Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)2 };
        PhotonNetwork.CreateRoom("Room" + randomNumber, roomOptions);
    }
}
