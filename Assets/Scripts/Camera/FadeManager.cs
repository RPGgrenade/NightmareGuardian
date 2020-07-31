using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    public RawImage Image;
    public float InitialPause = 0.4f;
    public float FadeOutSpeed = 2f;
    public float FadeInSpeed = 2f;
    public bool IsFaded = false;

    private Color color;
    private float pause;
    // Start is called before the first frame update
    void Start()
    {
        if (Image == null)
            Image = this.GetComponent<RawImage>();
        Instance = this;
        color = Image.color;
        IsFaded = true;
        pause = InitialPause;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (pause <= 0f)
        {
            if (IsFaded)
                FadeOut();
            else
                FadeIn();
        }
        else
        {
            pause -= Time.deltaTime;
        }
    }

    private void FadeOut()
    {
        float alpha = color.a;
        if(alpha > 0)
            alpha -= Time.deltaTime * FadeOutSpeed;
        color.a = alpha;
        Image.color = color;
    }

    private void FadeIn()
    {
        float alpha = color.a;
        if(alpha < 1)
            alpha += Time.deltaTime * FadeInSpeed;
        color.a = alpha;
        Image.color = color;
    }

    public void ChangeFading()
    {
        IsFaded = !IsFaded;
    }
}
