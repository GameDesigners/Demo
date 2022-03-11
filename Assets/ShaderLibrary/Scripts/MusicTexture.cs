using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicTexture : MonoBehaviour
{
    [Header("Shader Paramters")]
    public Shader MusicEffectShader;
    public Color  EffectColor = Color.white;
    public int    Divides = 64;
    [Range(0, 1)] public float  Antialias = 0.1f;
    [Range(0, 1)] public float  Radius = 0.2f;
    [Range(0, 1)] public float  RayOffset = 0.1f;
    [Range(0, 1)] public float  Brightness = 1f;
    [Space(20)]


    public float Multiplayer = 1.0f;
    [Range(0.0005f, 0.5f)] public float mDelay;
    [HideInInspector, System.NonSerialized] public float[] spatrumDataDelay;
    public Texture2D mDataTexture;
    public FilterMode filterMode;

    private AudioSource audioSource;
    private Material material;
    private int numSamples = 512;

    private void Start()
    {
        audioSource=GetComponent<AudioSource>();
        material = new Material(MusicEffectShader);
        ApplyMaterialParamters();
        GetComponent<MeshRenderer>().material = material;

        mDataTexture = new Texture2D(numSamples, 1, TextureFormat.RGBA32, false);
        mDataTexture.filterMode = filterMode;
        material.SetTexture("_MusicData",mDataTexture);
        spatrumDataDelay=new float[numSamples];
    }

    private void Update()
    {
        float[] spectrum=new float[numSamples];
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        int i = 1;
        while (i < numSamples + 1)
        {
            float newData = spectrum[i - 1] * 1.0f * Multiplayer;

            if (newData > spectrum[i - 1])
            {
                spatrumDataDelay[i - 1] += mDelay * Time.deltaTime;
                if (spatrumDataDelay[i - 1] > newData)
                    spatrumDataDelay[i - 1] = newData;
            }
            else
            {
                spatrumDataDelay[i - 1] -= (mDelay * Time.deltaTime);
                if (spatrumDataDelay[i - 1] < 0f)
                    spatrumDataDelay[i - 1] = 0f;
            }

            //设置颜色
            mDataTexture.SetPixel(i - 1, 1, new Color(spatrumDataDelay[i - 1] * 255.0f, 0, 0, 0));
            i++;
        }
        mDataTexture.Apply();
    }

    public void ApplyMaterialParamters()
    {
        material.SetColor("_Color",EffectColor);
        material.SetInt("_Divides",Divides);
        material.SetFloat("_Antialias", Antialias);
        material.SetFloat("_Radius", Radius);
        material.SetFloat("_RayOffset", RayOffset);
        material.SetFloat("_Brightness", Brightness);
    }

}
