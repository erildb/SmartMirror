using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ChangePhotos : MonoBehaviour
{
    private int photoCount = 0;
    private bool android = false;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            android = true;
        }
        else
        {
            android = false;
        }
    }

    public void ChangePhoto()
    {
        if (photoCount < PlayerPrefs.GetInt("photoNr") - 1)
        {
            photoCount += 1;
        }
        else
        {
            photoCount = 0;
        }

        string photo1 = "/photo" + photoCount + ".jpg";

        try
        {
            byte[] byteArray1 = File.ReadAllBytes(Application.persistentDataPath + photo1);
            if (byteArray1 != null)
            {
                Texture2D texture1 = new Texture2D(8, 8);
                texture1.LoadImage(byteArray1);
                GetComponent<RawImage>().texture = texture1;
            }
        }
        catch{}
    }

    private void OnEnable()
    {
        photoCount = 0;
        byte[] byteArray = File.ReadAllBytes(Application.persistentDataPath + "/photo0.jpg");
        Texture2D texture = new Texture2D(8, 8);
        texture.LoadImage(byteArray);
        GetComponent<RawImage>().texture = texture;
        Invoke("StartAnimation", 2f);
    }

    private void StartAnimation()
    {
        if (android)
        {
            GetComponent<Animator>().SetBool("fadeinandroid", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("fadein", true);
        }
    }

    private void OnDisable()
    {
        if (android)
        {
            GetComponent<Animator>().SetBool("fadeinandroid", false);
        }
        else
        {
            GetComponent<Animator>().SetBool("fadein", false);
        }
    }
}
