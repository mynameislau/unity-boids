using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BoidsNS;
using System.Linq;

public class Boids : MonoBehaviour {
	public Mesh mesh;
	public Material material;

	private Boid[] boids;

	private const int flockRadius = 100;


	// Use this for initialization
	void Start () {
		boids = Times<Boid>(4, createBoid);
		print(boids.Length);
	}

	T[] Times<T> (int times, Func<int, T> fn) {
		T[] array = new T[times];

		for (int i = 0; i < times; i++)
		{
			array[i] = fn(i);
		}

		return array;
	}

	Boid createBoid (int n = 0) {
		GameObject boidGO = new GameObject("BoidGO");
		boidGO.transform.parent = gameObject.transform;
		Rigidbody rb = boidGO.AddComponent<Rigidbody>();
		rb.useGravity = false;
		print(rb);
		boidGO.transform.position = new Vector3(
			UnityEngine.Random.Range(0, 10),
			UnityEngine.Random.Range(0, 10),
			UnityEngine.Random.Range(0, 10)
		);
		boidGO.AddComponent<MeshFilter>().mesh = mesh;
		boidGO.AddComponent<MeshRenderer>().material = material;
		
		return new Boid(boidGO, rb);
	}
	
	// Update is called once per frame
	void Update () {
		Array.ForEach(boids, updateBoid);
	}

	void updateBoid (Boid boid) {
		Rigidbody rb = boid.body;
		rb.AddForce(Vector3.right * 1f);
		rb.AddForce(Vector3.up * 1f);
	}

	Vector3 Cohesion (Boid boid) {
		Vector3 boidPos = boidPosition(boid);
		Vector3[] neighs = neighbours(
			flockRadius,
			boidPositions(otherBoids(boids, boid)),
			boidPos
		);

		Vector3 av = averageVectors(neighs);
		Vector3.Distance(boidPos, av);
	}

	Boid[] otherBoids (Boid[] boidList, Boid boid) {
		return boidList.Where(curr => curr != boid).ToArray();
	}

	Vector3 averageVectors (Vector3[] vectors) {
		Vector3 sum = vectors.Aggregate(new Vector3(0,0,0), (acc, curr) => acc + curr);
		return sum / vectors.Length;
	}

	Vector3 boidPosition (Boid boid) {
		return boid.body.position;
	}

	Vector3[] boidPositions (Boid[] boidList) {
		return boidList.Select(boidPosition).ToArray();
	}

	Vector3[] neighbours (float radius, Vector3[] positionList, Vector3 position) {
		return positionList.Where(pos => Vector3.Distance(position, pos) < radius).ToArray();
	}
}
