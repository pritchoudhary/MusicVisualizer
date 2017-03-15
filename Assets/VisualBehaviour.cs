using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualBehaviour : MonoBehaviour {

    public GameObject prefab;
    public int noOfObjects = 10;
    public float radius = 5;
    public GameObject[] cubes;
    public Material prefabColor;

	// Use this for initialization
	void Start () {

        for (int index = 0; index <= noOfObjects; index++)
        {
            float angle = index * Mathf.PI * 2 / noOfObjects;
            Vector3 posiiton = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Instantiate(prefab, posiiton, Quaternion.identity);
        }

        cubes = GameObject.FindGameObjectsWithTag("Cubes");
	}
	
	// Update is called once per frame
	void Update () {

        float[] spectrum = new float[1024];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int index = 0; index <= noOfObjects; index++)
        {
            Vector3 previousScale = cubes[index].transform.localScale;
            previousScale.y = Mathf.Lerp(previousScale.y, spectrum[index] * 40, Time.deltaTime * 30);
            cubes[index].transform.localScale = previousScale;
        }
        
    }
}
