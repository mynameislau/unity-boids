using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using RayNav;
using BoidsNS;
using MathUtils;
public class Sensing : MonoBehaviour {

	// private Rigidbody rb;
	// Use this for initialization
	private const float Infinity = 1/0f;
	private const float feelerLength = 5;
	private const float avoidanceStrength = 0.1f;
	private const float detectionFrequency = 0.5f;
	private Vector3? debugAverageReflection;
	private Vector3? debugAverageFeelerDirs;
	private Feeler[] feelers = {
		new Feeler("top", Normalize(new Vector3(0, 1, 1))),
		new Feeler("bottom", Normalize(new Vector3(0, -1, 1))),
		new Feeler("right", Normalize(new Vector3(1, 0, 1))),
		new Feeler("left", Normalize(new Vector3(-1, 0, 1))),
		new Feeler("forward", Vector3.forward)
	};
	private IEnumerable<bool> hitting;
	private IEnumerable<Vector3> feelerDirs;
	//private string[] feelerNames;
	private AvoidanceData? maybeAvoidanceData = null;

	private Navigator navigator;

	void Start () {
		feelerDirs = F.Map(Feeler.getDirection, feelers);
		//feelerNames = F.Map(Feeler.getName, feelers);
		// gameObject = gameObject.transform.GetChild(0).gameObject;
		// rb = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
		navigator = gameObject.GetComponent(typeof(Navigator)) as Navigator;
		// rb.velocity = new Vector3(
		// 	UnityEngine.Random.Range(-1, 1),
		// 	0,
		// 	UnityEngine.Random.Range(-1, 1)
		// );
		StartCoroutine(AvoidanceRoutine());
	}

	IEnumerator AvoidanceRoutine () {

		// maybeAvoidanceData = Avoidance();
		navigator.RegisterInfluence(new Influence("obstacles", Avoidance(), 1));
		yield return new WaitForSeconds(detectionFrequency);
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
			// drawAgentVector(maybeAvoidanceData.Value.torque * 10f, () => Color.yellow);
			//
		}
		if (debugAverageReflection.HasValue) {
			drawAgentVector(debugAverageReflection.Value, () => Color.magenta);
		}
		if (debugAverageFeelerDirs.HasValue) {
			drawAgentVector(debugAverageFeelerDirs.Value, () => Color.cyan);
		}
	}

	Vector3 ExponentialSimple (Vector3 vec) {
		float magnitude = vec.magnitude;
		Vector3 normalized = Vector3.Normalize(vec);
		return normalized * (1 - magnitude);
	}

	struct AvoidanceData {	public Vector3 torque; public float magnitude; }

	InfluenceData? Avoidance () {
		IEnumerable<FeelerData> castResults = F.Map(feelerDir => cast(gameObject, feelerDir), feelerDirs); 
		hitting = F.Map(result => result.positive, castResults);

		IEnumerable<FeelerData> positives = F.Filter(result => result.positive, castResults);
		IEnumerable<FeelerData> negatives = F.Filter(result => !result.positive, castResults);

		// TODO : Mixer les reflections pour les feelers qui ont un résultat, et les directions brutes de ceux qui n'ont pas de résultat, ca nous donne un bon chemin...
		debugAverageReflection = null;
		debugAverageFeelerDirs = null;

		if (F.Length(positives) > 0) {
			Vector3 averageReflection = V.Average(F.Map(current => current.reflection.Value, positives)).Value;
			Vector3? averageUntriggeredFeelerDirs = V.Average(F.Map(current => current.feelerDir, negatives));
			debugAverageReflection = averageReflection;

			if (averageUntriggeredFeelerDirs.HasValue) {
				float ln = averageReflection.magnitude;
				averageReflection = Normalize(Normalize(averageReflection) + Normalize(averageUntriggeredFeelerDirs.Value)) * ln;
				// averageReflection = V.Average(new Vector3[] { averageReflection, averageUntriggeredFeelerDirs.Value }).Value;
				debugAverageFeelerDirs = averageUntriggeredFeelerDirs;
			}


			// Vector3? average = V.Average(F.Map(current => current.hitData, filtered));
			// Vector3 averageDefault = average.HasValue ? average.Value : Vector3.zero;

			// Vector3 outVector = averageDefault;

			// expOutVector = ExponentialSimple(outVector);
			// Quaternion rot = Quaternion.FromToRotation(Vector3.forward, averageReflectionDefault);
			// Vector3 torque = GetTorque(rot.eulerAngles);
			// torque.z = 0;

			// AvoidanceData data;
			// data.torque = torque;
			// data.magnitude = outVector.magnitude;
			return new InfluenceData(gameObject.transform.TransformDirection(averageReflection));
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

	// FeelerData feelerData (bool positive, Vector3 vec = null, Vector3 normal = null) {
		
	// }

	struct FeelerData
	{
		public Vector3? hitData;
		public Vector3? reflection;

		public bool positive;

		public Vector3 feelerDir;

		public FeelerData (bool pPositive, Vector3 f, Vector3? mVec = null, Vector3? mNormal = null) {
			positive = pPositive;
			feelerDir = f;
			if (mVec.HasValue) {
				float linear = mVec.Value.magnitude / feelerLength;
				Vector3 norm = Normalize(mVec.Value);
				hitData = norm * linear;
				reflection = Vector3.Reflect(hitData.Value, mNormal.Value);
				reflection = Vector3.Slerp(Vector3.forward, reflection.Value, 1 - reflection.Value.magnitude);
			}
			else {
				hitData =  null;
				reflection = null;
			}
		}
	}

	FeelerData cast(GameObject gameObj, Vector3 feelerDir) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(feelerDir);
		RaycastHit rh = new RaycastHit();
		if (Physics.Raycast(pos, dir, out rh, feelerLength)) {
			Vector3 local =  gameObj.transform.InverseTransformDirection(rh.point - pos);
			return new FeelerData(true, feelerDir, local, gameObj.transform.InverseTransformDirection(rh.normal));
		}
		else {
			return new FeelerData(false, feelerDir);
		}
	}

	void drawAgentVector (Vector3 vec, Func<Color> colorFn) {
			Vector3 pos = gameObject.transform.TransformPoint(Vector3.zero);
			Vector3 dir = gameObject.transform.TransformDirection(vec);
			Debug.DrawLine(pos, pos + dir, colorFn());
	}

	void debugFeelers () {
		F.ForEach((feelerDir, index) => {
			drawAgentVector(feelerDir * feelerLength, () => F.Nth(index, hitting) ? Color.red : Color.grey);
		}, feelerDirs);
	}
}
