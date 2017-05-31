using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Functional;

public class ObjectSpawner : MonoBehaviour {

	public GameObject obj;
	public int objNB = 50;
	public float range = 10;
	private GameObject[] array;
	// Use this for initialization
	void Start () {
		array = new GameObject[objNB];
		for (int i = 0; i < objNB - 1; i++)
		{
			GameObject child = UnityEngine.Object.Instantiate(obj);
			array[i] = child;
			child.transform.parent = gameObject.transform;
			child.transform.position = new Vector3(
				UnityEngine.Random.Range(-range, range),
				UnityEngine.Random.Range(-range, range),
				UnityEngine.Random.Range(-range, range)
			);
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
