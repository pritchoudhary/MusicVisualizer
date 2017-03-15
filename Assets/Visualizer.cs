using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour {

    private const int SAMPLE_SIZE = 1024;   

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    public float backgroundIntensity;
    public Material backgroundMaterial;
    public Color minColor;
    public Color maxColor;

    public float maxVisualModifier = 25.0f;
    public float visualModifier = 50.0f;
    public float smoothSpeed = 10.0f;
    public float keepPercentage = 0.5f;

    private AudioSource source;
    private float[] samples = new float[SAMPLE_SIZE];
    private float[] spectrum = new float[SAMPLE_SIZE];
    private float sampleRate;

    private Transform[] visualList;
    private float[] visualScale;
    public int amnVisual = 256;
    // public GameObject ps;
    public GameObject[] particlesystemsgameobjects;// = new GameObject[64];
    private ParticleSystem[] particlesystems;// = new ParticleSystem[64];
    public GameObject flameObject;

    // Use this for initialization
    void Start () {
        particlesystems = new ParticleSystem[256];
        particlesystemsgameobjects = new GameObject[256];
        //for (int i = 0; i < particlesystemsgameobjects.Length; i++)
        //{
        //    particlesystems[i] = particlesystemsgameobjects[i].GetComponent<ParticleSystem>();
        //    particlesystemsgameobjects[i].transform.position = new Vector3(i, 0, 0);
        //}

        source = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        SpawnLine();
        //SpawnCircle();

    }
	
	// Update is called once per frame
	void Update () {

        AnalyzeSound();
        UpdateVisual();
        UpdateBackground();

	}

    void AnalyzeSound()
    {
        source.GetOutputData(samples, 0);

        //Get rms value
        float sum = 0;

        for(int index = 0; index < SAMPLE_SIZE; index++)
        {
            sum = samples[index] * samples[index];
        }

        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        //Get dbs value
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

        //Get sound spectrum
        source.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);

        //Find Pitch
        float maxVol = 0;
        var maxNoise = 0;

        for(int index = 0; index < SAMPLE_SIZE; index++)
        {
            if (!(spectrum[index] > maxVol) || !(spectrum[index] > 0.0f))
                continue;

            maxVol = spectrum[index];
            maxNoise = index;

        }

        float frequency = maxNoise;

        if(maxNoise > 0 && maxNoise < SAMPLE_SIZE - 1)
        {
            var dL = spectrum[maxNoise - 1] / spectrum[maxNoise];
            var dR = spectrum[maxNoise + 1] / spectrum[maxNoise];

            frequency += 0.5f * (dR * dR - dL * dL);
        }

        pitchValue = frequency * (sampleRate / 2) / SAMPLE_SIZE;
        //ParticleSystem p = ps.GetComponent<ParticleSystem>();
        //p.Emit((int)(spectrum[1]*1000.0f));

    }

    private void SpawnLine()
    {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual];

        for (int i = 0; i < amnVisual; i++)
        {
            particlesystemsgameobjects[i] = (GameObject)Instantiate(flameObject, transform.position + new Vector3(i,0,0), Quaternion.Euler(-90.0f,0,0));
            particlesystems[i] = particlesystemsgameobjects[i].GetComponent<ParticleSystem>();
        }
           

        //for (int i = 0; i < amnVisual; i++)
        //{

        //    //particlesystems[i].Emit((int)(spectrum[i] * 1000.0f));
        //    //particlesystems[i].Emit(100);
        //    GameObject go = GameObject.AddComponent(ParticleSystem);
        //    particlesystems[i].Emit((int)(spectrum[i] * 1000.0f));
        //    visualList[i] = particlesystems[i].transform;
        //    visualList[i].position = Vector3.right * i;
        //}
    }

    private void SpawnCircle()
    {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual];

        Vector3 centre = Vector3.zero;
        float radius = 10.0f;

        for (int i = 0; i < amnVisual; i++)
        {
            float angle = i*1.0f / amnVisual;
            angle *= Mathf.PI * 2;

            float x = centre.x + Mathf.Cos(angle) * radius;
            float y = centre.y + Mathf.Sin(angle) * radius;

            Vector3 pos = new Vector3(x,y,0);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(Vector3.forward, pos);
            visualList[i] = go.transform;
        }


    }

    private void UpdateVisual()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int avgSize = (int)(SAMPLE_SIZE * keepPercentage) / amnVisual;

        for (int i = 0; i < amnVisual; i++)
        {
            // particlesystems[i].Emit(100);
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);     
            
            particlesystems[i].Emit((int)(spectrum[i] * 1000.0f)+10);
            visualList[i] = particlesystems[i].transform;
            visualList[i].position = Vector3.right * i;
            ParticleSystem.MainModule mm = particlesystems[i].main;
            mm.startSpeed = spectrum[i] * 300.0f +5;
        }

        //while (visualIndex < amnVisual)
        //{
        //    int j = 0;
        //    float sum = 0;
        //    while (j < avgSize)
        //    {
        //        sum += spectrum[spectrumIndex];
        //        spectrumIndex++;
        //        j++;
        //    }

        //    float scaleY = sum / avgSize * visualModifier;
        //    visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;

        //    if (visualScale[visualIndex] < scaleY)
        //        visualScale[visualIndex] = scaleY;

        //    if (visualScale[visualIndex] > maxVisualModifier)
        //        visualScale[visualIndex] = maxVisualModifier;

        //    visualList[visualIndex].localScale = Vector3.one + Vector3.up * visualScale[visualIndex];
        //    visualIndex++;

        //}
    }

    private void UpdateBackground()
    {
        backgroundIntensity -= Time.deltaTime * 0.5f;

        if (backgroundIntensity < dbValue/40)
            backgroundIntensity = dbValue/40;

        backgroundMaterial.color = Color.Lerp(maxColor, minColor, -backgroundIntensity);
    }
}
