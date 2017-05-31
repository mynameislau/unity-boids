using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Functional;

namespace MathUtils {
	public class V {
		public static float? Average (IEnumerable<float> collection) {
			int length = F.Length(collection);
			if (length == 0) { return null; }
			float sum = F.Reduce(0f, (acc, curr) => acc + curr, collection);
			return sum / length;
		}

		public static Vector3? Average (IEnumerable<Vector3> collection) {
			int length = F.Length(collection);
			if (length == 0) { return null; }
			Vector3 sum = F.Reduce(new Vector3(0,0,0), (acc, curr) => acc + curr, collection);
			return sum / length;
		}
	}
}