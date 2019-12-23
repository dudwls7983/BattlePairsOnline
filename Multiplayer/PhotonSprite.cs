using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonSprite : MonoBehaviourPun, IPunObservable
{
    [PunRPC]
    public void SetName(string name)
    {
        this.name = name;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(name);
        }
        else
        {
            name = (string)stream.ReceiveNext();
        }
    }
}
