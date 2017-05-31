using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoidsNS
{
  public class Boid {
    public Rigidbody body;
    public Navigator navigator;
    public GameObject gameObject;
    public InfluenceData? influenceData = null;

    public Boid (GameObject go, Rigidbody rb, Navigator n) {
      body = rb;
      gameObject = go;
      navigator = n;
    }
  }
}