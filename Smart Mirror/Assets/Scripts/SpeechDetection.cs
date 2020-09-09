using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System.Collections;
using System.IO;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SpeechDetection : MonoBehaviour
{
    [SerializeField]
    private Text outputText;
    [SerializeField]
    private Button startRecoButton;
    [SerializeField]
    private GameObject[] myObjects;
    [SerializeField]
    private Image[] images;
    [SerializeField]
    private Text[] filternames;
    [SerializeField]
    private Canvas calibrationCanvas;
    [SerializeField]
    private Canvas filterCanvas;
    [SerializeField]
    private Canvas timeCanvas;
    [SerializeField]
    private Canvas weatherCanvas;
    [SerializeField]
    private Image mix;

    [SerializeField]
    private GameObject UIMenu;
    [SerializeField]
    private GameObject HelloCircle;
    [SerializeField]
    private GameObject CameraCircle;
    [SerializeField]
    private GameObject PhotosCircle;
    [SerializeField]
    private GameObject MusicCircle;
    [SerializeField]
    private GameObject HelpCircle;
    [SerializeField]
    private GameObject SlideShow;
    [SerializeField]
    private GameObject OpenMouth;

    [SerializeField]
    private GameObject rawImagea;
    [SerializeField]
    private GameObject musicPlayer;
    [SerializeField]
    private GameObject MyPhotos;

    public Text countdownText;
    public bool showingPhotos = false;
    public bool takingPhoto = false;

    private GameObject actualObj;
    private GameObject previousObj = null;
    private object threadLocker = new object();
    private string message;
    private bool waitingForReco;
    private bool micPermissionGranted = false;
    private bool inHello = false;
    private bool android = false;
    private bool InternetConn;
    private bool onetime = false;
    private bool internetCame;
    private bool isDetecting = false;
    private int isFirst = 0;
    private int count;
    private int photocount;
    private int timeLeft;

#if PLATFORM_ANDROID
    // Required to manifest microphone permission, cf.
    // https://docs.unity3d.com/Manual/android-manifest.html
    private Microphone mic;
#endif
    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            android = true;
        }

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            weatherCanvas.gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>().enabled = false;
            weatherCanvas.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            InternetConn = false;
        }
        else
        {
            weatherCanvas.gameObject.SetActive(true);
            InternetConn = true;
            ButtonClick();
        }
        
        GetComponent<GetWeather>().Request();

        if (outputText == null)
        {
            Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else if (startRecoButton == null)
        {
            Debug.LogError(message);
        }
        else
        {
            // Continue with normal initialization, Text and Button objects are present.

#if PLATFORM_ANDROID
            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            //message = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#else
            micPermissionGranted = true;
            //message = "Click button to recognize speech";
#endif
            //startRecoButton.onClick.AddListener(ButtonClick);
        }
    }

    void Update()
    {
#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;
        }
