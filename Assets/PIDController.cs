using System;
using UnityEngine;

public class PIDController : MonoBehaviour {

	public float Kp;
	public float Ki;
	public float Kd;
	private float P, I, D;
	private float prevError;

	public PIDController (float pKp = 1, float pKi = 0, float pKd = 0.1f) {
		Kp = pKp;
		Ki = pKi;
		Kd = pKd;
	}

	void Start () {

	}

	public float GetOutput(float currentError, float deltaTime)
	{
			P = currentError;
			I += P * deltaTime;
			D = (P - prevError) / deltaTime;
			prevError = currentError;
			
			return P*Kp + I*Ki + D*Kd;
	}
}