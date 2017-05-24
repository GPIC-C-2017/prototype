using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This is the agent which represents the mechanical vehicle.
 * It exposes four simple methods: Accelerate, Brake, SteerLeft and SteerRight.
 * These methods are meant to be used to higher-level agents (i.e. the Autonomous Driving
 *  Agent) to control the vehicle.
 */
[RequireComponent (typeof (Rigidbody))]
public class VehicleAgent : MonoBehaviour {

	// Public variables

	[Range (10.0f, 100.0f)]
	public float maximumVehicleSpeed = 85.0f;

	[Range (0.05f, 1.0f)]
	public float steeringResponsiveness = 0.65f;

    [Range(10.0f, 1000.0f)]
    public float maxSteeringTorque = 800.0f;

    [Range(0.5f, 500.0f)]
    public float minSteeringTorque = 10.0f;

    [Range (700.0f, 1100.0f)]
	public float minimumVehicleTorque = 750.0f;

	[Range (500.0f, 5000.0f)]
	public float maximumVehicleTorque = 1100.0f;


	// Public Methods 

	public void Accelerate() {
		Vector3 forceVector = gameObject.transform.forward * GetVehicleAcceleration() * Time.deltaTime;
		rb.AddForce (forceVector);
	}

	public void Brake() {
		Vector3 forceVector = -gameObject.transform.forward * GetVehicleAcceleration() * Time.deltaTime;
		rb.AddForce (forceVector);

	}	

	public void SteerRight() {
		Steer (Vector3.up);
	}

	public void SteerLeft() {
		Steer (Vector3.down);
	}

	public float GetCurrentSpeed() {
		return Vector3.Dot (rb.velocity, gameObject.transform.forward);
	}

	public bool IsMoving() {
		return Mathf.Abs(GetCurrentSpeed()) > 0;
	}

	// MonoBehaviour methods

	void Start () {
		rb = gameObject.GetComponent<Rigidbody> ();
	}

	void Update () {

	}

	// Private variables and methods

	private Rigidbody rb;

	private void Steer(Vector3 direction) {
		Vector3 torqueVector = direction * GetVehicleSteeringTorque () * Time.deltaTime;
		rb.AddRelativeTorque (torqueVector);
	}

	/**
	 * Bell curve, give most power at medium speed
	 */
	private float AccelerationCurveFunction(float speedRatio) {
		if (speedRatio < 0) {
			speedRatio = 0f;
		} else if (speedRatio > 1) {
			speedRatio = 1f;
		}
		return (Mathf.Sin(2f * Mathf.PI * (speedRatio - 1f/4f)) + 1f) / 2f;
	}

	private float GetVehicleAcceleration() {
		float speedRatio = GetCurrentAdirectionalSpeed() / maximumVehicleSpeed;
		float curveResult = AccelerationCurveFunction (speedRatio);
		float outputTorque = curveResult * maximumVehicleTorque;
		outputTorque = Mathf.Max (minimumVehicleTorque, outputTorque);
		return outputTorque;
	}

	private float GetCurrentAdirectionalSpeed() {
		return Mathf.Abs (GetCurrentSpeed ());
	}

	/**
	 * e^(-x), steer well at very low speeds, slowly at high speeds
	 */
	private float SteeringTorqueCurveFunction(float speedRatio) {
		return Mathf.Exp (-speedRatio);
	}

	public float GetVehicleSteeringTorque() {
		if (GetCurrentAdirectionalSpeed () == 0) {
			return 0;
		}

		float speedRatio = GetCurrentAdirectionalSpeed() / maximumVehicleSpeed;
		float curveResult = SteeringTorqueCurveFunction (speedRatio);
		float outputTorque = Mathf.Max(curveResult * steeringResponsiveness * maxSteeringTorque, minSteeringTorque);
		return outputTorque;
	}



}
