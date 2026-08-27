using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Video;
using JetBrains.Annotations;
using Unity.VisualScripting;



public enum GameState {
    Menu,
    Cutscene,
    Readying,
    Countdown,
    Playing,
    CatWin,
    RatWin
}

public class GameManager : MonoBehaviour
{

    public static GameManager current;

    [Header("Title Screen")]
    public GameObject StartButton;
    public GameObject ControlsButton;
    public GameObject CatMenuUI;
    public GameObject RatMenuUI;
    public VideoPlayer SpinVideo;
    public VideoPlayer CutsceneC;
    public VideoPlayer CutsceneR;

    [Header("Controls Screen")]
    public GameObject ControlsPopup;
    public GameObject ControlsPopupC;
    public GameObject StartMarker;
    public GameObject StartMarkerC;
    public GameObject EndMarker;
    public GameObject EndMarkerC;
    public GameObject ClickOffButton;
    public float popupDuration;

    [Header("Confirmation Screen")]
    public bool catConfirmed;
    public bool ratConfirmed;
    //CV = cat view RV = rat view
    public GameObject CatConfirmUI;
    public GameObject RatConfirmUI;
    public GameObject CCatIndicator;
    public GameObject CRatIndicator;
    public GameObject RCatIndicator;
    public GameObject RRatIndicator;
    public GameObject CatGuideText;
    public GameObject RatGuideText;


    [Header("WinScreen")]
    public GameObject CWinScreenUI;
    public GameObject CLoseScreenUI;
    public GameObject RWinScreenUI;
    public GameObject RLoseScreenUI;
    public float winScreenFadeOutSpeed;
    public GameObject[] Overlays;
    public AudioSource winScreenMusic;
    public AudioClip CatWinMusic;
    public AudioClip RatWinMusic;

    [Header("Timer")]
    [Tooltip("How long the game lasts")]public float gameTime = 300;
    private float timer;
    private string timerText;
    public TextMeshProUGUI CatTimer;
    public TextMeshProUGUI RatTimer;

    [Header("Game UI")]
    public GameObject WaterGunBar;

    //Slams
    public Sprite LeftButtonAir;
    public Sprite LeftButtonGround;
    public Sprite GamepadAir;
    public Sprite GamepadGround;
    public bool inAir;

    public Image WaterGunFill;
    public GameObject CCountdownTextObject;
    public TextMeshProUGUI CCountdownText;
    public GameObject RCountdownTextObject;
    public TextMeshProUGUI RCountdownText;
    public GameObject Gamepad;
    public GameObject CGameUI;
    public GameObject RGameUI;
    public GameObject RightButton;
    public GameObject LeftButton;
    public GameObject DPad;
    public GameObject LMB;
    public GameObject RMB;
    public GameObject UpArrow;
    public GameObject DownArrow;

    private Image LeftButtonImage;
    private Image GamepadImage;

    private Color RightButtonColor;
    private Color LeftButtonColor;
    private Color DPadColor;
    private Color LMBColor;
    private Color RMBColor;
    public Color UpArrowColor;
    public Color DownArrowColor;
    public float buttonFadeSpeed;
    public GameObject WaterSplash;
    public float WaterEvaporationSpeed;

    [Header("Cheese")]
    public GameObject CheesePrefab;
    public int numCheeseSpawned = 3;
    public GameObject[] CheesesFillUI;
    public int cheeseNeeded;
    private int cheeseCount = 0;
    public Vector3 TutorialCheesePos;
    public float ratWinDelay;

    [Header("States")]
    public GameState state;

    [Header("Scene Pointers")]
    public String menu = "Menu";
    public String game = "Game";
    public String winScreen = "Win Screen";

    [Header("Sounds")] 
    private AudioSource audioSource;
    public AudioClip menuClick;
    //add more if needed

