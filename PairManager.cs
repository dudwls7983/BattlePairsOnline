using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum EGameState
{
    Ready,
    PlayingGame,
    End,
}

public class PairManager : MonoBehaviourPun, IPunObservable
{

    static public PairManager S;


    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;

    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardPair target;
    public List<CardPair> tableau; // 
    public List<CardPair> drawPile; // 게임 시작시 셔플된 카드 리스트

    public List<GameObject> players;
    public List<GameObject> turnSprites;
    public int currentTurn;

    public EGameState gameState;

    CardPair A;

    int chargedPoint;
    int chainCount;

    bool blockFlipCard;

    public bool blockFlip
    {
        get
        {
            return blockFlipCard;
        }
        set
        {
            blockFlipCard = value;
        }
    }

    private void Awake()
    {
        S = this;

        gameState = EGameState.Ready;
    }

    private void Start()
    {
        currentTurn = 0;
        
        deck = GetComponent<Deck>();
        layout = GetComponent<Layout>();

        // 덱, 레이아웃 셋팅
        deck.initDeck(deckXML.text);
        layout.ReadLayout(layoutXML.text);
    }

    IEnumerator FaceDown(CardPair B)
    {
        yield return new WaitForSeconds(1);
        A.Flip(true);
        B.Flip(false);

        SetCardA(null);
    }
    public void SetCardA(CardPair _A)
    {
        A = _A;
    }
    public CardPair GetA()
    {
        return A;
    }

    public bool Check(CardPair B)
    {
        if (A == null || B == null)
            return false;

        if (A.rank == B.rank && A.color == B.color)
        {
            SetCardA(null);
            photonView.RPC("PlayPairSound", RpcTarget.All);
            return true;
        }

        StartCoroutine(FaceDown(B));
        return false;

    }
    
    public Player GetCurrnetTurnPlayer()
    {
        return players[currentTurn].GetComponent<Player>();
    }

    public void IncreaseChain(int point)
    {
        chargedPoint += point;
        chainCount++;
    }
    public void ResetChain()
    {
        chargedPoint = 0;
        chainCount = 0;
    }
    public int CalculateDamage()
    {
        return chargedPoint * chainCount;
    }

    public EGameState GetGameState()
    {
        return gameState;
    }

