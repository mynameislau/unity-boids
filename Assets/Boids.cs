using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BoidsNS;
using System.Linq;
using Functional;
using MathUtils;
public class Boids : MonoBehaviour {
	private static List<Boid> boids = new List<Boid>();
	public AnimationClip clip;
	private const int flockRadius = 100;
	public float boidSize = 1f;
	public float personalSpaceRadius = 2f;
	private const int nbBoids = 20;
	private const int spawnRadius = 20;
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 1f;
	public float separationWeight = 1f;
	public float influencesRecalcFrequency = 1f;
	public float boidDrag = 1f;
	private Boid boid;

	void Start () {
		//boids = F.Times<Boid>(nbBoids, createBoid);
		boid = createBoid();
		boids.Add(boid);
		StartCoroutine(ComputeInfluences(boid));
	}

	IEnumerator ComputeInfluences (Boid boid) {
		// foreach (var boid in boids)
		// {
		boid.navigator.RegisterInfluence(new Influence("flock", Influences(boid)));
		// }

		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(influencesRecalcFrequency);
		StartCoroutine(ComputeInfluences(boid));
	}

	Boid createBoid (int n = 0) {
		//GameObject gameObject = UnityEngine.Object.Instantiate(boid);
		gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
		gameObject.transform.parent = gameObject.transform;
		Rigidbody rb = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
		Navigator navigator = gameObject.GetComponent(typeof(Navigator)) as Navigator;
		rb.useGravity = false;
		rb.drag = boidDrag;
		// gameObject.transform.position = new Vector3(
		// 	UnityEngine.Random.Range(-spawnRadius, spawnRadius),
		// 	UnityEngine.Random.Range(-spawnRadius, spawnRadius),
		// 	UnityEngine.Random.Range(-spawnRadius, spawnRadius)
		// );

		return new Boid(gameObject, rb, navigator);
	}

	void FixedUpdate () {
		updateBoid(boid);
	}

	Vector3 GetTorque (Vector3 eulerAngle) {
		Vector3 bla = eulerAngle / 360;
		return new Vector3(
			bla.x > 0.5 ? -(1 - bla.x) : bla.x,
			bla.y > 0.5 ? -(1 - bla.y) : bla.y,
			bla.z > 0.5 ? -(1 - bla.z) : bla.z
		);
	}
	InfluenceData Influences (Boid boid) {
		// Rigidbody rb = boid.body;

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
		// Quaternion rot = Quaternion.FromToRotation(Vector3.forward, localSum);
		// Vector3 torque = GetTorque(rot.eulerAngles) * 0.1f;
		// torque.z = 0;

		return new InfluenceData(sum);
	}

	void updateBoid (Boid boid) {
		// Rigidbody rb = boid.body;
		// Vector3 torque;
		// float magnitude;

		if (boid.influenceData.HasValue) {
			// torque = boid.influencesData.Value.torque;
			// magnitude = boid.influencesData.Value.magnitude;

			// rb.AddRelativeTorque(torque);
			// rb.AddRelativeForce(Vector3.forward * 0.2f);
			// drawAgentVector(boid.gameObject, boid.influenceData.Value.vector, () => Color.green);
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
		IEnumerable<Boid> neighs = neighbours(
			radius,
			everyBoidsBut(boids, boid),
			boid
		);

		return V.Average(boidPositions(neighs));
	}

	Vector3 Alignment (Boid boid) {
		IEnumerable<Boid> neighs = neighbours(
			flockRadius,
			boids,
			boid
		);

		return V.Average(boidVelocities(neighs)).Value * alignmentWeight;
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

	IEnumerable<Boid> everyBoidsBut (IEnumerable<Boid> boidList, Boid boid) {
		return F.Filter(curr => curr != boid, boidList);
	}

	Vector3 boidPosition (Boid boid) { return boid.body.position; }

	IEnumerable<Vector3> boidPositions (IEnumerable<Boid> boidList) { return F.Map(boidPosition, boidList); }

	Vector3 boidVelocity (Boid boid) { return boid.body.velocity; }

	IEnumerable<Vector3> boidVelocities (IEnumerable<Boid> boidList) { return F.Map(boidPosition, boidList); }

	IEnumerable<Boid> neighbours (float radius, IEnumerable<Boid> otherBoids, Boid boid) {
		return F.Filter(
			currentBoid => Vector3.Distance(boid.body.position, currentBoid.body.position) < radius,
			otherBoids
		);
	}
}
