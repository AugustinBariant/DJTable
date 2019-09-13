using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametricCube : MonoBehaviour
{
    // public attributes
    public int cubeFrequencyBand;

    public float cubeStartScale;
    public float cubeScaleMultiplier;

    public bool useBuffer = true;

    // private attributes
    private Material _cubeMaterial;

    // public methods

    // private methods
    private void Start()
    {
        //this.cubeStartScale      =  2.0f;
        //this.cubeScaleMultiplier = 10.0f;
        this.transform.localScale = new Vector3(2, 2, 2);

        this._cubeMaterial = GetComponent<MeshRenderer>().materials[0];
    }

	private void Update ()
    {
        if (this.useBuffer)
        {
            this.transform.localScale = new Vector3(this.transform.localScale.x, (Sampling.bandBuffer[this.cubeFrequencyBand]) + this.cubeStartScale, this.transform.localScale.z);

            Color color = new Color(Sampling.audioBandBuffer[this.cubeFrequencyBand],
                                    Sampling.audioBandBuffer[this.cubeFrequencyBand],
                                    Sampling.audioBandBuffer[this.cubeFrequencyBand]);

            this._cubeMaterial.SetColor("_EmissionColor", color);
        }
        else    // use the frequency band
        {
            this.transform.localScale = new Vector3(this.transform.localScale.x, (Sampling.freqBands[this.cubeFrequencyBand])  + this.cubeStartScale, this.transform.localScale.z);

            Color color = new Color(Sampling.audioBand[this.cubeFrequencyBand],
                                    Sampling.audioBand[this.cubeFrequencyBand],
                                    Sampling.audioBand[this.cubeFrequencyBand]);

            this._cubeMaterial.SetColor("_EmissionColor", color);
        }
    }
}
