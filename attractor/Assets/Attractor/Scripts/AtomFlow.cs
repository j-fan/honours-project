using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomFlow : MonoBehaviour {
    Rigidbody rigidbody;
    public GameObject attractor;
    public float forceModifier = 100f;
    public float maxMagnitude = 100f;
    public BeatsFFT beatsFFT;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if(attractor != null)
        {
            Vector3 direction = attractor.transform.position - transform.position;
            rigidbody.AddForce(direction * forceModifier );
            rigidbody.velocity = rigidbody.velocity * beatsFFT.avgFreq * 1000;
            if (rigidbody.velocity.magnitude > maxMagnitude)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * maxMagnitude;
            }
            float hue = 1 / Vector3.Distance(attractor.transform.position, transform.position);
            GetComponent<Renderer>().material.color = Color.HSVToRGB(hue, 1.0f, 0.7f);
        }

	}
}
