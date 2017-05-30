using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using RayNav;
using BoidsNS;
public class Sensing : MonoBehaviour {

	private Rigidbody rb;
	private GameObject agentObj;
	// Use this for initialization
	private const float Infinity = 1/0f;
	private const float feelerLength = 3;
	private const float avoidanceStrength = 0.1f;
	private Feeler[] feelers = {
		new Feeler("top", Normalize(new Vector3(0, 1, 1))),
		new Feeler("bottom", Normalize(new Vector3(0, -1, 1))),
		new Feeler("right", Normalize(new Vector3(1, 0, 1))),
		new Feeler("left", Normalize(new Vector3(-1, 0, 1))),
		new Feeler("forward", Vector3.forward)
	};
	private bool[] hitting;
	private Vector3[] feelerDirs;
	//private string[] feelerNames;
	private AvoidanceData? maybeAvoidanceData = null;

	private Navigator navigator;

	void Start () {
		feelerDirs = F.Map(Feeler.getDirection, feelers);
		//feelerNames = F.Map(Feeler.getName, feelers);

		agentObj = gameObject;
		// agentObj = gameObject.transform.GetChild(0).gameObject;
		rb = agentObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
		Navigator navigator = agentObj.GetComponent(typeof(Navigator)) as Navigator;
		// rb.velocity = new Vector3(
		// 	UnityEngine.Random.Range(-1, 1),
		// 	0,
		// 	UnityEngine.Random.Range(-1, 1)
		// );
		StartCoroutine(AvoidanceRoutine());
	}

	IEnumerator AvoidanceRoutine () {

		// maybeAvoidanceData = Avoidance();
		navigator.RegisterInfluence("obstacles", Avoidance());
		yield return new WaitForSeconds(1);
		StartCoroutine(AvoidanceRoutine());
	}

	static Vector3 Normalize (Vector3 vec) {
		Vector3 nVec = vec;
		nVec.Normalize();
		return nVec;
	}
	
	// Update is called once per frame
	void Update () {
		debugFeelers();
		if (maybeAvoidanceData.HasValue) {
			//debug stuff
			drawAgentVector(maybeAvoidanceData.Value.torque * 10f, () => Color.yellow);
			//
		}
	}

	Vector3? Average (Vector3[] vectors) {
		if (vectors.Length == 0) { return null; }
		Vector3 sum = F.Reduce(Vector3.zero, (acc, curr) => acc + curr, vectors);
		return sum / vectors.Length;
	}

	Vector3 ExponentialSimple (Vector3 vec) {
		float magnitude = vec.magnitude;
		Vector3 normalized = Vector3.Normalize(vec);
		return normalized * (1 - magnitude);
	}

	struct AvoidanceData {	public Vector3 torque; public float magnitude; }

	InfluencesData? Avoidance () {
		FeelerData?[] castResults = F.Map(feelerDir => cast(agentObj, feelerDir), feelerDirs); 
		hitting = F.Map(result => result.HasValue, castResults);

		FeelerData[] filtered = F.FilterOutNulls(castResults);

		if (filtered.Length > 0) {
			Vector3? averageReflection = Average(F.Map(current => current.reflection, filtered));
			Vector3 averageReflectionDefault = averageReflection.HasValue ? averageReflection.Value : Vector3.zero;
			Vector3? average = Average(F.Map(current => current.hitData, filtered));
			Vector3 averageDefault = average.HasValue ? average.Value : Vector3.zero;

			Vector3 outVector = averageDefault;

			// expOutVector = ExponentialSimple(outVector);
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, averageReflectionDefault);
			Vector3 torque = GetTorque(rot.eulerAngles);
			torque.z = 0;

			AvoidanceData data;
			data.torque = torque;
			data.magnitude = outVector.magnitude;
			return new InfluencesData(averageReflectionDefault);
		}
		else {
			return null;
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

	void FixedUpdate () {

		// Vector3 outVector;
		// Vector3 expOutVector;

		// if (maybeAvoidanceData.HasValue) {
		// 	AvoidanceData data = maybeAvoidanceData.Value;
		// 	rb.AddRelativeTorque(data.torque * avoidanceStrength);
		// 	rb.AddRelativeForce(Vector3.forward * data.magnitude);
		// }
		// else {
		// 	outVector = Vector3.forward;
		// 	float zRotation = rb.rotation.eulerAngles.z;
		// 	zRotation = zRotation / 360;
		// 	zRotation = zRotation > 0.5 ? -(1 - zRotation) : zRotation;
		// 	rb.AddRelativeTorque(new Vector3(0, 0, -zRotation * 0.1f));
		// 	rb.AddRelativeForce(Vector3.forward * outVector.magnitude);
		// }
		
	}

	Vector3 Exponential (Vector3 vec) {
		float linear = vec.magnitude;
		double exp = Mathf.Cos(linear * Mathf.PI) / 2 + 0.5;
		return vec * ((float)exp);
	}

	FeelerData feelerData (Vector3 vec, Vector3 normal) {
		float linear = vec.magnitude / feelerLength;
		Vector3 norm = Normalize(vec);
		FeelerData data;
		data.hitData = norm * linear;
		data.reflection = Vector3.Reflect(data.hitData, normal);
		return data;
	}

	struct FeelerData
	{
		public Vector3 hitData;
		public Vector3 reflection;
	}

	FeelerData? cast(GameObject gameObj, Vector3 localDir) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(localDir);
		RaycastHit rh = new RaycastHit();
		if (Physics.Raycast(pos, dir, out rh, feelerLength)) {
			Vector3 local =  gameObj.transform.InverseTransformDirection(rh.point - pos);
			return feelerData(local, gameObj.transform.InverseTransformDirection(rh.normal));
		}
		else {
			return null;
		}
	}

	void drawAgentVector (Vector3 vec, Func<Color> colorFn) {
			Vector3 pos = agentObj.transform.TransformPoint(Vector3.zero);
			Vector3 dir = agentObj.transform.TransformDirection(vec);
			Debug.DrawLine(pos, pos + dir, colorFn());
	}

	void debugFeelers () {
		F.ForEach((feelerDir, index) => {
			drawAgentVector(feelerDir * feelerLength, () => hitting[index] ? Color.red : Color.grey);
		}, feelerDirs);
	}
}