#endif
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            InternetConn = false;
            if (!internetCame)
            {
                StartMirror();
                //OpenMouth.SetActive(true);
                internetCame = true;
                weatherCanvas.gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>().enabled = false;
                weatherCanvas.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        else
        {
            InternetConn = true;
            if (internetCame)
            {
                StartMirror();
                GetComponent<GetWeather>().Request();
                weatherCanvas.gameObject.SetActive(true);
                weatherCanvas.gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>().enabled = true;
                weatherCanvas.gameObject.transform.GetChild(2).gameObject.SetActive(true);
                internetCame = false;
            }
        }
        lock (threadLocker)
        {
            if (startRecoButton != null)
            {
                startRecoButton.interactable = !waitingForReco && micPermissionGranted;
            }
            if (outputText != null)
            {
                outputText.text = message;              
            }
            if (InternetConn && isDetecting)
            {
                mix.gameObject.SetActive(true);
                if (message != null)
                {
                    if (!onetime)
                    {
                        onetime = true;
                        if (message.Contains("hello"))
                        {
                            mix.color = Color.green;
                            SayHello();           
                        }

                        if (message.Contains("goodbye"))
                        {
                            mix.color = Color.green;
                            Idle();
                            HideUIMenu();
                        }

                        if (message.Contains("music"))
                        {
                            mix.color = Color.green;
                            musicPlayer.GetComponent<MusicPlayer>().playMusic();
                            HelpCircle.GetComponent<Animator>().SetBool("isActiveHelp", true);
                            HideUIMenu();
                        }

                        if (message.Contains("pause"))
                        {
                            mix.color = Color.green;
                            musicPlayer.GetComponent<MusicPlayer>().pauseMusic();
                        }

                        if (message.Contains("help"))
                        {
                            mix.color = Color.green;
                            ShowUIMenu();
                        }                        
                        if (message.Contains("camera") || message.Contains("photo"))
                        { 
                            mix.color = Color.green;
                            HideUIMenu();
                            StartCoroutine("TakeAPicture");
                        }

                        if (message.Contains("gallery") || message.Contains("my pictures"))
                        {     
                            mix.color = Color.green;
                            HideUIMenu();  
                            ShowMyPhotos();
                        }
                        if (inHello)
                        {
                            if (message.Contains("glasses"))
                            {
                                ChangeImage("Glasses");
                                showObjects(myObjects[0]);
                            }

                            if (message.Contains("beard"))
                            {
                                ChangeImage("Beard");
                                showObjects(myObjects[1]);
                            }

                            if (message.Contains("cat"))
                            {
                                ChangeImage("Cat");
                                showObjects(myObjects[2]);
                            }

                            if (message.Contains("princess"))
                            {
                                ChangeImage("Princess");
                                showObjects(myObjects[3]);
                            }

                            if (message.Contains("devil") || message.Contains("devel"))
                            {
                                ChangeImage("Devil");
                                showObjects(myObjects[4]);
                            }

                            if (message.Contains("mask"))
                            {
                                ChangeImage("Mask");
                                showObjects(myObjects[5]);
                            }

                            if (message.Contains("wolf") || message.Contains("who is"))
                            {
                                ChangeImage("Wolf");
                                showObjects(myObjects[6]);
                            }
                        }

                        if (message.Contains("normal"))
                        {
                            mix.color = Color.green;
                            HideFilters();
                            MyPhotos.SetActive(false);
                        }

                        //---Filter controller---

                        if (message.Contains("remove buttons"))
                        {
                            calibrationCanvas.gameObject.SetActive(false);
                            mix.color = Color.green;
                        }
                        if (message.Contains("display buttons"))
                        {
                            calibrationCanvas.gameObject.SetActive(true);
                            mix.color = Color.green;
                        }
                        if (message.Contains("remove face"))
                        {
                            rawImagea.GetComponent<RawImage>().enabled = false;
                            mix.color = Color.green;
                        }
                        if (message.Contains("face detection"))
                        {
                            rawImagea.GetComponent<RawImage>().enabled = true;
                            mix.color = Color.green;
                        }
                        if (message.Contains("reset count"))
                        {
                            PlayerPrefs.SetInt("photoNr", 0);
                            mix.color = Color.green;
                        }       
                        Next();
                    }
                }
            }
        }
    }

    public IEnumerator TakeAPicture()
    {
        takingPhoto = true;
        rawImagea.GetComponent<RawImage>().enabled = true;
        photocount = PlayerPrefs.GetInt("photoNr");
        yield return new WaitForSeconds(1f);

        ScreenCapture.CaptureScreenshot("photo" + photocount + ".jpg"); 
        photocount ++;
        PlayerPrefs.SetInt("photoNr", photocount);
        Invoke("ShowMyPhotos", 1.1f);
        HideUIMenu();
    }

    IEnumerator LoseTime()
    {
        while (timeLeft>0)
        {
            countdownText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1);
            timeLeft--;
        }
        countdownText.gameObject.SetActive(false);
    }

    public void SayHello()
    {
        HelloCircle.GetComponent<Animator>().SetBool("isActiveHello", false);
        CameraCircle.GetComponent<Animator>().SetBool("SayHello", true);
        PhotosCircle.GetComponent<Animator>().SetBool("isActivePhotos", false);
        MusicCircle.GetComponent<Animator>().SetBool("isActiveMusic", false);       
        weatherCanvas.GetComponent<Animator>().SetBool("idle", true);
        timeCanvas.GetComponent<Animator>().SetBool("idle", true);
        HelpCircle.GetComponent<Animator>().SetBool("isActiveHelp", true);
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", true);
        MyPhotos.SetActive(false);
        RandomFilter();
    }

    public void ShowUIMenu()
    {
        OpenMouth.SetActive(false);
        isDetecting = true;
        MyPhotos.SetActive(false);
        HideFilters();
        weatherCanvas.GetComponent<Animator>().SetBool("idle", true);
        timeCanvas.GetComponent<Animator>().SetBool("idle", true);
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", true);
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", true);
        musicPlayer.GetComponent<MusicPlayer>().pauseMusic();
        HelpCircle.GetComponent<Animator>().SetBool("isActiveHelp", false);
        HelloCircle.GetComponent<Animator>().SetBool("isActiveHello", true);
        if (CameraCircle.GetComponent<Animator>().GetBool("SayHello"))
        {
            CameraCircle.GetComponent<Animator>().SetBool("SayHello", false);
        }
        CameraCircle.GetComponent<Animator>().SetBool("isActiveCamera", true);
        PhotosCircle.GetComponent<Animator>().SetBool("isActivePhotos", true);
        MusicCircle.GetComponent<Animator>().SetBool("isActiveMusic", true);
    }

    public void HideUIMenu()
    {
        HelloCircle.GetComponent<Animator>().SetBool("isActiveHello", false);
        if (CameraCircle.GetComponent<Animator>().GetBool("SayHello"))
        {
            CameraCircle.GetComponent<Animator>().SetBool("SayHello", false);
        }
        CameraCircle.GetComponent<Animator>().SetBool("isActiveCamera", false);
        PhotosCircle.GetComponent<Animator>().SetBool("isActivePhotos", false);
        MusicCircle.GetComponent<Animator>().SetBool("isActiveMusic", false);
    }

    private void DisableeUIMenu()
    {
        UIMenu.SetActive(false);
    }

    public void StartSlideShow()
    {
        musicPlayer.GetComponent<MusicPlayer>().playMusic();
        SlideShow.SetActive(true);
        HideUIMenu();
        HideFilters();
        OpenMouth.SetActive(false);
    }

    void StartWIthNoConnection()
    {
        if (!InternetConn)
        {
            mix.gameObject.SetActive(false);
            HideUIMenu();
            OpenMouth.SetActive(true);
            OpenMouth.GetComponent<Animator>().SetBool("IsActiveMouth", true);
            RandomFilter();
        }
    }

    public void StartMirror()
    {
        isDetecting = true;
        HelpCircle.GetComponent<Animator>().SetBool("isActiveHelp", false);
        SlideShow.SetActive(false);
        MyPhotos.SetActive(false);
        musicPlayer.GetComponent<MusicPlayer>().pauseMusic();
        weatherCanvas.GetComponent<Animator>().SetBool("idle", true);
        timeCanvas.GetComponent<Animator>().SetBool("idle", true);
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", true);
        if(InternetConn)
        {
          //  GetComponent<UIManager>().SpeechPlayback();
            ShowUIMenu();
            ButtonClick();
            OpenMouth.SetActive(false);
        }
        else
        {
            mix.gameObject.SetActive(false);
            Invoke("StartWIthNoConnection", 0.2f);
        }
    }

    public void Idle()
    {
        isDetecting = false;
        HelpCircle.GetComponent<Animator>().SetBool("isActiveHelp", false);
        weatherCanvas.GetComponent<Animator>().SetBool("idle", false);
        timeCanvas.GetComponent<Animator>().SetBool("idle", false);
        musicPlayer.GetComponent<MusicPlayer>().pauseMusic();
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", false);
        HideFilters();
        HideUIMenu();
        Invoke("HideFilters", 0.5f);
        Invoke("HideUIMenu", 0.5f);
        OpenMouth.SetActive(false);
        if (InternetConn)
        {
            GetComponent<GetWeather>().Request();
            weatherCanvas.gameObject.SetActive(true);
            weatherCanvas.gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>().enabled = true;
            weatherCanvas.gameObject.transform.GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            weatherCanvas.gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>().enabled = false;
            weatherCanvas.gameObject.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    void DisableAllObjects()
    {
        for (int i = 0; i < myObjects.Length; i++)
        {
            myObjects[i].SetActive(false);
        }
    }

    public void RandomFilter()
    {
        MyPhotos.SetActive(false);
        weatherCanvas.GetComponent<Animator>().SetBool("idle", true);
        timeCanvas.GetComponent<Animator>().SetBool("idle", true);
        transform.parent.GetComponent<Animator>().SetBool("IsActiveFade", true);
        inHello = true;
        filterCanvas.gameObject.SetActive(true);
        string[] filters = new string[] { "Glasses", "Beard", "Cat", "Princess", "Devil", "Mask", "Wolf"};
        int rand = Random.Range(0,6);
        ChangeImage(filters[rand]);
        showObjects(myObjects[rand]);
        filterCanvas.GetComponent<Animator>().SetBool("isIdleF", true);
    }

    void HideFilters()
    {
        DisableAllObjects();
        if (filterCanvas.isActiveAndEnabled)
        {
            filterCanvas.GetComponent<Animator>().SetBool("isIdleF", false);
        }
    }

    private void HideFilterCanvas()
    {
        filterCanvas.gameObject.SetActive(false);
    }
    public IEnumerator mixColor()
    {
        yield return new WaitForSeconds(1);
        mix.color = Color.white;
    }

    void Next()
    {
        Invoke("ButtonClick", 0.5f);
    }

    void ChangeImage(string img)
    {
        mix.color = Color.green;
        while (images[images.Length/2].sprite.name != img)
        {
            Sprite temp = images[0].GetComponent<Image>().sprite;
            for (int j = 0; j < images.Length-1; j++)
            {
                images[j].GetComponent<Image>().sprite = images[j+1].GetComponent<Image>().sprite;
            }
            images[images.Length-1].GetComponent<Image>().sprite = temp;
        }
        for (int i = 0; i < filternames.Length; i++)
        {
            filternames[i].text = images[i].sprite.name;       
        }
    }

    void ShowMyPhotos()
    {
        isDetecting = false;
        takingPhoto = false;
        rawImagea.GetComponent<RawImage>().enabled = false;
        showingPhotos = true;
        HideFilters();
        MyPhotos.SetActive(true);  

        if (PlayerPrefs.GetInt("photoNr") < 6)
        {
            count = 0;
        }
        else
        {
            count = PlayerPrefs.GetInt("photoNr") - 6;
        }

        foreach(Transform child in MyPhotos.transform)
        {
            string photo = "/photo" + count + ".jpg";
            try
            {
                byte[] byteArray = File.ReadAllBytes(Application.persistentDataPath + photo);
                Texture2D texture = new Texture2D(8, 8);
                texture.LoadImage(byteArray);
                child.GetComponent<RawImage>().texture = texture;
            }
            catch{}
            count++;                               
        }

        Invoke("PhotosShowing", 8f);

        if (!InternetConn)
        {
            Invoke("RandomFilter", 8f);
        }
        else
        {
            Invoke("ShowUIMenu", 8f); 
        }
    }

    public void PhotosShowing()
    {
        showingPhotos = false;
    }

    void showObjects(GameObject obj)
    {
        if(isFirst == 0)
        {
            previousObj = null;
            actualObj = obj;
            isFirst = 1;
        }

        if(isFirst == 1)
        {
            previousObj = actualObj;
            actualObj = obj;
        }

        previousObj.SetActive(false);
        actualObj.SetActive(true);
        Update();
    }

    public async void ButtonClick()
    {
        StartCoroutine(mixColor());
        var config = SpeechConfig.FromSubscription("25744a577b3c452e8edc06fc79ac82d9", "westeurope");
        message = null;
        onetime = false;

        using (var recognizer = new SpeechRecognizer(config))
        {
            lock (threadLocker)
            {
                waitingForReco = true;
            }

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result.
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query.
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                newMessage = "Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
            }

            lock (threadLocker)
            {
                message = newMessage.ToLower();
                waitingForReco = false;
            }
        }
    }
}