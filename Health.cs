using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Health : MonoBehaviourPun, IPunObservable
{
    public const int startHealth = 100;
    public int value = 100;
    
    TextMesh health2Text;
    Color color;

    private void Awake()
    {
        health2Text = gameObject.GetComponent<TextMesh>();
    }
    public void Start()
    {
        color = Color.green;
        value = startHealth;
    }
    private void Update()
    {
        if (value > 70)
            color = Color.green;
        else if (value > 30)
            color = Color.yellow;
        else
            color = Color.white;
       
        health2Text.text = value.ToString();
        health2Text.color = color;
    }

    public Color VectorToColor(Vector3 vector)
    {
        Color color = new Color(vector.x, vector.y, vector.z);
        return color;
    }

    public Vector3 ColorToVector(Color color)
    {
        Vector3 vector = new Vector3(color.r, color.g, color.b);
        return vector;
    }

    [PunRPC]
    public void RPC_SetupPlayerHealth(string name)
    {
        this.name = name;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Color는 기본적으로 Photon에서 Serialize를 제공해주지 않는다. Vector3로 변환해서 통신을 한다.
            stream.SendNext(ColorToVector(color));
            stream.SendNext(name);
            stream.SendNext(value);
        }
        else
        {
            color = VectorToColor((Vector3)stream.ReceiveNext());
            name = (string)stream.ReceiveNext();
            value = (int)stream.ReceiveNext();
        }
    }
}
