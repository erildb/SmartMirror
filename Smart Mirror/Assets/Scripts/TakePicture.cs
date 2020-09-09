using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TakePicture : MonoBehaviour
{
    [SerializeField]
    private Text countdownText;
    [SerializeField]
    private Image PleaseWait;
    [SerializeField]
    private RawImage rawimage;

    private WebCamTexture webcamTexture = null;
    private int count;
    private int timeLeft;

    private void Start()
    {
        PlayerPrefs.SetInt("photoNr", 0);

        timeLeft = 3;
        if (Application.platform == RuntimePlatform.Android)
        {
            rawimage.transform.Rotate(0,0,90);
            rawimage.transform.localScale = new Vector3(1.765164f, 0.8662923f, 9.257f);
        }

        count = PlayerPrefs.GetInt("photoNr");
        string camName = null;
        WebCamDevice[] camDevices = WebCamTexture.devices;

        foreach(var camDevice in camDevices){ 
            if(camDevice.isFrontFacing){
                camName = camDevice.name;
                break;
            }
        }

        webcamTexture = new WebCamTexture(camName);
        rawimage.texture = webcamTexture;
        rawimage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
        StartCoroutine("LoseTime");
        Invoke("SaveImage", 3.1f);
    }

    private void SaveImage()
    {
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath +  "/photo" + count + ".jpg");
        count ++;
        PlayerPrefs.SetInt("photoNr", count);
        PlayerPrefs.SetInt("photoTaken", 1);
        webcamTexture.Stop();
        Destroy(webcamTexture);
        SceneManager.LoadSceneAsync("Smart Mirror");    
    }

    private IEnumerator LoseTime()
    {
        countdownText.text = timeLeft.ToString();

        while (timeLeft>0)
        {
            countdownText.text = (timeLeft.ToString());
            yield return new WaitForSeconds(1);
            timeLeft--;
        }

        countdownText.gameObject.SetActive(false);
        PleaseWait.gameObject.SetActive(true);
    }
}
