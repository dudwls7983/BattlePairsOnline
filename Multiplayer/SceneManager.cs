using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;
    public Dictionary<string, GameObject> dictionaryGameObjects;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void CreateManager()
    {
        GameObject gameObject = new GameObject("SceneManger");
        gameObject.AddComponent<SceneManager>();
    }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

        dictionaryGameObjects = new Dictionary<string, GameObject>();
    }

    private void Start()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        OnSceneChanged(currentScene, currentScene);
    }

    public void OnSceneChanged(UnityEngine.SceneManagement.Scene before, UnityEngine.SceneManagement.Scene after)
    {
        dictionaryGameObjects.Clear();
        
        switch(after.name)
        {
            #region Intro
            case "0.Intro":
                foreach (var gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    switch (gameObject.name)
                    {
                        case "Background":
                        case "Logo":
                        case "Button_Start":
                        case "Button_Rule":
                        case "Button_Quit":
                            dictionaryGameObjects.Add(gameObject.name, gameObject);
                            break;
                    }
                }

                StartCoroutine(Intro_AnimationScene());
                break;
            #endregion
            #region Main
            case "1.Main":
                GameObject panel = null;

                foreach (var gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    switch (gameObject.name)
                    {
                        case "Connecting":
                        case "Connected":
                        case "Disconnected":
                        case "Panel_Matching":
                        case "Panel_Howto":
                            dictionaryGameObjects.Add(gameObject.name, gameObject);
                            break;
                        case "Panel_PlayerImage":
                            panel = gameObject;
                            dictionaryGameObjects.Add(gameObject.name, gameObject);
                            break;
                        case "Button_Start":
                            dictionaryGameObjects.Add(gameObject.name, gameObject);
                            gameObject.GetComponent<Image>().sprite = PhotonTool.instance.startButtonImages[1];
                            gameObject.GetComponent<Button>().enabled = false;
                            break;
                    }
                }

                if(Photon.Pun.PhotonNetwork.IsConnected == false)
                {
                    Photon.Pun.PhotonNetwork.ConnectUsingSettings();
                    MainScene_UpdateState("Connecting");
                }
                else
                {
                    MainScene_UpdateState("Connected");
                }
                MainScene_UpdatePlayerImage(panel);
                break;
            #endregion
            #region Battle
            case "2.Battle":
                Debug.Log("Fire OnEntered Game");
                PhotonManager.instance.Invoke("OnEnteredGame", 0.5f);
                break;
                #endregion
        }
    }

    #region IntroScene
    IEnumerator Intro_AnimationScene()
    {
        GameObject background;
        GameObject logo;
        GameObject startButton;
        GameObject ruleButton;
        GameObject quitButton;

        bool success = true;
        var dictionary = instance.dictionaryGameObjects;


        success = dictionary.TryGetValue("Background", out background);
        if (success == false) yield break;

        success = dictionary.TryGetValue("Logo", out logo);
        if (success == false) yield break;

        success = dictionary.TryGetValue("Button_Start", out startButton);
        if (success == false) yield break;

        success = dictionary.TryGetValue("Button_Rule", out ruleButton);
        if (success == false) yield break;

        success = dictionary.TryGetValue("Button_Quit", out quitButton);
        if (success == false) yield break;
        
        
        // animation 시작
        yield return new WaitForSeconds(1);

        Image backgroundImage = background.GetComponent<Image>();
        background.SetActive(true);

        Color backgroundColor = new Color(1, 1, 1, 0);

        for (int i = 0; i < 40; i++)
        {
            backgroundColor.a += 0.025f;
            backgroundImage.color = backgroundColor;
            yield return new WaitForSeconds(0.025f);
        }

        yield return new WaitForSeconds(0.5f);


        Image logoImage = logo.GetComponent<Image>();
        logo.SetActive(true);

        Color logoColor = new Color(1, 1, 1, 0);

        for (int i = 0; i < 20; i++)
        {
            logoColor.a += 0.05f;
            logoImage.color = logoColor;
            yield return new WaitForSeconds(0.025f);
        }

        yield return new WaitForSeconds(0.5f);
        
        Vector3 rulePosition = ruleButton.transform.localPosition;
        rulePosition.y = -60f;
        Vector3 quitPosition = quitButton.transform.localPosition;
        quitPosition.y = -60f;

        startButton.SetActive(true);
        ruleButton.SetActive(true);
        quitButton.SetActive(true);

        for (int i = 0; i < 20; i++)
        {
            rulePosition.y -= 6.5f;
            quitPosition.y -= 9.5f;
            ruleButton.transform.localPosition = rulePosition;
            quitButton.transform.localPosition = quitPosition;
            yield return new WaitForSeconds(0.025f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("1.Main");
    }
    #endregion
    #region MainScene
    public static void MainScene_UpdateState(string state)
    {
        GameObject connecting;
        GameObject connected;
        GameObject disconnected;
        GameObject startButton;
        GameObject playerImagePanel;
        GameObject matchingPanel;
        GameObject howtoPanel;

        bool success = true;
        var dictionary = instance.dictionaryGameObjects;

        success = dictionary.TryGetValue("Connecting", out connecting);
        if (success == false) return;

        success = dictionary.TryGetValue("Connected", out connected);
        if (success == false) return;

        success = dictionary.TryGetValue("Disconnected", out disconnected);
        if (success == false) return;

        success = dictionary.TryGetValue("Button_Start", out startButton);
        if (success == false) return;

        success = dictionary.TryGetValue("Panel_PlayerImage", out playerImagePanel);
        if (success == false) return;

        success = dictionary.TryGetValue("Panel_Matching", out matchingPanel);
        if (success == false) return;

        success = dictionary.TryGetValue("Panel_Howto", out howtoPanel);
        if (success == false) return;

        switch (state)
        {
            case "Connected":
                connecting.SetActive(false);
                connected.SetActive(true);
                disconnected.SetActive(false);
                startButton.GetComponent<Image>().sprite = PhotonTool.instance.startButtonImages[0];
                startButton.GetComponent<Button>().enabled = true;
                break;
            case "Disconnected":
                connecting.SetActive(false);
                connected.SetActive(false);
                disconnected.SetActive(true);
                startButton.GetComponent<Image>().sprite = PhotonTool.instance.startButtonImages[1];
                startButton.GetComponent<Button>().enabled = false;
                break;
            case "SelectPlayerImage":
                playerImagePanel.SetActive(true);
                break;
            case "StartMatching":
                matchingPanel.SetActive(true);
                break;
            case "ShowHowto":
                howtoPanel.SetActive(true);
                break;
            case "HideHowto":
                howtoPanel.SetActive(false);
                break;
        }
    }

    public static void MainScene_UpdatePlayerImage(GameObject panel)
    {
        // 이미지 패널에 이미지 추가
        if (panel)
        {
            GameObject content = panel.transform.Find("PlayerImage").Find("Content").gameObject;
            Sprite[] sprites = PhotonTool.instance.playerImages;
            //content.GetComponent<RectTransform>().offsetMax = new Vector2((sprites.Length * 160) - 590, 0);
            for (int i = 0; i < sprites.Length; i++)
            {
                GameObject playerImage = Instantiate(Resources.Load("_Prefabs/PlayerImage")) as GameObject;
                playerImage.transform.SetParent(content.transform);
                playerImage.GetComponent<Image>().sprite = sprites[i];
                playerImage.name = i.ToString();
                playerImage.transform.localScale = Vector3.one;

                //RectTransform rectTransform = playerImage.GetComponent<RectTransform>();
                //rectTransform.offsetMin = new Vector2(i * 160, 10);
                //rectTransform.offsetMax = new Vector2((i * 160) - 600, 10);
            }
        }
    }
    #endregion
}
