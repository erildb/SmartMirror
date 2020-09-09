using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] songs;
    [SerializeField]
    private Image fillbar;
    [SerializeField]
    private Text title;
    [SerializeField]
    private Image play;
    [SerializeField]
    private Image pause;

    private AudioSource audioSource;

    private void Update()
    {
        try
        {
            fillbar.fillAmount = audioSource.time / audioSource.clip.length;
        }
        catch{}
    }

    public void playMusic()
    {
        audioSource = FindObjectOfType<AudioSource>();
        audioSource.loop = true;
        if (!audioSource.isPlaying)
        {
            gameObject.SetActive(true);
            play.gameObject.SetActive(false);
            pause.gameObject.SetActive(true);
            audioSource.clip = songs[0];
            audioSource.Play();
            title.text = "Relaxing Music"; 
        }
    }

    public void pauseMusic()
    {
        try
        {
            audioSource.Pause();
            gameObject.SetActive(false);
        }
        catch{}
    }
}
