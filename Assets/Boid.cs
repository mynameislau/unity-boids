using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoidsNS
{
  public class Boid {
    public Rigidbody body;
    public GameObject gameObject;

    public Boid (GameObject go, Rigidbody rb) {
      body = rb;
      gameObject = go;
    }
  }
}