    void Awake() {
        if (current)
        {
            Destroy(gameObject);
        }
        else
        {
            current = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        state = GameState.Menu;
        catConfirmed = false;
        ratConfirmed = false;
        

        cheeseCount = 0;

        RightButtonColor = RightButton.GetComponent<Image>().color;
        LeftButtonColor = LeftButton.GetComponent<Image>().color;
        DPadColor = DPad.GetComponent<Image>().color;
        LMBColor = LMB.GetComponent<Image>().color;
        RMBColor = RMB.GetComponent<Image>().color;
        UpArrowColor = UpArrow.GetComponent<Image>().color;
        DownArrowColor = DownArrow.GetComponent<Image>().color;

        LeftButtonImage = LeftButton.GetComponent<Image>();
        GamepadImage = Gamepad.GetComponent<Image>();

        StartSpinVideo();
       
        TwoMonitorDisplay();
        
        Debug.Log("voided start");
    }

    void Update()
    {
        if(state == GameState.Readying)
        {
            if(InputManager.current.CatMove.action.WasPressedThisFrame())
            {
                ClickSound();
                ConfirmCat();
            }
            if(InputManager.current.RatMove.action.WasPressedThisFrame())
            {
                ClickSound();
                ConfirmRat();
            }
        }
        if(state == GameState.Playing)
        {
            if(timer >= 0) {
                timer -= Time.deltaTime;
                timerText = ((int)(timer/60)).ToString() + ":" +  ((int)(timer%60)).ToString("D2");
                CatTimer.text = timerText;
                RatTimer.text = timerText;
            } else
            {
                CatWin();
            }
        }

        //remember that inputs should always be taken in upd, not fixed

        if(InputManager.current.CatMove.action.WasPressedThisFrame())
            {
                RightButtonColor.a = 1;
            }
            if (InputManager.current.CatJump.action.WasPressedThisFrame())
            {
                LeftButtonColor.a = 1;
            } 
            if (InputManager.current.CatTurn.action.WasPressedThisFrame())
            {
                DPadColor.a = 1;
            }
            if (InputManager.current.RatBlast.action.WasPressedThisFrame())
            {
               LMBColor.a = 1;
            }
            if (InputManager.current.RatPickUp.action.WasPressedThisFrame())
            {
               RMBColor.a = 1;
            }
            if (InputManager.current.RatMove.action.WasPressedThisFrame())
            {
                UpArrowColor.a = 1;
            }
            if (InputManager.current.RatJump.action.WasPressedThisFrame())
            {
                DownArrowColor.a = 1;
            }

            if(state == GameState.CatWin || state == GameState.RatWin)
        {
            if(InputManager.current.CatJump.action.WasPressedThisFrame() || InputManager.current.CatMove.action.WasPressedThisFrame()) {
                StartCoroutine(DelayedMenu());
            }
        }
            
    }

    void FixedUpdate()
    {
        //control UI

            DimButton(ref RightButtonColor);
            DimButton(ref LeftButtonColor);
            DimButton(ref DPadColor);
            DimButton(ref RMBColor);
            DimButton(ref LMBColor);
            DimButton(ref UpArrowColor);
            DimButton(ref DownArrowColor);

            UpdateSlamUI();

            //water splash
            DimWaterSplash();

            UpdateButtonColors();

        
    }
    
    public void UpdateSlamUI()
    {
        if (inAir)
        {
            LeftButtonImage.sprite = LeftButtonAir;
            GamepadImage.sprite = GamepadAir;
            
        }
        else
        {
            LeftButtonImage.sprite = LeftButtonGround;
            GamepadImage.sprite = GamepadGround;
        }
    }

    //screen
    public void TwoMonitorDisplay()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }

    //Delay

    public IEnumerator DelayedMenu()
    {
        yield return new WaitForSeconds(0.5f);
        CallMenu();       
    }
    
    //upd funcs
    private void DimButton(ref Color color)
    {
        if(color.a >= 0) {
            color.a -= buttonFadeSpeed;
        }
    }
    
    private void UpdateButtonColors()
    {
        RightButton.GetComponent<Image>().color = RightButtonColor;
        LeftButton.GetComponent<Image>().color = LeftButtonColor;
        DPad.GetComponent<Image>().color = DPadColor;
        LMB.GetComponent<Image>().color = LMBColor;
        RMB.GetComponent<Image>().color = RMBColor;
        UpArrow.GetComponent<Image>().color = UpArrowColor;
        DownArrow.GetComponent<Image>().color = DownArrowColor;
    }

    public void DimWaterSplash()
    {
        Image splashImage = WaterSplash.GetComponent<Image>();
        Color color = splashImage.color;
        if(color.a >= 0) {
            color.a -= WaterEvaporationSpeed;
        }
        splashImage.color = color;
    }
    public void ShowWaterSplash()
    {
        Image splashImage = WaterSplash.GetComponent<Image>();
        Color color = splashImage.color;
        color.a = 1f;
        splashImage.color = color;
    }

    //menu video
    void StartCutscene()
    {
        CutsceneC.Stop();
        CutsceneR.Stop();
    }

    void StartSpinVideo()
    {
        SpinVideo.Play();
    }

    //Confirmation Screen ---------------------------------------------------------------------------------------------------------
    public void ConfirmCat()
    {
        if(state != GameState.Readying) {return;}
        if(!catConfirmed) {
            RCatIndicator.SetActive(true);
            CCatIndicator.SetActive(true);
            CatGuideText.SetActive(false);
            catConfirmed = true;
            CheckToStart();
        } else
        {
            RCatIndicator.SetActive(false);
            CCatIndicator.SetActive(false);
            CatGuideText.SetActive(true);
            catConfirmed = false;
        }
    }
    public void ConfirmRat()
    {
        if(state != GameState.Readying) {return;}
         if(!ratConfirmed) {
            RRatIndicator.SetActive(true);
            CRatIndicator.SetActive(true);
            RatGuideText.SetActive(false);
            ratConfirmed = true;
            CheckToStart();
        } else
        {
            RRatIndicator.SetActive(false);
            CRatIndicator.SetActive(false);
            RatGuideText.SetActive(true);
            ratConfirmed = false;
        }
    }

