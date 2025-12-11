using UnityEngine;
using DG.Tweening;

public class ElevatorDoor : Door
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Animation Settings")]
    [SerializeField] private float doorSeparation = 2f;

    [SerializeField] private float openSpeed = 2.5f;
    [SerializeField] private float closeSpeed = 0.5f;
    [SerializeField] private float delayBeforeMoving = 3f;

    private Vector3 leftDoorInitialPos;
    private Vector3 rightDoorInitialPos;

    private void Start()
    {
        CacheInitialPositions();
    }

    private void CacheInitialPositions()
    {
        if (leftDoor != null)
            leftDoorInitialPos = leftDoor.localPosition;
        
        if (rightDoor != null)
            rightDoorInitialPos = rightDoor.localPosition;
    }

    public override void OpenDoor()
    {
        AnimateDoors(
            delayBeforeMoving, 
            leftDoorInitialPos.x - doorSeparation,
            rightDoorInitialPos.x + doorSeparation,
            openSpeed
        );
    }

    public override void CloseDoor()
    {
        AnimateDoors(
            delayBeforeMoving,
            leftDoorInitialPos.x,
            rightDoorInitialPos.x,
            closeSpeed
        );
    }

    private void AnimateDoors(float delayBeforeMoving = 0, float leftTargetX = 1, float rightTargetX= 1, float duration = 1, bool snap= false)
    {
        Sequence doorSequence = DOTween.Sequence();
        doorSequence.AppendInterval(delayBeforeMoving);
        doorSequence.Append(leftDoor.DOLocalMoveX(leftTargetX, duration, snap));
        doorSequence.Join(rightDoor.DOLocalMoveX(rightTargetX, duration, snap));
    }
}