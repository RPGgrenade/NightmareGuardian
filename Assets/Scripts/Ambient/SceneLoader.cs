using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneLoader : MonoBehaviour
{
    public EventSystem Events;
    public AudioSource MenuMusic;
    public bool fading = false;
    private bool loading = false;

    private GameObject lastSelected;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Events = this.GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Events.currentSelectedGameObject == null)
            Events.SetSelectedGameObject(lastSelected);
        else
            lastSelected = Events.currentSelectedGameObject;


        if (fading)
            MenuMusic.volume -= (Time.deltaTime * FadeManager.Instance.FadeOutSpeed);
        else
            MenuMusic.volume += (Time.deltaTime * FadeManager.Instance.FadeInSpeed);


        if (FadeManager.Instance.Image.color.a >= 1 && fading)
        {
            if (!loading)
            {
                loading = true;
                SceneManager.LoadSceneAsync("House");
            }
        }
    }

    public void LoadScene()
    {
        if (!fading)
        {
            fading = true;
            this.GetComponent<AudioSource>().Play();
            FadeManager.Instance.ChangeFading();
        }
    }
}
