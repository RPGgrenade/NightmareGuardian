using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TV : MonoBehaviour
{
    [Header("Action Properties")]
    public bool On = false;
    [Header("Material Properties")]
    public Material Screen;
    public Color OffColor = Color.black;
    public Color OnColor = Color.white;
    public float StaticSpeed = 2f;
    public float RandomizeSpeed = 0.2f;

    private Vector2 staticDirection;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        setTVSettings();
    }

    private void setTVSettings()
    {
        if (On)
        {
            if(!Screen.IsKeywordEnabled("_EMISSION"))
                Screen.EnableKeyword("_EMISSION");
            if (Screen.color == OffColor)
                Screen.color = OnColor;
            moveScreen();
        }
        else
        {
            if (Screen.IsKeywordEnabled("_EMISSION"))
                Screen.DisableKeyword("_EMISSION");
            if (Screen.color == OnColor)
                Screen.color = OffColor;
        }
    }

    private void moveScreen()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            staticDirection = new Vector2(Random.Range(-1f,1f), Random.Range(-1f,1f)).normalized;
            timer = RandomizeSpeed;
        }
        Vector2 currentSpeed = staticDirection * Time.deltaTime * StaticSpeed;
        Vector2 currentOffset = Screen.GetTextureOffset("_MainTex");
        Screen.SetTextureOffset("_MainTex", currentOffset + currentSpeed);
    }
}
