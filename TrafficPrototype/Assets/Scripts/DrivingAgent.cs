using UnityEngine;

public enum DrivingAgentState {
    Driving,
    JoiningInternalLane,
    JoiningExternalLane
}


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
	
    public float MinimumFrontDistance = 1f;
    public float FrontDistancePerSpeed = .10f;

    public bool EnableCollaborationFeatures = true;
    public float TargetApproachDistance = 1.0f;
    public float TargetApproachMinRelativeSpeed = 0.35f;

    [Range(1, 3)] public int CurrentLane = 1;
	private int nextLane = 0;

    public const float LanesWidth = 2f; // Width of single lanes
    [Range(0.15f, 0.9f)] public const float LaneMergingError = 0.005f; // Tolerated error when merging lane
    [Range(0.1f, 0.8f)] public const float LaneMergingMaxSteeringAngle = 0.30f;
    public Vector3 TrafficDirection = Vector3.left; // Left for UK/Japan traffic, Right for intl.

    private Vector3 currentTarget;

    private const float DistanceThreshold = .8f;

    [Range(0.01f, 0.9f)] public float LaneMergingMinRelativeSpeed = 0.1f;

    [Range(0.02f, 1.0f)] public float LaneMergingMaxRelativeSpeed = 0.8f;

    public float LaneMergingSpeedOffset = 0.20f;

    // TODO: temporary public for debugging purposes
    public DrivingAgentState CurrentState = DrivingAgentState.Driving;

    private VehicleAgent vehicle;
    private NavigationAgent navigator;
    private Vector3 currentTargetDirection;
    
	public void SetNextTarget(Vector3 target, Vector3 targetDirection, int nextLaneNumber) {
        currentTarget = target;
        currentTargetDirection = targetDirection;
		nextLane = nextLaneNumber; 
    }

	public void SetNextTarget(Vector3 target, Vector3 targetDirection) {
		SetNextTarget(target, targetDirection, 0);
	}

    public void SetLane(int lane) {
        CurrentLane = lane;
    }

    // Use this for initialization
    void Start() {
        vehicle = gameObject.GetComponent<VehicleAgent>();
        navigator = gameObject.GetComponent<NavigationAgent>();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.black;
        if (currentTarget != default(Vector3)) {
            Gizmos.DrawCube(GetLaneAdjustedTarget(), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (CurrentState) {
		case DrivingAgentState.Driving:
			KeepDesiredSpeed ();
			KeepSafeDistance ();
			SteerTowardsTarget ();
            break;

		case DrivingAgentState.JoiningInternalLane:
		case DrivingAgentState.JoiningExternalLane:
			KeepDesiredSpeed ();
			KeepSafeDistance ();
			JoinLaneIfSafe(TrafficDirection * GetChangeInLaneInt(CurrentState));
            break;

        default:
            break;
        }
    }

    private int GetChangeInLaneInt(DrivingAgentState state) {
        switch (state) {
            case DrivingAgentState.JoiningInternalLane:
                return -1;
            case DrivingAgentState.JoiningExternalLane:
                return 1;
            default:
                return 0;
        }
    }

	private void KeepDesiredSpeed() {
		if (NeedsAcceleration ()) {
			vehicle.Accelerate ();
		} else if (NeedsBraking ()) {
			vehicle.Brake ();
		}
	}

	private void KeepSafeDistance() {
		if (ObstaclePresentInFront()) {
			vehicle.Brake();
		}
	}

    private void JoinLaneIfSafe(Vector3 direction) {

        // Debug.Log("Current speed is " + vehicle.GetCurrentSpeed() + ", ideal speed " + idealLaneMergingSpeed.ToString());
        if (ReachedDestinationLane()) {
            // Debug.Log("Reached destination lane");
            CurrentLane = GetDesiredLane();
            CurrentState = DrivingAgentState.Driving;
			return;
        }
 
        if (IsDesiredLaneFreeOfObstacles()) {
            Debug.Log("Lane is free. Steering towards it...");

            Vector3 dir = (GetLaneAdjustedTarget() - gameObject.transform.position).normalized;
            float currentSteeringAngle = Vector3.Dot(dir, transform.forward);

            // Steer towards lane.
			direction = TrafficDirection == Vector3.left ? direction : -direction;
            if (direction == Vector3.left && currentSteeringAngle > 1 - LaneMergingMaxSteeringAngle)
                vehicle.SteerLeft();

            else if (direction == Vector3.right && currentSteeringAngle > 1 - LaneMergingMaxSteeringAngle)
                vehicle.SteerRight();
        }
        
    }

    private float GetCurrentIdealLaneMergingSpeed() {
        float distance = Vector3.Distance(ProjectPositionOnDesiredLane(), gameObject.transform.position);
        distance = Mathf.Abs(distance / LanesWidth);
        float curveResult = LaneMergingSpeedCurve(distance) * LaneMergingMaxRelativeSpeed * vehicle.maximumVehicleSpeed;
        return Mathf.Max(LaneMergingMinRelativeSpeed * vehicle.maximumVehicleSpeed, curveResult);
    }

    private float LaneMergingSpeedCurve(float distance) {
        if (distance < 0) {
            distance = 0f;
        }
        else if (distance > 1) {
            distance = 1f;
        }
        return (Mathf.Sin(2f * Mathf.PI * (distance - 1f / 4f)) + 1f) / 2f;
    }

    public float GetTargetApproachAbsoluteDistance() {
        return TargetApproachDistance * GetOptimalFrontDistance();
    }
    
    private float DesiredSpeedCurve(float distance) {
        float targetApproachAbsoluteDistance = GetTargetApproachAbsoluteDistance();
		if (distance >= targetApproachAbsoluteDistance) {
            return 1;
        }
		float remainingDistance = targetApproachAbsoluteDistance - distance;
		remainingDistance /= targetApproachAbsoluteDistance; // 0...1
		float result = 1 - remainingDistance;
		if (result < TargetApproachMinRelativeSpeed)
            return TargetApproachMinRelativeSpeed;
        return result;
    }


    private int GetDesiredLane() {
        return CurrentLane + GetChangeInLaneInt(CurrentState);
    }

    private Vector3 GetDesiredLaneAdjustedTarget() {
		return GenerateLaneAdjustedTarget(TrafficDirection, GetDesiredLane(), nextLane);
    }

    private bool ReachedDestinationLane() {
        Vector3 positionRelativeToDestinationLaneAdjustedTarget =
            GetDesiredLaneAdjustedTarget() - gameObject.transform.position;
//            .InverseTransformPoint(gameObject.transform.position);
        float xDistance = Mathf.Abs(positionRelativeToDestinationLaneAdjustedTarget.x);
        return xDistance <= LaneMergingError * LanesWidth;
    }

    private float GetVehicleLength() {
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

    private bool IsDesiredLaneFreeOfObstacles() {
        float rayLength = GetVehicleLength() + GetMaximumFrontDistance();
        Vector3 rayOriginPosition = ProjectPositionOnDesiredLane();
        Vector3 rayDirection = -currentTargetDirection;
        RaycastHit hit;
		Debug.DrawRay(rayOriginPosition, rayDirection * rayLength, Color.blue, 0.05f, false);
        return Physics.Raycast(rayOriginPosition, rayDirection, rayLength);
    }

    // Checks whether there is an obstacle in front of the vehicle or not at a specific distance
    private bool ObstaclePresentAtDistance(float distance) {
        Vector3 forward = gameObject.transform.TransformVector(Vector3.forward);
        Debug.DrawRay(gameObject.transform.position, forward * distance, Color.red, 0.05f, false);
        RaycastHit hit;

        // 
        bool isPresent = Physics.Raycast(gameObject.transform.position, forward, out hit, distance);
        if (!EnableCollaborationFeatures || !isPresent) {
            return isPresent;
        }

        float obstacleDistance = hit.distance;
        GameObject obstacle = hit.collider.gameObject;

        bool isVehicle = obstacle.GetComponent<DrivingAgent>() != null &&
                         obstacle.GetComponent<DrivingAgent>().CurrentState == DrivingAgentState.Driving;
        if (!isVehicle) {
            return isPresent;
        }

        float otherDistance = obstacle.GetComponent<DrivingAgent>().GetOptimalFrontDistance();
        float distanceToFrontVehicle = (distance - otherDistance) + MinimumFrontDistance;
        return hit.distance < distanceToFrontVehicle;
    }

    // Checks whether there is an obstacle in front of the vehicle at an optimal distance (based on current speed)
    private bool ObstaclePresentInFront() {
        float distance = GetOptimalFrontDistance();
        return ObstaclePresentAtDistance(distance);
    }

    public float GetOptimalFrontDistance() {
		return MinimumFrontDistance + (FrontDistancePerSpeed * Mathf.Pow(vehicle.GetCurrentSpeed(), 2));
    }

    public float GetMaximumFrontDistance() {
        return MinimumFrontDistance + (FrontDistancePerSpeed * vehicle.maximumVehicleSpeed);
    }


    private bool NeedsAcceleration() {
        return vehicle.GetCurrentSpeed() < GetDesiredSpeed();
    }

    private bool NeedsBraking() {
        return vehicle.GetCurrentSpeed() > GetDesiredSpeed();
    }

    private float GetDesiredSpeed() {
		float desiredSpeed;

		if (currentTarget != default(Vector3)) {
			float distanceToTarget = Vector3.Distance (gameObject.transform.position, GetLaneAdjustedTarget ());
			return DesiredSpeedCurve (distanceToTarget) * vehicle.maximumVehicleSpeed;
		}

		desiredSpeed = vehicle.maximumVehicleSpeed;

		if (CurrentState != DrivingAgentState.Driving) {
			desiredSpeed = GetCurrentIdealLaneMergingSpeed ();

			if (!IsDesiredLaneFreeOfObstacles()) {
				// You want to be stopped while waiting for your desired lane to free.s
				return 0;
			}
		}

		return desiredSpeed;
    }
		
    private bool IsMoving() {
        return (Mathf.Abs(vehicle.GetCurrentSpeed()) > 0);
    }

    private Vector3 NeededSteeringDirection() {
        Vector3 relativePoint = gameObject.transform.InverseTransformPoint(GetLaneAdjustedTarget());
        if (relativePoint.x > 0) {
            return Vector3.right;
        }
        else if (relativePoint.x < 0) {
            return Vector3.left;
        }
        else {
            return Vector3.zero;
        }
    }

	private float NeededSteeringRatio() {
		Vector3 relativePoint = gameObject.transform.InverseTransformPoint(GetLaneAdjustedTarget()).normalized;
		return Mathf.Abs (relativePoint.x);
	}

    private void SteerTowardsTarget() {
		float ratio = NeededSteeringRatio ();
        if (NeededSteeringDirection() == Vector3.left) {
            vehicle.SteerLeft(ratio);
        }
        else if (NeededSteeringDirection() == Vector3.right) {
            vehicle.SteerRight(ratio);
        }
    }

	private Vector3 GenerateLaneAdjustedTarget(Vector3 direction, int lane, int laneAtNextJunction) {
        Vector3 laneAdjustedTargetPosition;

		var right = Vector3.Cross(currentTargetDirection, Vector3.down).normalized;
	    
	    laneAtNextJunction = direction == Vector3.left ? -laneAtNextJunction : laneAtNextJunction;
	    
	    if (direction == Vector3.left) {
	        laneAdjustedTargetPosition = currentTarget - (right * GetLaneOffset(lane));
	        
	    } else {
	        laneAdjustedTargetPosition = currentTarget + (right * GetLaneOffset(lane));
	    }


	    if (nextLane != 0)
	        laneAdjustedTargetPosition += laneAtNextJunction * currentTargetDirection 
	                                      * GetLaneOffset (Mathf.Abs(laneAtNextJunction));
		
        return laneAdjustedTargetPosition;
    }


    public static float GetLaneOffset(int lane) {
		if (lane == 0)
			return 0f;
        
        float offset = ((lane - 1) * LanesWidth) + 0.5f * LanesWidth;
        return offset;
    }

    float GetCurrentLaneOffset() {
        return GetLaneOffset(CurrentLane);
    }

    private Vector3 GetLaneAdjustedTarget() {
        return GenerateLaneAdjustedTarget(TrafficDirection, CurrentLane, nextLane);
    }

    public bool ReachedCurrentTarget() {
        var distance = Vector3.Distance(transform.position, GetLaneAdjustedTarget());
        return distance <= DistanceThreshold;
    }

    
    public bool IsAtJunction(float radius) {
        var distance = Vector3.Distance(transform.position, navigator.ClosestWaypoint().transform.position);
        return distance < radius;
    }

    public float GetLaneMergingMinSpeed() {
        return LaneMergingMinRelativeSpeed * vehicle.maximumVehicleSpeed;
    }

    public float GetLaneMergingMaxSpeed() {
        return LaneMergingMaxRelativeSpeed * vehicle.maximumVehicleSpeed;
    }
}