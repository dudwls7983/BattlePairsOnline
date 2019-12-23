using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Deck : MonoBehaviour
{
    public static Deck instance;

    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabSprite;
    public GameObject prefabCard;

    public bool ________________;

    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string,Sprite> dictSuits;

    private void Awake()
    {
        instance = this;
    }

    //initDeck은 준비되면 gamemanager가 호출함
    public void initDeck(string deckXMLText) {
        if (deckAnchor == null)
        {
            GameObject anchorGO = GameObject.Find("_Deck");
            if(anchorGO == null)
                anchorGO = new GameObject("_Deck");

            deckAnchor = anchorGO.transform;
        }
        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C",suitClub},
            {"D",suitDiamond },
            {"H",suitHeart },
            {"S",suitSpade }
        };
        ReadDeck(deckXMLText);
    }
    //ReadDeck은 전달된 XML파일을 해석하고 CardDefintion에 저장
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);


        string s = "xml[0] decorator[0]";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += "x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += "y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += "scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        //모든 카드의 데코레이터를 읽음
        decorators = new List<Decorator>();
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;

        for (int i = 0; i < xDecos.Count; i++)
        {
            deco = new Decorator();

            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"));
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));

            decorators.Add(deco);
        }
        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for (int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale")) {
                        deco.scale = float.Parse(xPips[j].att("scale"));

                    }
                    cDef.pips.Add(deco);

                }

            }
            if (xCardDefs[i].HasAtt("face")) {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach(CardDefinition cd in cardDefs)
        {
            if (cd.rank == rnk)
            {
                return (cd);
            }
        }
        return (null);
    }

    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach(string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)
        {
            GameObject cgo = PhotonTool.instance.Instantiate("_Prefabs/PrefabCard");
            CardPair card = cgo.GetComponent<CardPair>();
            card.name = cardNames[i];
            card.suit = cardNames[i][0].ToString();
            card.rank = int.Parse(card.name.Substring(1));

            if (card.suit == "D" || card.suit == "H")
                card.color = Color.red;
            else
                card.color = Color.black;

            card.photonView.RPC("RPC_SetupCard", RpcTarget.All, card.name, card.suit, card.rank, new Vector3(card.color.r, card.color.g, card.color.b), deckAnchor.name, 
                new Vector3((i % 13) * 3, i / 13 * 4, 0));

            cards.Add(card);
         
        }
    }//MakeCard()
    public static void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int ndx;
        tCards = new List<Card>();
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }
    public Sprite GetFace(string faceS)
    {
        foreach (Sprite tS in faceSprites)
        {
            if (tS.name == faceS)
                return (tS);
        }
        return (null);
    }
    public Sprite GetSuit(string suit)
    {
        switch(suit)
        {
            case "C":
                return (suitClub);
            case "D":
                return (suitDiamond);
            case "H":
                return (suitHeart);
            case "S":
                return (suitSpade);
        }
        return (null);
    }


}
