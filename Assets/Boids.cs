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
	private const int personalSpaceRadius = 2;
	private const int nbBoids = 50;
	private const int spawnRadius = 20;


	// Use this for initialization
	void Start () {
		boids = Times<Boid>(nbBoids, createBoid);
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
			UnityEngine.Random.Range(0, spawnRadius),
			UnityEngine.Random.Range(0, spawnRadius),
			UnityEngine.Random.Range(0, spawnRadius)
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
		rb.AddForce(Cohesion(boid) * 1f);
		rb.AddForce(Separation(boid) * 1f);
	}

	Vector3? AverageNeighbours (int radius, Boid boid) {
		Vector3 boidPos = boidPosition(boid);
		Boid[] neighs = neighbours(
			radius,
			everyBoidsBut(boids, boid),
			boid
		);

		return averageVectors(boidPositions(neighs));
	}

	Vector3 Alignment (Boid boid) {
		Vector3 boidPos = boidPosition(boid);
		Boid[] neighs = neighbours(
			flockRadius,
			boids,
			boid
		);

		return averageVectors(boidVelocities(neighs)).Value;
	}

	Vector3 Cohesion (Boid boid) {
		Vector3? av = AverageNeighbours(flockRadius, boid);
		if (av.HasValue) {
			float distance = Vector3.Distance(boid.body.position, av.Value);
			float ratio = distance / flockRadius;

			return (av.Value - boid.body.position) * ratio;
		}
		else {
			return Vector3.zero;
		}
	}

	Vector3 Separation (Boid boid) {
		Vector3? av = AverageNeighbours(personalSpaceRadius, boid);
		if (av.HasValue) {
			float distance = Vector3.Distance(boid.body.position, av.Value);
			float ratio = distance / personalSpaceRadius;

			return (boid.body.position - av.Value) * ratio;
		}
		else {
			return Vector3.zero;
		}
	}

	Boid[] everyBoidsBut (Boid[] boidList, Boid boid) {
		return boidList.Where(curr => curr != boid).ToArray();
	}

	Vector3? averageVectors (Vector3[] vectors) {
		if (vectors.Length == 0) { return null; }
		Vector3 sum = Reduce(new Vector3(0,0,0), (acc, curr) => acc + curr, vectors);
		return sum / vectors.Length;
	}

	T1[] Map<T, T1> (Func<T, T1> fn, T[] array) => array.Select(fn).ToArray();
	T[] Filter<T> (Func<T, bool> fn, T[] array) => array.Where(fn).ToArray();

	B Reduce <T, B> (B baseAcc, Func<B, T, B> fn, T[] array) => array.Aggregate(baseAcc, fn);

	Vector3 boidPosition (Boid boid) => boid.body.position;

	Vector3[] boidPositions (Boid[] boidList) => Map(boidPosition, boidList);

	Vector3 boidVelocity (Boid boid) => boid.body.velocity;

	Vector3[] boidVelocities (Boid[] boidList) => Map(boidPosition, boidList);

	Boid[] neighbours (float radius, Boid[] otherBoids, Boid boid) =>
		Filter(
			currentBoid => Vector3.Distance(boid.body.position, currentBoid.body.position) < radius,
			otherBoids
		);
}
