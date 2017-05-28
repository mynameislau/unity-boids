using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelocityClamper : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Rigidbody rb = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
		rb.angularVelocity = Vector3.Max(rb.angularVelocity, new Vector3(
			0.1f,
			0.1f,
			0.1f
		));
	}
}
