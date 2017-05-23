using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BoidsNS;
using System.Linq;
using Functional;
public class Boids : MonoBehaviour {
	public Mesh mesh;
	public Material material;

	private Boid[] boids;

	private const int flockRadius = 100;
	public float boidSize = 1f;
	public float personalSpaceRadius = 2f;
	private const int nbBoids = 50;
	private const int spawnRadius = 20;
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 1f;
	public float separationWeight = 1f;
	public float boidDrag = 1f;


	// Use this for initialization
	void Start () {
		boids = Times<Boid>(nbBoids, createBoid);
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
		rb.drag = boidDrag;
		boidGO.transform.position = new Vector3(
			UnityEngine.Random.Range(-spawnRadius, spawnRadius),
			UnityEngine.Random.Range(-spawnRadius, spawnRadius),
			UnityEngine.Random.Range(-spawnRadius, spawnRadius)
		);

		boidGO.AddComponent<MeshFilter>().mesh = mesh;
		boidGO.AddComponent<MeshRenderer>().material = material;

		return new Boid(boidGO, rb);
	}

	void Update () {
		if (boids.Length > 0) {
			F.ForEach(updateBoid, boids);
		}
	}

	void updateBoid (Boid boid) {
		Rigidbody rb = boid.body;
		rb.AddForce(LimitedSteer(rb.velocity, Cohesion(boid)));
		rb.AddForce(LimitedSteer(rb.velocity, Separation(boid)));
		rb.AddForce(LimitedSteer(rb.velocity, Alignment(boid)));
		rb.AddForce(LimitedSteer(rb.velocity, MainZone(boid)));

		boid.gameObject.transform.rotation = Quaternion.LookRotation(rb.velocity);
	}

	Vector3 LimitedSteer (Vector3 source, Vector3 target) {
		float magnitude = target.magnitude;
		Vector3 rotated = Vector3.RotateTowards(NormalizeVec(source), NormalizeVec(target), (float) Math.PI * 0.2f, 1f);
		return rotated * magnitude;
	}

	Vector3 NormalizeVec (Vector3 vec) {
		Vector3 copy = vec;
		copy.Normalize();
		return copy;
	}

	Vector3? AverageNeighbours (float radius, Boid boid) {
		Boid[] neighs = neighbours(
			radius,
			everyBoidsBut(boids, boid),
			boid
		);

		return averageVectors(boidPositions(neighs));
	}

	Vector3 Alignment (Boid boid) {
		Boid[] neighs = neighbours(
			flockRadius,
			boids,
			boid
		);

		return averageVectors(boidVelocities(neighs)).Value * alignmentWeight;
	}

	Vector3 MainZone (Boid boid) {
		Vector3 distanceFromCenter = Vector3.zero - boid.body.position; //could have been just magnitude of boid pos
		return distanceFromCenter * 0.2f;
	}

	Vector3 Cohesion (Boid boid) {
		Vector3? av = AverageNeighbours(flockRadius, boid);
		if (av.HasValue) {
			float distance = Vector3.Distance(boid.body.position, av.Value);
			float ratio = distance / flockRadius;

			return (av.Value - boid.body.position) * ratio * cohesionWeight;
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

			return (boid.body.position - av.Value) * ratio * separationWeight;
		}
		else {
			return Vector3.zero;
		}
	}

	Boid[] everyBoidsBut (Boid[] boidList, Boid boid) {
		return F.Filter(curr => curr != boid, boidList);
	}

	Vector3? averageVectors (Vector3[] vectors) {
		if (vectors.Length == 0) { return null; }
		Vector3 sum = F.Reduce(new Vector3(0,0,0), (acc, curr) => acc + curr, vectors);
		return sum / vectors.Length;
	}

	Vector3 boidPosition (Boid boid) { return boid.body.position; }

	Vector3[] boidPositions (Boid[] boidList) { return F.Map(boidPosition, boidList); }

	Vector3 boidVelocity (Boid boid) { return boid.body.velocity; }

	Vector3[] boidVelocities (Boid[] boidList) { return F.Map(boidPosition, boidList); }

	Boid[] neighbours (float radius, Boid[] otherBoids, Boid boid) {
		return F.Filter(
			currentBoid => Vector3.Distance(boid.body.position, currentBoid.body.position) < radius,
			otherBoids
		);
	}
}
