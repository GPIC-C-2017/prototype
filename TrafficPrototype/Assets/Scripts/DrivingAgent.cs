using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrivingAgentState {
    Driving, JoiningInternalLane, JoiningExternalLane
};


/**
 * This is the agent which represents the Autonomous Driving capabilities of a vehicle.
 * It is built on top of a 'VehicleAgent' and is able to:
 * - Navigate towards a set target, i.e. the next junction
 * - Use the pedals (VehicleAgent's Accelerate and Brake methods) to keep a safe, consistent speed
 * - Use the steering wheel (VehicleAgent's SteerLeft, and SteerRight methods) to follow the road
 * - Keeping the appropriate safety distance with the vehicle in front
 * - Brake suddenly to avoid close, fixed obstacles
 * - Change lanes and behave appropriately with regards to nearby pods
 * - Report permanent obstructions to the NavigationAgent (i.e. to request for a new target)
 **/
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(VehicleAgent))]
[RequireComponent(typeof(NavigationAgent))]
public class DrivingAgent : MonoBehaviour {

	public float minimumFrontDistance = 1f;
	public float frontDistancePerSpeed = .5f;
	public bool enableCollaborationFeatures = true;

    [Range(1, 3)]
    public int currentLane = 1;

    public const float lanesWidth = 2.5f;              // Width of single lanes
    [Range(0.15f, 0.9f)]
    public const float laneMergingError = 0.005f;        // Tolerated error when merging lane
    [Range(0.1f, 0.8f)]
    public const float laneMergingMaxSteeringAngle = 0.30f;
    public Vector3 trafficDirection = Vector3.left;    // Left for UK/Japan traffic, Right for intl.

	private Vector3 currentTarget;

    private const float DistanceThreshold = .5f;

    [Range(0.01f, 0.9f)]
    public float laneMergingMinRelativeSpeed = 0.1f;

    [Range(0.02f, 1.0f)]
    public float laneMergingMaxRelativeSpeed = 0.8f;

    public float laneMergingSpeedOffset = 0.20f;

    // TODO: temporary public for debugging purposes
    public DrivingAgentState currentState = DrivingAgentState.Driving;

    public void SetNextTarget(Vector3 target, Vector3 targetDirection) {
		currentTarget = target;
        currentTargetDirection = targetDirection;
    }

    public void SetLane(int lane) {
        currentLane = lane;
    }

	private VehicleAgent vehicle;
    private Vector3 currentTargetDirection;

    // Use this for initialization
	void Start () {
		vehicle = gameObject.GetComponent<VehicleAgent> ();
		
	}

