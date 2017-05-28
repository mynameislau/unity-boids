using UnityEngine;
using Functional;

namespace RayNav {
  class Feeler {
    public string name;
    public Vector3 direction;
    public static string getName(Feeler feeler) { return feeler.name; }

    public static Vector3 getDirection(Feeler feeler) { return feeler.direction; }
    
    public Feeler (string pName, Vector3 pDirection) {
      name = pName;
      direction = pDirection;
    }
  }
}