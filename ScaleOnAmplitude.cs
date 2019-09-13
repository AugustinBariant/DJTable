using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOnAmplitude : MonoBehaviour
{
    // public attributes
    public float startScale;
    public float maxScale;
    public float angle;
    public float speed;

    //public float red;         we can use this values to alter the color
    //public float green;
    //public float blue;

    public bool useBuffer;

    // private attributes
    private Material _material;

    // public methods

    // private methods
	private void Start ()
    {
        //this.startScale = 0.6f;
        //this.maxScale = 1.25f;
        this.useBuffer = true;
        this.speed = 2.0f;
        this.angle = 0.0f;

        this._material = GetComponent<MeshRenderer>().materials[0];
        this.transform.localScale = new Vector3(1, 1, 1);
    }

    private void Update ()
    {
        angle += Time.deltaTime * speed;
        this.transform.localEulerAngles = new Vector3(0.0f, angle, 0.0f);

        if (this.useBuffer)
        {
            this.transform.localScale = new Vector3(Sampling.amplitudeBuffer * this.maxScale + this.startScale,
                                                    Sampling.amplitudeBuffer * this.maxScale + this.startScale,
                                                    Sampling.amplitudeBuffer * this.maxScale + this.startScale);

            Color color = new Color(Sampling.amplitudeBuffer,
                                    Sampling.amplitudeBuffer,
                                    Sampling.amplitudeBuffer);

            this._material.SetColor("_EmissionColor", color);
        }
        else
        {
            this.transform.localScale = new Vector3(Sampling.amplitude * this.maxScale + this.startScale,
                                                    Sampling.amplitude * this.maxScale + this.startScale,
                                                    Sampling.amplitude * this.maxScale + this.startScale);

            Color color = new Color(Sampling.amplitude,
                                    Sampling.amplitude,
                                    Sampling.amplitude);

            this._material.SetColor("_EmissionColor", color);
        }
	}
}