    #region DeckSystem
    CardPair Draw()
    {
        CardPair cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }
    List<CardPair> ConvertListCardToListCardPair(List<Card> ICD)
    {
        List<CardPair> ICP = new List<CardPair>();
        CardPair tCP;
        foreach (Card tCD in ICD)
        {
            tCP = tCD as CardPair;
            ICP.Add(tCP);
            //print("convert");
            //print(tCP.layoutID);
        }
        return (ICP);
    }
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = GameObject.Find("_LayoutAnchor");
            if(tGO == null)
                tGO = PhotonTool.instance.CreateGameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardPair cp;
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.photonView.RPC("SetFaceUp", RpcTarget.All, tSD.faceUp);
            cp.photonView.RPC("SetParent", RpcTarget.All, layoutAnchor.gameObject.GetPhotonView().ViewID);
            cp.photonView.RPC("SetPosition", RpcTarget.All, new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID));

            tableau.Add(cp);
        }
    }
    #endregion

    #region RPC_Procedure
    [PunRPC]
    public void AddPlayerToList(string name)
    {
        players.Add(GameObject.Find(name));
    }

    [PunRPC]
    public void ClearPlayerList()
    {
        players.Clear();
    }

    [PunRPC]
    public void ClearCardList()
    {
        if (photonView.IsMine == false)
            return;

        foreach(CardPair card in tableau)
        {
            PhotonNetwork.Destroy(card.photonView);
            Destroy(card.gameObject);
        }
        tableau.Clear();
    }

    [PunRPC]
    public void PlayPairSound()
    {
        AudioSource.PlayClipAtPoint(PhotonTool.instance.pairsSound, Vector3.zero);
    }

    [PunRPC]
    public void PlayBellSound()
    {
        AudioSource.PlayClipAtPoint(PhotonTool.instance.bellSound, Vector3.zero);
    }

    [PunRPC]
    public void NextTurn()
    {
        Player player = players[currentTurn].GetComponent<Player>();
        currentTurn = (currentTurn + 1) % players.Count;
        Player target = players[currentTurn].GetComponent<Player>();
        target.photonView.RPC("TakeDamage", RpcTarget.All, player.photonView.ViewID, CalculateDamage());

        SetTurn(currentTurn);
        ResetChain();
    }

    [PunRPC]
    public void SetTurn(int turnIndex)
    {
        currentTurn = turnIndex % 2;
        // turnSprites[0](left UI)는 currentTurn이 0일때 불이 들어오고, 1일때 꺼진다.
        turnSprites[0].GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.player1TurnImages[(currentTurn + 1) % 2];
        turnSprites[1].GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.player2TurnImages[currentTurn];
    }

    [PunRPC]
    public void CloseTurn()
    {
        turnSprites[0].GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.player1TurnImages[0];
        turnSprites[1].GetComponent<SpriteRenderer>().sprite = PhotonTool.instance.player2TurnImages[0];
    }

    [PunRPC]
    public void StartGame()
    {
        if(gameState == EGameState.PlayingGame)
            return;

        if(photonView.IsMine)
        {
            foreach (var player in players)
            {
                player.GetPhotonView().RPC("RPC_SetupHealth", RpcTarget.All);
            }
            gameState = EGameState.PlayingGame;
        }

        deck.MakeCards();
        Deck.Shuffle(ref deck.cards);

        drawPile = ConvertListCardToListCardPair(deck.cards);
        LayoutGame();


        // 3초동안 보여주기
        foreach (var card in tableau)
        {
            card.faceup = false;
        }
        StartCoroutine(ShowRandomCard());
    }
    
    public void EndGame(int winPlayerViewID, bool restart)
    {
        Sprite winSprite = PhotonTool.instance.winnerImages;
        if (winSprite == null)
            return;

        PhotonView winner = PhotonView.Find(winPlayerViewID);
        if (winner == null)
            return;

        // 승리 이미지는 호스트, 클라이언트 모두 볼 수 있어야 한다.
        GameObject winnerBackground = new GameObject("winner");
        winnerBackground.transform.localScale = new Vector3(4, 4, 1);
        winnerBackground.transform.localPosition = new Vector3(0, 1, 0);
        Destroy(winnerBackground, 3);

        SpriteRenderer winnerBackgroundSprite = winnerBackground.AddComponent<SpriteRenderer>();
        winnerBackgroundSprite.sprite = winSprite;
        winnerBackgroundSprite.sortingOrder = 4;

        GameObject winnerObject = new GameObject("winnerImage");
        winnerObject.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        winnerObject.transform.localPosition = new Vector3(0, 2, 0);
        Destroy(winnerObject, 3);

        SpriteRenderer winnerSprite = winnerObject.AddComponent<SpriteRenderer>();
        winnerSprite.sprite = winner.GetComponent<SpriteRenderer>().sprite;
        winnerSprite.sortingOrder = 3;

        if(photonView.IsMine)
        {
            photonView.RPC("CloseTurn", RpcTarget.All);
        }

        // 아래 내용은 오직 한번만 실행되어야 한다.
        if (gameState != EGameState.PlayingGame)
            return;
        
        if (photonView.IsMine)
        {
            photonView.RPC("ClearCardList", RpcTarget.MasterClient);
        }

        if (restart)
        {
            StartCoroutine(_EndGame());
        }

        gameState = EGameState.End;
    }
    #endregion

    #region IEnumerator
    private IEnumerator ShowRandomCard()
    {
        blockFlip = true;
        int ndx;
        List<CardPair> cardList = new List<CardPair>();
        cardList.AddRange(tableau);
        while (cardList.Count > 0)
        {
            ndx = Random.Range(0, cardList.Count);
            cardList[ndx].Flip(true);
            cardList.RemoveAt(ndx);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(3f);

        foreach (var card in tableau)
            card.Flip(false);

        for (int i = 0; i < 3; i++)
            AudioSource.PlayClipAtPoint(PhotonTool.instance.flipSound, Vector3.zero);

        blockFlip = false;
        photonView.RPC("SetTurn", RpcTarget.All, Random.Range(0, 2));
    }

    IEnumerator _EndGame()
    {
        Room room = PhotonNetwork.CurrentRoom;

        yield return new WaitForSeconds(3);

        // 3초뒤 방이 달라질(또는 나갈) 경우 실행을 중지한다.
        if (PhotonNetwork.CurrentRoom != room)
            yield break;
        
        photonView.RPC("StartGame", RpcTarget.MasterClient);
    }
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentTurn);
            stream.SendNext(chargedPoint);
            stream.SendNext(chainCount);
            stream.SendNext((byte)gameState);
        }
        else
        {
            currentTurn = (int)stream.ReceiveNext();
            chargedPoint = (int)stream.ReceiveNext();
            chainCount = (int)stream.ReceiveNext();
            gameState = (EGameState)stream.ReceiveNext();
        }
    }
}