     public void ResetConfirmUI()
    {
        RCatIndicator.SetActive(false);
        CCatIndicator.SetActive(false);
        CatGuideText.SetActive(true);
        catConfirmed = false;
        RRatIndicator.SetActive(false);
        CRatIndicator.SetActive(false);
        RatGuideText.SetActive(true);
        ratConfirmed = false;
        InputManager.current.EnableCat();
        InputManager.current.EnableRat();
    }
    //public IEnumerator Breathe()

    //State Changes ------------------------------------------------------------------------------------------------------------------------
    public void StartReadyUp()
    {
        ClickSound();
        
        state = GameState.Readying;

        ResetConfirmUI();
        ConfirmUIActive(true);
        TitleUIActive(false);
    }

    public void CallMenu()
    {
        StartCoroutine(BackToMenu());
    }

    public IEnumerator BackToMenu()
    {
        state = GameState.Menu;
        AsyncOperation operation = SceneManager.LoadSceneAsync(menu);
        while(!operation.isDone)
        {
            yield return null;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        catConfirmed = false;
        ratConfirmed = false;

        winScreenMusic.Stop();

        ConfirmUIActive(false);
        WinScreenActive(false);
        TitleUIActive(true);
    }

    //Cheese -----------------------------------------------------------------------------------------------------------------------

    public void UpdateCheeses(int change)
    {
        cheeseCount += change;
        if(cheeseCount>=cheeseNeeded) {
            StartCoroutine(RatWinDelay());
            InputManager.current.DisableCat();
            InputManager.current.DisableRat();
        }
        for(int i = 0; i < cheeseNeeded; i++)
        {
            if(i<cheeseCount) { 
                CheesesFillUI[i].SetActive(true);
            } else
            {
                CheesesFillUI[i].SetActive(false);
            }
        }
        
    }

    private IEnumerator RatWinDelay()
    {
        yield return new WaitForSeconds(ratWinDelay);
        RatWin();        
    }

    //Water Gun ------------------------------------------------------------------------------------------------------------------
    public void ShowWaterGun(bool s)
    {
        WaterGunBar.SetActive(s);
    }
    public void SetWaterGun(float capacity, float current)
    {
        WaterGunFill.fillAmount = current/capacity;
    }

    //Starting Game Sequence -----------------------------------------------------------------------------------------------
    
    public IEnumerator TryToStart() //increases aura trust
    {
        cheeseCount = 0;
        UpdateCheeses(0);
        InputManager.current.DisableCat();
        InputManager.current.DisableRat();
        AsyncOperation operation = SceneManager.LoadSceneAsync(game);
        while(!operation.isDone)
        {
            yield return null;
        }
        ConfirmUIActive(false);
        StartCoroutine(Countdown(3));
    }
    void CheckToStart()
    {

        if(ratConfirmed && catConfirmed && state == GameState.Readying)
        {
            state = GameState.Countdown;
            StartCoroutine(TryToStart());
        }
    }

    public void StartPlay()
    {
        Debug.Log("Started Play");
        timer = gameTime;
        state = GameState.Playing;
        PlaceCheese();
        InputManager.current.EnableCat();
        InputManager.current.EnableRat();
    }

    public void PlaceCheese()
    {
        Transform spawnParent =
        GameObject.Find("CheeseSpawnLocations").transform;

        List<Transform> places = new List<Transform>();

        foreach (Transform child in spawnParent)
        {
            places.Add(child);
        }

        Debug.Log($"Actual spawn locations: {places.Count}");

        int spawnAmount = Mathf.Min(numCheeseSpawned, places.Count);

        Instantiate(CheesePrefab, TutorialCheesePos, Quaternion.identity);

        for (int i = 0; i < spawnAmount; i++)
        {
            int index = Random.Range(0, places.Count);

            Debug.Log( $"Cheese {i + 1}: index {index}, " + $"location {places[index].name}");

            Transform selectedPlace = places[index];
            Instantiate(CheesePrefab, selectedPlace.position, selectedPlace.rotation
        );

        places.RemoveAt(index);
    }
}

    public IEnumerator Countdown(int seconds)
    {
        GameUIActive(true);
        CountdownsActive(true);
        int count = seconds;
        while(count > 0)
        {
            CCountdownText.text = count.ToString();
            RCountdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }
        CountdownsActive(false);
        StartPlay();
    }

    //Winning ------------------------------------------------------------------------------------------------------------------

    public void CatWin()
    {
        state = GameState.CatWin;
        PlayWinMusic();
        StartCoroutine(LoadWinScreen(true));
        
        //StartCoroutine(FadeOutWinScreen());
    }

    public void RatWin()
    {
        state = GameState.RatWin;
        PlayWinMusic();
        StartCoroutine(LoadWinScreen(false));

        //StartCoroutine(FadeOutWinScreen());
    }

    public IEnumerator LoadWinScreen(bool isCat)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(winScreen);
        while(!operation.isDone)
        {
            yield return null;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(isCat) 
        {
        CWinScreenUI.SetActive(true);
        RLoseScreenUI.SetActive(true);
        GameUIActive(false);
        } else
        {
        CLoseScreenUI.SetActive(true);
        RWinScreenUI.SetActive(true);
        GameUIActive(false);
        }
    }

    public void PlayWinMusic()
    {
        if(state == GameState.CatWin)
        {
            winScreenMusic.clip = CatWinMusic;
            winScreenMusic.Play();
        }
        else if(state == GameState.RatWin)
        {
            winScreenMusic.clip = RatWinMusic;
            winScreenMusic.Play();
        }
    }
/* overcomplicated fade out
    public IEnumerator FadeOutWinScreen()
    {
        Color originalColor = Overlays[0].GetComponent<Image>().color;
        float overlayOpacity = originalColor.a;
        while(overlayOpacity < 1)
        {
            overlayOpacity += winScreenFadeOutSpeed;
            ChangeOverlayColor(overlayOpacity);
            yield return new WaitForFixedUpdate();
        }
        ChangeOverlayColor(originalColor); 
        BackToMenu();
    }

    public void ChangeOverlayColor(float opacity)
    {
        foreach(GameObject ov in Overlays)
        {
            Color changeColor = ov.GetComponent<Image>().color;
            changeColor.a = opacity;
            ov.GetComponent<Image>().color = changeColor;
        }    
    }
    public void ChangeOverlayColor(Color color)
    {
        foreach(GameObject ov in Overlays)
        {
            ov.GetComponent<Image>().color = color;
        }
    }
*/
    //Controls Popup -----------------------------------------------------------------------------------
    public void OpenControls()
    {
        audioSource.PlayOneShot(menuClick);
        MenuButtonsActive(false);
        ClickOffButton.SetActive(true);
        StartCoroutine(PopupControls(true));
    }

    public IEnumerator PopupControls(bool appear)
    {
        Vector3 startPos = ControlsPopup.transform.position;
        Vector3 targetPos = appear ? EndMarker.transform.position : StartMarker.transform.position;

        Vector3 startPosC = ControlsPopupC.transform.position;
        Vector3 targetPosC = appear ? EndMarkerC.transform.position : StartMarkerC.transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < popupDuration)
        {
            float completion = elapsedTime / popupDuration;

            ControlsPopup.transform.position = Vector3.Lerp(startPos, targetPos, completion);
            ControlsPopupC.transform.position = Vector3.Lerp(startPosC, targetPosC, completion);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ControlsPopup.transform.position = targetPos;
        ControlsPopupC.transform.position = targetPos;
    }


    public void ClickOff()
    {
        ClickOffButton.SetActive(false);
        audioSource.PlayOneShot(menuClick);
        MenuButtonsActive(true);
        StartCoroutine(PopupControls(false));
    }
    //UI Screen Activations -----------------------------------------------------------------------------------------------------
    public void WinScreenActive(bool s)
    {
        CWinScreenUI.SetActive(s);
        RLoseScreenUI.SetActive(s);
        CLoseScreenUI.SetActive(s);
        RWinScreenUI.SetActive(s);
    }

    public void TitleUIActive(bool s)
    {
        CatMenuUI.SetActive(s);
        RatMenuUI.SetActive(s);
    }
    public void ConfirmUIActive(bool s)
    {
       CatConfirmUI.SetActive(s);
       RatConfirmUI.SetActive(s);
    }
    public void GameUIActive(bool s)
    {
        CGameUI.SetActive(s);
        RGameUI.SetActive(s);
    }
    public void CountdownsActive(bool s)
    {
        CCountdownTextObject.SetActive(s);
        RCountdownTextObject.SetActive(s);
    
    }

    public void MenuButtonsActive(bool s)
    {
        StartButton.GetComponent<Button>().enabled = s;
        ControlsButton.GetComponent<Button>().enabled = s;
    }

    //Sound -------------------------------------------------------------------------------------------

    public void ClickSound()
    {
        audioSource.PlayOneShot(menuClick);
    }

    //Quit ------------------------------------------------------------------------------------------------------------------
    public void Quit()
    {
        Application.Quit();
    }
}
