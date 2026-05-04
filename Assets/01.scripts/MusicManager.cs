using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    private string sceneName;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        sceneName = string.Empty;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("MusicManager::OnSceneLoaded");

        CancelInvoke("PlayMusic");

        string newSceneName = scene.name;
        if (newSceneName != sceneName) {
            sceneName = newSceneName;
            Invoke("PlayMusic", 0.2f);
        }
        
    }
    
    

    void PlayMusic()
    {
        AudioClip clipToplay = null;
        
        if (sceneName == "Menu") {
            clipToplay = menuTheme;
        } else if (sceneName == "Game") {
            clipToplay = mainTheme;
        }
        
        if (AudioManager.instance == null) {
            Debug.LogError("AudioManager.instance is null");
            return;
        }
        
        if (clipToplay != null) {
            AudioManager.instance.PlayMusic (clipToplay, 2);
            Invoke("PlayMusic", clipToplay.length);
        }


    }
}