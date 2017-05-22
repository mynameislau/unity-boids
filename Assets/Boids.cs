using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoidsNS;

public class Boids : MonoBehaviour {

	private GameObject boid;
	public Mesh mesh;
	public Material material;

	private Boid[] boids;


	// Use this for initialization
	void Start () {
		for (int i = 0; i < length; i++)
		{
			boids.Add(createBoid());
		}
	}

	Boid createBoid () {
		GameObject boidGO = new GameObject("BoidGO");
		boidGO.transform.parent = gameObject.transform;
		Rigidbody rb = boidGO.AddComponent<Rigidbody>();
		rb.useGravity = false;
		print(rb);
		boidGO.AddComponent<MeshFilter>().mesh = mesh;
		boidGO.AddComponent<MeshRenderer>().material = material;
		
		return new Boid(boidGO, rb);
	}
	
	// Update is called once per frame
	void Update () {
		Rigidbody rb = boid.GetComponent(typeof(Rigidbody)) as Rigidbody;
		print(boid.transform.position);
		rb.AddForce(Vector3.right * 1f);
		rb.AddForce(Vector3.up * 1f);
	}
}
