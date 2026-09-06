using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager AMInstance;

    public AudioSource AS;

    public AudioClip Menuclip;

    public AudioClip Gameclip1;
    public AudioClip Gameclip2;


    public int currentMusic;


    private void Awake()
    {
        if(AMInstance!=null)
        {
            Destroy(gameObject);
            return;
        }

        AMInstance = this;

        Menuclip.LoadAudioData();
        Gameclip1.LoadAudioData();
        Gameclip2.LoadAudioData();

        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
        return;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlaySceneMusic();
    }

    // Update is called once per frame
    void Update()
    {
        if(!AS.isPlaying)
        {
            PlayNextMusic();
        }
    }
    public void OnSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
    {
        PlaySceneMusic();
    }

    public void PlaySceneMusic()
    {
        string scenename = SceneManager.GetActiveScene().name;
        if (scenename == SceneName.MainMenu)
        {
            PlayMenuMusic();
        }
        else
        {
            PlayGameMusic();
        }
    }

    public void PlayMenuMusic()
    {
        if(AS.clip == Menuclip)
        {
            return;
        }
        AS.clip = Menuclip;
        AS.loop = true;
        AS.Play();
        return;
    }

    public void PlayGameMusic()
    {
        if(AS.clip != Menuclip && AS.isPlaying)
        {
            return;
        }
        currentMusic = 1;
        AS.clip = Gameclip1;
        AS.loop = false;
        AS.Play();
        return;
    }
    public void PlayNextMusic()
    {
        if(currentMusic == 1)
        {
            currentMusic = 2;
            AS.clip = Gameclip2;
            AS.loop = false;
            AS.Play();
            return;
        }
        else
        {
            currentMusic = 1;
            AS.clip = Gameclip1;
            AS.loop = false;
            AS.Play();
            return;
        }
    }
}
