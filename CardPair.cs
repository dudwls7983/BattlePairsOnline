using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardPair : Card, IPunObservable
{
    public List<CardPair> hiddenby = new List<CardPair>();

    bool isFlipping;

    private void OnMouseUp()
    {
        if (isFlipping || PairManager.S.blockFlip) // 카드를 뒤집을 수 없는 경우
            return;

        if (faceup) // 이미 펼쳐진 카드는 다시 펼칠 수 없다.
            return;

        if(PairManager.S.GetCurrnetTurnPlayer().GetPlayerIndex() != PhotonManager.instance.playerIndex)
            return;

        CardPair A = PairManager.S.GetA();
        if(A == null)
        {
            PairManager.S.SetCardA(this);
            Flip(true);
            return;
        }

        Flip(true);

        if (PairManager.S.Check(this))
            PairManager.S.IncreaseChain(rank);
        else
            PairManager.S.photonView.RPC("NextTurn", RpcTarget.All);
    }

    public void Flip(bool playSound)
    {
        if (photonView == null)
            return;

        photonView.RPC("FlipCard", RpcTarget.All, playSound);
    }

    #region RPC_Procedure
    [PunRPC]
    public void RPC_SetupCard(string name, string suit, int rank, Vector3 color, string parentName, Vector3 position)
    {
        this.name = name;
        this.suit = suit;
        this.rank = rank;
        this.color = new Color(color.x, color.y, color.z);
        transform.parent = GameObject.Find(parentName).transform;
        transform.localPosition = position;
        isFlipping = false;

        #region DecoStart

        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        def = Deck.instance.GetCardDefinitionByRank(rank);

        foreach (Decorator deco in Deck.instance.decorators)
        {
            if (deco.type == "suit")
            {
                tGO = Instantiate(Deck.instance.prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();

                tSR.sprite = Deck.instance.GetSuit(suit);
            }
            else
            {
                tGO = Instantiate(Deck.instance.prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                tS = Deck.instance.rankSprites[rank];
                tSR.sprite = tS;
                tSR.color = this.color;
            }
            tSR.sortingOrder = 1;
            tGO.transform.parent = transform;
            tGO.transform.localPosition = deco.loc;
            if (deco.flip)
            {
                tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (deco.scale != 1)
            {
                tGO.transform.localScale = Vector3.one * deco.scale;
            }
            tGO.name = deco.type;
            decoGOs.Add(tGO);
        }
        foreach (Decorator pip in def.pips)
        {
            tGO = Instantiate(Deck.instance.prefabSprite) as GameObject;
            tGO.transform.parent = transform;
            tGO.transform.localPosition = pip.loc;
            if (pip.flip)
            {
                tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (pip.scale != 1)
            {
                tGO.transform.localScale = Vector3.one * pip.scale;
            }
            tGO.name = "pip";
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = Deck.instance.GetSuit(suit);
            tSR.sortingOrder = 1;
            pipGOs.Add(tGO);
        }
        if (def.face != "")
        {
            tGO = Instantiate(Deck.instance.prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer>();

            tS = Deck.instance.GetFace(def.face + suit);
            //print(card.def.face + card.suit);

            tSR.sprite = tS;
            tSR.sortingOrder = 1;
            tGO.transform.parent = transform;
            tGO.transform.localPosition = Vector3.zero;
            tGO.name = "face";
        }
        tGO = Instantiate(Deck.instance.prefabSprite) as GameObject;
        tSR = tGO.GetComponent<SpriteRenderer>();
        tSR.sprite = Deck.instance.cardBack;
        tGO.transform.parent = transform;
        tGO.transform.localPosition = Vector3.zero;

        tSR.sortingOrder = 2;
        tGO.name = "back";

        #endregion

        back = tGO;
        faceup = false;
    }

    [PunRPC]
    public void SetFaceUp(bool faceup)
    {
        this.faceup = faceup;
    }

    [PunRPC]
    public void SetParent(int parentViewID)
    {
        transform.parent = PhotonView.Find(parentViewID).transform;
    }

    [PunRPC]
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    [PunRPC]
    public void FlipCard(bool playSound)
    {
        if (playSound) AudioSource.PlayClipAtPoint(PhotonTool.instance.flipSound, Vector3.zero);
        StartCoroutine(_FlipCard());
    }
    #endregion

    private IEnumerator _FlipCard()
    {
        isFlipping = true;
        if (!faceup) transform.rotation = Quaternion.Euler(0, 180, 0);
        for (int i = 0; i < 40; i++)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 4.5f, 0);
            if (i == 20)
                faceup = !faceup;
            yield return new WaitForSeconds(0.0125f);
        }
        isFlipping = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
