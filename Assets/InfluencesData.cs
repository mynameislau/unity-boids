using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoidsNS
{
    public struct InfluencesData {
    public Vector3 torque;
    public float magnitude;
    public Vector3 vector;
    public string name;
  }

	struct NamedInfluence {
		public string name;
		public InfluencesData? influence;
		NamedInfluence (string n, InfluencesData? i) {
      name = n;
      influence = i;
    }
	}
}