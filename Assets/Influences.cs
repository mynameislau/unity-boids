using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoidsNS
{
    public struct InfluenceData {
    // public Vector3 torque;
    // public float magnitude;
    public float steeringModifer;
    public Vector3 vector;

    public InfluenceData (Vector3 v, float s = 1) {
      vector = v;
      steeringModifer = s;
    }
  }

  public struct Influence {
		public string name;
    public int priority;
		public InfluenceData? influenceData;
		public Influence (string n, InfluenceData? i, int p = 0) {
      name = n;
      influenceData = i;
      priority = p;
    }
	}
}