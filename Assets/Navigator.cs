using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using BoidsNS;
using Functional;
using VacuumBreather;
using MathUtils;

public class Navigator : MonoBehaviour {
	private Rigidbody rb;
	private float minVelocityMagnitude = 0.5f;
	private float maxVelocityMagnitude = 3f;
	private PidQuaternionController anglePIDController = new PidQuaternionController(8.0f, 0.0f, 0.05f);
	private List<Influence> influences = new List<Influence>();
	// Use this for initialization
	void Start () {
		rb = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
	}

	public void RegisterInfluence (Influence influence) {
		influences = F.Filter(curr => curr.name != influence.name, influences);
		influences.Add(influence);
	}

	Vector3 GetOrientation (Vector3 eulerAngle) {
		Vector3 bla = eulerAngle / 360;
		return new Vector3(
			bla.x > 0.5 ? -(1 - bla.x) : bla.x,
			bla.y > 0.5 ? -(1 - bla.y) : bla.y,
			bla.z > 0.5 ? -(1 - bla.z) : bla.z
		);
	}
	// Update is called once per frame
	void FixedUpdate () {
		IEnumerable<Influence> filtered = F.Filter(influence => influence.influenceData.HasValue, influences);
		int maxPrio = F.Reduce(
			0,
			(acc, val) => {
				return Mathf.Max(acc, val.priority);
			},
			filtered
		);
		IEnumerable<Influence> prioritized = F.Filter(influence => influence.priority >= maxPrio, filtered);
		IEnumerable<InfluenceData?> datas = F.Map(influence => influence.influenceData, prioritized);
		IEnumerable<InfluenceData> filteredDatas = F.FilterOutNulls(datas);
		IEnumerable<Vector3> vectors = F.Map(influence => influence.vector, filteredDatas);
		IEnumerable<float> steering = F.Map(influence => influence.steeringModifer, filteredDatas);

		Vector3? maybeVectorsAverage = V.Average(vectors);
		float? maybeSteeringAverage = V.Average(steering);

		if (F.Length(filteredDatas) > 0) {
			Vector3 vectorsAverage = maybeVectorsAverage.Value;
			float steeringAverage = maybeSteeringAverage.Value;
			Quaternion rot = Quaternion.LookRotation(vectorsAverage);
			// Vector3 torque = GetTorque(rot.eulerAngles) * 0.1f;
			// torque.z = 0;
			
			// rb.AddRelativeTorque(torque);
			// rb.AddRelativeForce(Vector3.forward * 0.2f * vectorsAverage.magnitude);

			// Vector3 requiredAngularAcceleration = anglePIDController
			// 	.ComputeRequiredAngularAcceleration(
			// 		gameObject.transform.rotation,
			// 		rot,
			// 		rb.angularVelocity,
			// 		Time.fixedDeltaTime
			// 	);
			// requiredAngularAcceleration.z = 0;
			// rb.AddTorque(requiredAngularAcceleration * steeringAverage * 0.01f, ForceMode.Acceleration);

			rb.AddRelativeTorque(GetDirection(vectorsAverage) * 0.5f);

			rb.AddRelativeForce(Vector3.forward * vectorsAverage.magnitude);

			rb.velocity = ClampMagnitudeMin(rb.velocity, minVelocityMagnitude);
			rb.velocity = ClampMagnitudeMax(rb.velocity, maxVelocityMagnitude);

			drawAgentVector(gameObject, gameObject.transform.InverseTransformDirection(vectorsAverage), () =>
				maxPrio > 0 ? Color.yellow : Color.green
			);
		}
	}
	Vector3 GetDirection (Vector3 dir) {
		float side = 1/-Mathf.Sqrt(2);
		Vector3 FORWARD_DOWN_LEFT = new Vector3(-side, -side, side);
		Vector3 FORWARD_TOP_LEFT = new Vector3(-side, side, side);
		Vector3 FORWARD_DOWN_RIGHT = new Vector3(side, -side, side);
		Vector3 FORWARD_TOP_RIGHT = new Vector3(side, side, side);


		Quaternion diff = Quaternion.FromToRotation(Vector3.forward, gameObject.transform.InverseTransformDirection(dir));
		Vector3 euler = diff.eulerAngles;
		Vector3 orientation = GetOrientation(euler);

		// print(orientation);

		Vector3 result;

		if (orientation.y < 0 && orientation.x > 0) { result = Quaternion.FromToRotation(Vector3.forward, FORWARD_DOWN_LEFT).eulerAngles; }
		else if (orientation.y < 0 && orientation.x < 0) { result = Quaternion.FromToRotation(Vector3.forward, FORWARD_TOP_LEFT).eulerAngles; }
		else if (orientation.y > 0 && orientation.x > 0) { result = Quaternion.FromToRotation(Vector3.forward, FORWARD_DOWN_RIGHT).eulerAngles; }
		else if (orientation.y > 0 && orientation.x < 0) { result = Quaternion.FromToRotation(Vector3.forward, FORWARD_TOP_RIGHT).eulerAngles; }
		else if (orientation.y < 0 && orientation.x == 0) { result = Quaternion.FromToRotation(Vector3.forward, Vector3.left).eulerAngles; }
		else if (orientation.y > 0 && orientation.x == 0) { result = Quaternion.FromToRotation(Vector3.forward, Vector3.right).eulerAngles; }
		else if (orientation.y == 0 && orientation.x > 0) { result = Quaternion.FromToRotation(Vector3.forward, Vector3.down).eulerAngles; }
		else if (orientation.y == 0 && orientation.x < 0) { result = Quaternion.FromToRotation(Vector3.forward, Vector3.up).eulerAngles; }
		else { result = Vector3.zero; }

		print(orientation);
		return result;
	}

	Vector3 ClampMagnitudeMax (Vector3 vec, float magnitude) {
		return Vector3.ClampMagnitude(vec, magnitude);
	}

	Vector3 ClampMagnitudeMin (Vector3 vec, float magnitude) {
		if (vec.magnitude < magnitude) { return vec; }
		else {
			return vec * (magnitude / vec.magnitude);
		}
	}


	void drawAgentVector (GameObject gameObj, Vector3 vec, Func<Color> colorFn) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(vec);
		Debug.DrawLine(pos, pos + dir, colorFn());
	}
}
