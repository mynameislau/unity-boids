using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BoidsNS;
using System.Linq;
using Functional;
public class Boids : MonoBehaviour {
	public GameObject boid;
	private Boid[] boids;
	public AnimationClip clip;
	private const int flockRadius = 100;
	public float boidSize = 1f;
	public float personalSpaceRadius = 2f;
	private const int nbBoids = 20;
	private const int spawnRadius = 20;
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 1f;
	public float separationWeight = 1f;
	public float boidDrag = 1f;

	void Start () {
		boids = F.Times<Boid>(nbBoids, createBoid);
		StartCoroutine(ComputeInfluences());
	}

	IEnumerator ComputeInfluences () {
		foreach (var boid in boids)
		{
			boid.navigator.RegisterInfluence("flock", Influences(boid));
			yield return null;
		}

		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.2f);
		StartCoroutine(ComputeInfluences());
	}

	Boid createBoid (int n = 0) {
		GameObject boidGO = UnityEngine.Object.Instantiate(boid);
		boidGO.transform.rotation = Quaternion.Euler(0, 0, 0);
		boidGO.transform.parent = gameObject.transform;
		Rigidbody rb = boidGO.GetComponent(typeof(Rigidbody)) as Rigidbody;
		Navigator navigator = boidGO.GetComponent(typeof(Navigator)) as Navigator;
		rb.useGravity = false;
		rb.drag = boidDrag;
		boidGO.transform.position = new Vector3(
			UnityEngine.Random.Range(-spawnRadius, spawnRadius),
			UnityEngine.Random.Range(-spawnRadius, spawnRadius),
			UnityEngine.Random.Range(-spawnRadius, spawnRadius)
		);

		return new Boid(boidGO, rb, navigator);
	}

	void FixedUpdate () {
		if (boids.Length > 0) {
			F.ForEach(updateBoid, boids);
		}
	}

	Vector3 GetTorque (Vector3 eulerAngle) {
		Vector3 bla = eulerAngle / 360;
		return new Vector3(
			bla.x > 0.5 ? -(1 - bla.x) : bla.x,
			bla.y > 0.5 ? -(1 - bla.y) : bla.y,
			bla.z > 0.5 ? -(1 - bla.z) : bla.z
		);
	}
	InfluencesData Influences (Boid boid) {
		Rigidbody rb = boid.body;

		Vector3 cohesion = Cohesion(boid);
		Vector3 separation = Separation(boid);
		Vector3 alignment = Alignment(boid);
		Vector3 mainZone = MainZone(boid);

		Vector3[] influences = { cohesion, separation, alignment, mainZone };
		Vector3 sum = F.Reduce(Vector3.zero, (acc, vec) => acc + vec, influences);

		Vector3 localSum = boid.gameObject.transform.InverseTransformDirection(sum);
		//rb.angularDrag = 1;
		//rb.AddRelativeTorque(Vector3.forward - new Vector3(-localSum.y, -localSum.x, 0));
		// rb.AddRelativeTorque(new Vector3(-localSum.y, -localSum.x, 0));
		//boid.gameObject.transform.rotation = Quaternion.LookRotation(localSum);
		Quaternion rot = Quaternion.FromToRotation(Vector3.forward, localSum);
		Vector3 torque = GetTorque(rot.eulerAngles) * 0.1f;
		torque.z = 0;

		InfluencesData influencesData;
		influencesData.torque = torque;
		influencesData.magnitude = sum.magnitude;
		influencesData.vector = localSum;
		influencesData.name = "flock";
		return influencesData;
	}

	void updateBoid (Boid boid) {
		Rigidbody rb = boid.body;
		Vector3 torque;
		float magnitude;

		if (boid.influencesData.HasValue) {
			torque = boid.influencesData.Value.torque;
			magnitude = boid.influencesData.Value.magnitude;

			// rb.AddRelativeTorque(torque);
			// rb.AddRelativeForce(Vector3.forward * 0.2f);
			// drawAgentVector(boid.gameObject, boid.influencesData.Value.vector, () => Color.green);
		}
		// drawAgentVector(boid.gameObject, localSum , () => Color.red);

		// rb.AddForce(LimitedSteer(rb.velocity, Cohesion(boid)));
		// rb.AddForce(LimitedSteer(rb.velocity, Separation(boid)));
		// rb.AddForce(LimitedSteer(rb.velocity, Alignment(boid)));
		// rb.AddForce(LimitedSteer(rb.velocity, MainZone(boid)));

	//	boid.gameObject.transform.rotation = Quaternion.LookRotation(rb.velocity);
	}
	void drawAgentVector (GameObject gameObj, Vector3 vec, Func<Color> colorFn) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(vec);
		Debug.DrawLine(pos, pos + dir, colorFn());
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
