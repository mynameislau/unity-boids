using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoidsNS
{
    public struct InfluencesData {
    // public Vector3 torque;
    // public float magnitude;
    public Vector3 vector;

    public InfluencesData (Vector3 v) {
      vector = v;
    }
  }

  public struct Influence {
		public string name;
    public int priority;
		public InfluencesData? influence;
		public Influence (string n, InfluencesData? i, int p = 0) {
      name = n;
      influence = i;
      priority = p;
    }
	}
}