    void OnDrawGizmos() {
        Gizmos.color = Color.black;
        if (currentTarget != default(Vector3)) {
            Gizmos.DrawCube(GetLaneAdjustedTarget(), new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    // Update is called once per frame
    void FixedUpdate() {

        switch (currentState)
        {
            case DrivingAgentState.Driving:
                Drive();
                break;

            case DrivingAgentState.JoiningInternalLane:
            case DrivingAgentState.JoiningExternalLane:
                JoinLane(trafficDirection * GetChangeInLaneInt(currentState));
                break;

            default:
                break;
        }

    }

    private int GetChangeInLaneInt(DrivingAgentState state)
    {
        if (state == DrivingAgentState.JoiningInternalLane)
        {
            return -1;
        } else if (state == DrivingAgentState.JoiningExternalLane)
        {
            return 1;
        } else
        {
            return 0;
        }
    }

    private void Drive() {
        if (vehicle.IsMoving())
        {

            // Acceleration

            if (ObstaclePresentInFront())
            {
                //Debug.Log ("Obstacle detected! Braking.");
                vehicle.Brake();

            }
            else if (NeedsAcceleration())
            {
                //Debug.Log("Way is clear. Accelerating...");
                vehicle.Accelerate();

            }
            else
            {
                // Coast...

            }

            SteerTowardsTarget();

        }
        else
        {

            if (NeedsAcceleration())
            {
                vehicle.Accelerate();

            }

        }
    }

    private void JoinLane(Vector3 direction) {
        
        float idealLaneMergingSpeed = GetCurrentIdealLaneMergingSpeed();

        // Debug.Log("Current speed is " + vehicle.GetCurrentSpeed() + ", ideal speed " + idealLaneMergingSpeed.ToString());

        if (vehicle.GetCurrentSpeed() < idealLaneMergingSpeed)
        {
            // Need to get to lane merging minimum speed
            // Debug.Log("Merging - Accelerating (Need to get to lane merging minimum speed...");
            vehicle.Accelerate();

        }
        else if (vehicle.GetCurrentSpeed() > idealLaneMergingSpeed + laneMergingSpeedOffset * idealLaneMergingSpeed)
        {
            // Need to slow down to lane merging minimum speed
            // Debug.Log("Merging - Braking (Need to get to lane merging minimum speed...");
            vehicle.Brake();
        }

        
        if (ReachedDestinationLane())
        {
            // Debug.Log("Reached destination lane");
            currentLane = GetDesiredLane();
            currentState = DrivingAgentState.Driving;
        }
        else
        {
            if (IsDesiredLaneFreeOfObstacles())
            {
                // Debug.Log("Lane is free. Steering towards it...");

                Vector3 dir = (GetLaneAdjustedTarget() - gameObject.transform.position).normalized;
                float currentSteeringAngle = Vector3.Dot(dir, transform.forward);

                // Steer towards lane.
                if (direction == Vector3.left && currentSteeringAngle > 1 - laneMergingMaxSteeringAngle)
                    vehicle.SteerLeft();

                else if (direction == Vector3.right && currentSteeringAngle > 1 - laneMergingMaxSteeringAngle)
                    vehicle.SteerRight();

            } else
            {
                // Debug.Log("Waiting for lane to free...");
                // Coast and wait for lane to free.
                // Maybe implement a timer for giving up all hope?
            }
        }

    }

    private float GetCurrentIdealLaneMergingSpeed() {
        float distance = Vector3.Distance(ProjectPositionOnDesiredLane(), gameObject.transform.position);
        distance = Mathf.Abs(distance / lanesWidth);
        float curveResult = LaneMergingSpeedCurve(distance) * laneMergingMaxRelativeSpeed * vehicle.maximumVehicleSpeed;
        return Mathf.Max(laneMergingMinRelativeSpeed * vehicle.maximumVehicleSpeed, curveResult);
    }

    private float LaneMergingSpeedCurve(float distance)
    {
        if (distance < 0)
        {
            distance = 0f;
        }
        else if (distance > 1)
        {
            distance = 1f;
        }
        return (Mathf.Sin(2f * Mathf.PI * (distance - 1f / 4f)) + 1f) / 2f;
    }

    private int GetDesiredLane()
    {
        return currentLane + GetChangeInLaneInt(currentState);
    }

    private Vector3 GetDesiredLaneAdjustedTarget()
    {
        return GenerateLaneAdjustedTarget(trafficDirection, GetDesiredLane());
    }

    private bool ReachedDestinationLane() {
        Vector3 positionRelativeToDestinationLaneAdjustedTarget =
            GetDesiredLaneAdjustedTarget() - gameObject.transform.position;
//            .InverseTransformPoint(gameObject.transform.position);
        float xDistance = Mathf.Abs(positionRelativeToDestinationLaneAdjustedTarget.x);
        return xDistance <= laneMergingError * lanesWidth;
    }

    private float GetVehicleLength()
    {
        return gameObject.GetComponent<Collider>().bounds.size.z;
    }

    private Vector3 ProjectPositionOnDesiredLane() {
        Vector3 positionRelativeToDestinationLaneAdjustedTarget = 
            GetDesiredLaneAdjustedTarget() - gameObject.transform.position;
//            .InverseTransformPoint(gameObject.transform.position);
        float zDistance = Mathf.Abs(positionRelativeToDestinationLaneAdjustedTarget.z);
        Vector3 position = GetDesiredLaneAdjustedTarget() - (currentTargetDirection * zDistance);
//            .TransformPoint(Vector3.back * zDistance);
        position.y = gameObject.transform.position.y;
        return position;
    }

    private bool IsDesiredLaneFreeOfObstacles()
    {
        float rayLength = GetVehicleLength() + GetMaximumFrontDistance();
        Vector3 rayOriginPosition = ProjectPositionOnDesiredLane();
        Vector3 rayDirection = -currentTargetDirection;
        RaycastHit hit;
        bool isPresent = Physics.Raycast(rayOriginPosition, rayDirection, out hit, rayLength);
        Debug.DrawRay(rayOriginPosition, rayDirection * rayLength, Color.blue, 0.05f, false);
        return !isPresent;
    }

	// Checks whether there is an obstacle in front of the vehicle or not at a specific distance
	private bool ObstaclePresentAtDistance(float distance) {
		
		Vector3 forward = gameObject.transform.TransformVector(Vector3.forward);
		Debug.DrawRay (gameObject.transform.position, forward * distance, Color.red, 0.05f, false);
		RaycastHit hit;

        // 
		bool isPresent = Physics.Raycast (gameObject.transform.position, forward, out hit, distance);
		if (!enableCollaborationFeatures || !isPresent) {
			return isPresent;
		}

		float obstacleDistance = hit.distance;
		GameObject obstacle = hit.collider.gameObject;

		bool isVehicle = obstacle.GetComponent<DrivingAgent>() != null && obstacle.GetComponent<DrivingAgent>().currentState == DrivingAgentState.Driving;
        if (!isVehicle) {
            return isPresent;
        }

        float otherDistance = obstacle.GetComponent<DrivingAgent>().GetOptimalFrontDistance();
        float distanceToFrontVehicle = (distance - otherDistance) + minimumFrontDistance;
        return hit.distance < distanceToFrontVehicle;

    }

	// Checks whether there is an obstacle in front of the vehicle at an optimal distance (based on current speed)
	private bool ObstaclePresentInFront() {
        float distance = GetOptimalFrontDistance();
		return ObstaclePresentAtDistance (distance);
	}

    public float GetOptimalFrontDistance()
    {
        return minimumFrontDistance + (frontDistancePerSpeed * vehicle.GetCurrentSpeed());
    }

    public float GetMaximumFrontDistance()
    {
        return minimumFrontDistance + (frontDistancePerSpeed * vehicle.maximumVehicleSpeed);
    }


    private bool NeedsAcceleration() {
		return true;
	}
		
	private bool IsMoving() {
		return (Mathf.Abs (vehicle.GetCurrentSpeed ()) > 0);
	}

	private Vector3 NeededSteeringDirection() {
		Vector3 relativePoint = gameObject.transform.InverseTransformPoint(GetLaneAdjustedTarget());
		if (relativePoint.x > 0) {
			return Vector3.right;
		} else if (relativePoint.x < 0) {
			return Vector3.left;
		} else {
			return Vector3.zero;
		}		
	}

	private void SteerTowardsTarget() {
		if (NeededSteeringDirection () == Vector3.left) {
			vehicle.SteerLeft ();
		} else if (NeededSteeringDirection () == Vector3.right) {
			vehicle.SteerRight ();
		}
	}
    
    private Vector3 GenerateLaneAdjustedTarget(Vector3 direction, int lane) {
        Vector3 laneAdjustedTargetPosition;
        var right = Vector3.Cross(currentTargetDirection, Vector3.up).normalized;
        if (direction == Vector3.left)
        {
            laneAdjustedTargetPosition = currentTarget - right * GetLaneOffset(lane);
        } else
        {
            laneAdjustedTargetPosition = currentTarget + right * GetLaneOffset(lane);
        }
        return laneAdjustedTargetPosition;
    }

    float GetLaneOffset(int lane)
    {
        float offset = ((lane - 1) * lanesWidth) + 0.5f * lanesWidth;
        return offset;
    }

    float GetCurrentLaneOffset()
    {
        return GetLaneOffset(currentLane);
    }

    private Vector3 GetLaneAdjustedTarget()
    {
        return GenerateLaneAdjustedTarget(trafficDirection, currentLane);
    }

    public bool ReachedCurrentTarget() {
        var distance = Vector3.Distance(transform.position, GetLaneAdjustedTarget());
        return distance <= DistanceThreshold;
    }

    public float GetLaneMergingMinSpeed()
    {
        return laneMergingMinRelativeSpeed * vehicle.maximumVehicleSpeed;
    }

    public float GetLaneMergingMaxSpeed()
    {
        return laneMergingMaxRelativeSpeed * vehicle.maximumVehicleSpeed;
    }

}
