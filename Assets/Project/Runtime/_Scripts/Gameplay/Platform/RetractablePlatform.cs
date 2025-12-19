using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;
using System.Data.Common;

public class RetractablePlatform : MonoBehaviour
{

    [field: Header("Door References")]
    [field: SerializeField] private Transform leftDoor;
    [field: SerializeField] private Transform rightDoor;

    private enum Axis { X, Y, Z, Vector3 }

    [field: Header("3D Orientation")]
    [field: SerializeField] private Axis tweenAxis = Axis.X;

    [field: Header("Animation Settings")]
    [field: SerializeField] private float doorSeparation = 2f;
    [field: SerializeField] private float openSpeed = 2.5f;
    [field: SerializeField] private float closeSpeed = 0.5f;
    [field: SerializeField] private float delayBeforeMoving = 3f;
    [field: SerializeField] private bool snapToTarget = false;

    [field: Header("Vector3 Mode (only used when Axis = Vector3)")]
    [field: SerializeField] private Vector3 leftDoorOpenOffset = new Vector3(-2f, 0f, 0f);
    [field: SerializeField] private Vector3 rightDoorOpenOffset = new Vector3(2f, 0f, 0f);

    [field: Header("Animation Curves")]
    [field: SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [field: SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private static Sequence doorSequence;
    private Vector3 leftDoorInitialPosition;
    private Vector3 rightDoorInitialPosition;

    [field: Header("Audio")]
    [field: SerializeField] private EventInstance openElevatorAudio;
    [field: SerializeField] private EventInstance closeElevatorAudio;

    [field: SerializeField] public bool IsElevatorOpen { get; private set; }

    private void Start()
    {
        openElevatorAudio = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.ElevatorOpen);
        closeElevatorAudio = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.ElevatorClose);
        CacheInitialPositions();
    }

    private void CacheInitialPositions()
    {
        doorSequence = DOTween.Sequence();
        if (leftDoor == null || rightDoor == null) return;

        leftDoorInitialPosition = leftDoor.localPosition;
        rightDoorInitialPosition = rightDoor.localPosition;

    }


    public void OpenDoor()
    {
        Vector3 leftTarget = CalculateTargetPosition(leftDoorInitialPosition, -doorSeparation, leftDoorOpenOffset);
        Vector3 rightTarget = CalculateTargetPosition(rightDoorInitialPosition, doorSeparation, rightDoorOpenOffset);
        if (!IsElevatorOpen)
            UpdateAudio(true);
        AnimateDoorSequence(leftTarget, rightTarget, openSpeed, openCurve);
        IsElevatorOpen = true;

    }

    public void CloseDoor()
    {
        if (IsElevatorOpen)
            UpdateAudio(false);
        AnimateDoorSequence(leftDoorInitialPosition, rightDoorInitialPosition, closeSpeed, closeCurve);
        IsElevatorOpen = false;
    }

    private Vector3 CalculateTargetPosition(Vector3 initialPosition, float separation, Vector3 vector3Offset)
    {
        switch (tweenAxis)
        {
            case Axis.X:
                return new Vector3(initialPosition.x + separation, initialPosition.y, initialPosition.z);
            case Axis.Y:
                return new Vector3(initialPosition.x, initialPosition.y + separation, initialPosition.z);
            case Axis.Z:
                return new Vector3(initialPosition.x, initialPosition.y, initialPosition.z + separation);
            case Axis.Vector3:
                return initialPosition + vector3Offset;
            default:
                return initialPosition;
        }
    }

    private void AnimateDoorSequence(Vector3 leftTarget, Vector3 rightTarget, float duration, AnimationCurve curve)
    {
        if (doorSequence != null)
            doorSequence.Kill();

        doorSequence.AppendInterval(delayBeforeMoving);

        // Animate left door
        Tweener leftTween = CreatePositionTween(leftDoor, leftTarget, duration);
        if (leftTween != null)
        {
            doorSequence.Append(leftTween.SetEase(curve));
        }

        // Animate right door
        Tweener rightTween = CreatePositionTween(rightDoor, rightTarget, duration);
        if (rightTween != null)
        {
            doorSequence.Join(rightTween.SetEase(curve));
        }
    }


    private Tweener CreatePositionTween(Transform target, Vector3 endPosition, float duration)
    {
        if (tweenAxis == Axis.Vector3)
        {
            return target.DOLocalMove(endPosition, duration, snapToTarget);
        }

        switch (tweenAxis)
        {
            case Axis.X:
                return target.DOLocalMoveX(endPosition.x, duration, snapToTarget);
            case Axis.Y:
                return target.DOLocalMoveY(endPosition.y, duration, snapToTarget);
            case Axis.Z:
                return target.DOLocalMoveZ(endPosition.z, duration, snapToTarget);
            default:
                return null;
        }
    }

    private void UpdateAudio(bool elevatorState)
    {
        PLAYBACK_STATE playbackStateOfOpen;
        PLAYBACK_STATE playbackStateOfClose;

        openElevatorAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        closeElevatorAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        openElevatorAudio.getPlaybackState(out playbackStateOfOpen);
        closeElevatorAudio.getPlaybackState(out playbackStateOfClose);

        // if door is opening
        if (elevatorState)
        {
            // check is closeing audio is play 
            if (playbackStateOfClose.Equals(PLAYBACK_STATE.PLAYING))
            {
                closeElevatorAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }

            if (playbackStateOfOpen.Equals(PLAYBACK_STATE.STOPPED))
            {
                openElevatorAudio.start();
            }
            return;
        }


        if (playbackStateOfOpen.Equals(PLAYBACK_STATE.PLAYING))
        {
            openElevatorAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (playbackStateOfClose.Equals(PLAYBACK_STATE.STOPPED))
        {
            closeElevatorAudio.start();
        }

    }

}