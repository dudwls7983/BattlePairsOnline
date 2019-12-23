using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonTool : MonoBehaviour
{
    public static PhotonTool S;

    public Sprite[] playerImages;
    public Sprite[] player1TurnImages;
    public Sprite[] player2TurnImages;
    public Sprite[] startButtonImages;
    public Sprite winnerImages;

    public AudioClip bellSound;
    public AudioClip pairsSound;
    public AudioClip damageSound;
    public AudioClip flipSound;

    public static PhotonTool instance
    {
        get
        {
            if (S == null)
            {
                GameObject go = Instantiate(Resources.Load("_Prefabs/PhotonTool")) as GameObject;
                return go.GetComponent<PhotonTool>();
            }
            else
                return S;
        }
    }

    private void Awake()
    {
        S = this;
    }

    #region MainScene
    public void Main_StartMatching()
    {
        PhotonManager.instance.selectedSpriteIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
        SceneManager.MainScene_UpdateState("StartMatching");
        PhotonNetwork.JoinRandomRoom();
    }

    public void Main_SelectImage()
    {
        SceneManager.MainScene_UpdateState("SelectPlayerImage");
    }

    public void Main_ShowHowto()
    {
        SceneManager.MainScene_UpdateState("ShowHowto");
    }

    public void Main_HideHowto()
    {
        SceneManager.MainScene_UpdateState("HideHowto");
    }

    public void Main_Quit()
    {
        Application.Quit();
    }
    #endregion

    #region BattleScene
    public void Battle_ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion

    public GameObject Instantiate(string prefabName)
    {
        GameObject go = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
        return go;
    }

    public GameObject CreateGameObject(string name)
    {
        GameObject go = PhotonNetwork.Instantiate("_Prefabs/EmptyObject", Vector3.zero, Quaternion.identity);
        go.GetPhotonView().RPC("SetName", RpcTarget.All, name);
        return go;
    }
}
