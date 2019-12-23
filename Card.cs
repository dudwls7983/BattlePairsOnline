using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : Photon.Pun.MonoBehaviourPun
{
    public string suit;
    public int rank;
    public Color color = Color.black;
    public List<GameObject> decoGOs = new List<GameObject>();
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back;
    public CardDefinition def;

    public bool faceup
    {
        get
        {
            if (back == null)
                return false;

            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }
}

[System.Serializable]
public class Decorator
{
    // DeckXML에서 얻은 각 데코레이터나 핍에대한 정보를 저장.

    public string type; // 카드 핍의 경우 type = "pip"
    public Vector3 loc; //카드에서 스프라이트 위치
    public bool flip = false; //스프라이트를 세로로 뒤집는지 여부
    public float scale = 1f;
}
[System.Serializable]
public class CardDefinition {

    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>();



}
