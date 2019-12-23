using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Player : MonoBehaviourPun, IPunObservable
{
    int healthViewID;
    int spriteIndex;

    int playerIndex;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    IEnumerator _TakeDamage(int attackerViewID, int damage)
    {
        if (damage <= 0)
            yield break;

        PhotonView healthView = PhotonView.Find(healthViewID);
        if (healthView == null)
            yield break;

        // 체력은 즉시 감소시켜준다.
        Health health = healthView.GetComponent<Health>();
        if ((health.value -= damage) <= 0)
        {
            health.value = 0;
        }
        
        AudioSource.PlayClipAtPoint(PhotonTool.instance.damageSound, Vector3.zero);

        GameObject indicator = Instantiate(Resources.Load("_Prefabs/Damage")) as GameObject;
        indicator.GetComponent<TextMesh>().text = damage.ToString();

        Vector3 position = healthView.transform.position;
        position.y += 1.95f;

        Vector3 scale = new Vector3(0.2025f, 0.2025f, 0.1f);
        for (int i = 0; i < 40; i++)
        {
            position.y += 0.05f;
            scale -= new Vector3(0.0025f, 0.0025f, 0);

            indicator.transform.position = position;
            indicator.transform.localScale = scale;

            yield return new WaitForSeconds(0.025f);
        }
        Destroy(indicator);

        // 모든 작업이 끝나면 게임이 끝났는지 체크한다.
        if(health.value == 0)
        {
            PairManager.S.EndGame(attackerViewID, true);
        }
    }

    [PunRPC]
    public void TakeDamage(int attackerViewID, int damage)
    {
        StartCoroutine(_TakeDamage(attackerViewID, damage));
    }

    [PunRPC]
    public void RPC_SetupHealth()
    {
        PhotonView healthView = PhotonView.Find(healthViewID);
        if (healthView == null)
            return;

        Health health = healthView.GetComponent<Health>();
        health.value = Health.startHealth;
    }

    [PunRPC]
    public void RPC_SetupPlayer(int playerIndex, string name, int healthViewID, int spriteIndex)
    {
        this.name = name;
        this.healthViewID = healthViewID;
        this.spriteIndex = spriteIndex;
        this.playerIndex = playerIndex;

        GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.playerImages[spriteIndex];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(name);
            stream.SendNext(healthViewID);
            stream.SendNext(spriteIndex);
            stream.SendNext(playerIndex);
        }
        else
        {
            int beforeSpriteIndex = spriteIndex;

            name = (string)stream.ReceiveNext();
            healthViewID = (int)stream.ReceiveNext();
            spriteIndex = (int)stream.ReceiveNext();
            playerIndex = (int)stream.ReceiveNext();

            // 스프라이트 값이 바뀌면 즉시 교체시켜준다.
            if(spriteIndex != beforeSpriteIndex || GetComponent<SpriteRenderer>().sprite == null)
            {
                GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.playerImages[spriteIndex];
            }
        }
    }
}
