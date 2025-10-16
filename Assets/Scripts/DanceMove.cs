using UnityEngine;

public class DanceMove
{
    public Vector3 LeftControllerPosition;
    public Quaternion LeftControllerRotation;

    public Vector3 RightControllerPosition;
    public Quaternion RightControllerRotation;

    public float PositionTolerance = 0.1f; 
    public float RotationTolerance = 5f;
    public float TimeLimit = 5f;

    public DanceMove(Vector3 leftControllerPosition, Quaternion leftControllerRotation, Vector3 rightControllerPosition, Quaternion rightControllerRotation, float positionTolerance, float rotationTolerance, float timeLimit)
    {
        LeftControllerPosition = leftControllerPosition;
        LeftControllerRotation = leftControllerRotation;
        RightControllerPosition = rightControllerPosition;
        RightControllerRotation = rightControllerRotation;
        PositionTolerance = positionTolerance;
        RotationTolerance = rotationTolerance;
        TimeLimit = timeLimit;
    }

    /* based on the threshold when we create the dance move, just check to see if the user is within it
     * this should be locked on the local z-axis i think in terms of positioning since we can't control 
     * how the user does this and it should be relatively irrelevant */
    public bool CheckMove(Vector3 leftPos, Quaternion leftRot, Vector3 rightPos, Quaternion rightRot)
    {
        bool leftPosMatch = Vector3.Distance(LeftControllerPosition, leftPos) <= PositionTolerance;
        bool rightPosMatch = Vector3.Distance(LeftControllerPosition, rightPos) <= PositionTolerance;

        bool leftRotMatch = Quaternion.Angle(LeftControllerRotation, leftRot) <= RotationTolerance;
        bool rightRotMatch = Quaternion.Angle(RightControllerRotation, rightRot) <= RotationTolerance;

        return leftPosMatch && leftRotMatch && rightPosMatch && rightRotMatch;
    }
}
