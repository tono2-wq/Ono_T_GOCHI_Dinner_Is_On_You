using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public AudioSource GOCHIMainMenuMusic;

    public void PlayGame()
    {
        StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        float startVolume = GOCHIMainMenuMusic.volume;

        while (GOCHIMainMenuMusic.volume > 0)
        {
            GOCHIMainMenuMusic.volume -= startVolume * Time.deltaTime;
            yield return null;
        }

        GOCHIMainMenuMusic.Stop();
        SceneManager.LoadScene("GameScene");
    }
}