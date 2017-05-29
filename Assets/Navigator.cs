using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using BoidsNS;
using Functional;

public class Navigator : MonoBehaviour {
	private Rigidbody rb;
	private GameObject agentObj;

	private List<NamedInfluence> influences;
	// Use this for initialization
	void Start () {
		rb = agentObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
	}

	public void registerInfluence (string name, NamedInfluence influence) {
		F.Filter(curr => curr.name == influence.name, influences.ToArray())
		.ToList()
		.Add(new NamedInfluence(name, influence));
	}

	Vector3 GetTorque (Vector3 eulerAngle) {
		Vector3 bla = eulerAngle / 360;
		return new Vector3(
			bla.x > 0.5 ? -(1 - bla.x) : bla.x,
			bla.y > 0.5 ? -(1 - bla.y) : bla.y,
			bla.z > 0.5 ? -(1 - bla.z) : bla.z
		);
	}
	// Update is called once per frame
	void FixedUpdate () {
		InfluencesData[] filtered = F.FilterOutNulls(influences.ToArray());
		Vector3[] vectors = F.Map(influence => influence.vector, filtered);
		Vector3? maybeVectorsAverage = AverageVectors(vectors);

		if (maybeVectorsAverage.HasValue) {
			Vector3 vectorsAverage = maybeVectorsAverage.Value;
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, vectorsAverage);
			Vector3 torque = GetTorque(rot.eulerAngles) * 0.1f;
			torque.z = 0;
			
			rb.AddRelativeTorque(torque);
			rb.AddRelativeForce(Vector3.forward * 0.2f * vectorsAverage.magnitude);

			drawAgentVector(gameObject, vectorsAverage, () => Color.green);
		}
	}
	void drawAgentVector (GameObject gameObj, Vector3 vec, Func<Color> colorFn) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(vec);
		Debug.DrawLine(pos, pos + dir, colorFn());
	}


	Vector3? AverageVectors (Vector3[] vectors) {
		if (vectors.Length == 0) { return null; }
		Vector3 sum = F.Reduce(new Vector3(0,0,0), (acc, curr) => acc + curr, vectors);
		return sum / vectors.Length;
	}
}
