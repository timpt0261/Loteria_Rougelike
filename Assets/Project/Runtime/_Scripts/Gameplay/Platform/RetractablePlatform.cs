using UnityEngine;
using DG.Tweening;

public class RetractablePlatform : MonoBehaviour
{
    [field: Header("Door References")]
    [field: SerializeField] private Transform leftDoor;
    [field: SerializeField] private Transform rightDoor;

    private enum Axis { X, Y, Z, Vector3 }
    private enum TweenType { Position, Rotation, Scale }

    [field: Header("3D Orientation")]
    [field: SerializeField] private Axis tweenAxis = Axis.X;
    [field: SerializeField] private TweenType tweenType = TweenType.Position;

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

    private Vector3 leftDoorInitialValue;
    private Vector3 rightDoorInitialValue;

    private void Start()
    {
        CacheInitialValues();
    }

    private void CacheInitialValues()
    {
        if (leftDoor == null || rightDoor == null) return;

        leftDoorInitialValue = GetTransformValue(leftDoor, tweenType);
        rightDoorInitialValue = GetTransformValue(rightDoor, tweenType);
    }

    public void OpenDoor()
    {
        Vector3 leftTarget = CalculateTargetValue(leftDoorInitialValue, -doorSeparation, leftDoorOpenOffset);
        Vector3 rightTarget = CalculateTargetValue(rightDoorInitialValue, doorSeparation, rightDoorOpenOffset);

        AnimateDoors(leftTarget, rightTarget, openSpeed, openCurve);
    }

    public void CloseDoor()
    {
        AnimateDoors(leftDoorInitialValue, rightDoorInitialValue, closeSpeed, closeCurve);
    }

    private Vector3 CalculateTargetValue(Vector3 initialValue, float separation, Vector3 vector3Offset)
    {
        switch (tweenAxis)
        {
            case Axis.X:
                return new Vector3(initialValue.x + separation, initialValue.y, initialValue.z);
            case Axis.Y:
                return new Vector3(initialValue.x, initialValue.y + separation, initialValue.z);
            case Axis.Z:
                return new Vector3(initialValue.x, initialValue.y, initialValue.z + separation);
            case Axis.Vector3:
                return initialValue + vector3Offset;
            default:
                return initialValue;
        }
    }

    private void AnimateDoors(Vector3 leftTarget, Vector3 rightTarget, float duration, AnimationCurve curve)
    {
        Sequence doorSequence = DOTween.Sequence();
        doorSequence.AppendInterval(delayBeforeMoving);

        // Animate left door
        Tweener leftTween = CreateTween(leftDoor, leftTarget, duration, tweenType);
        if (leftTween != null)
        {
            doorSequence.Append(leftTween.SetEase(curve));
        }

        // Animate right door
        Tweener rightTween = CreateTween(rightDoor, rightTarget, duration, tweenType);
        if (rightTween != null)
        {
            doorSequence.Join(rightTween.SetEase(curve));
        }
    }

    private Tweener CreateTween(Transform target, Vector3 endValue, float duration, TweenType type)
    {
        switch (type)
        {
            case TweenType.Position:
                if (tweenAxis == Axis.Vector3)
                    return target.DOLocalMove(endValue, duration, snapToTarget);
                else
                    return CreateAxisTween(target, endValue, duration,
                        (t, v, d, s) => t.DOLocalMoveX(v, d, s),
                        (t, v, d, s) => t.DOLocalMoveY(v, d, s),
                        (t, v, d, s) => t.DOLocalMoveZ(v, d, s));

            case TweenType.Rotation:
                if (tweenAxis == Axis.Vector3)
                    return target.DOLocalRotate(endValue, duration);
                else
                    return CreateAxisTween(target, endValue, duration,
                        (t, v, d, s) => t.DOLocalRotate(new Vector3(v, 0, 0), d),
                        (t, v, d, s) => t.DOLocalRotate(new Vector3(0, v, 0), d),
                        (t, v, d, s) => t.DOLocalRotate(new Vector3(0, 0, v), d));

            case TweenType.Scale:
                if (tweenAxis == Axis.Vector3)
                    return target.DOScale(endValue, duration);
                else
                    return CreateAxisTween(target, endValue, duration,
                        (t, v, d, s) => target.DOScaleX(v, d),
                        (t, v, d, s) => target.DOScaleY(v, d),
                        (t, v, d, s) => target.DOScaleZ(v, d));

            default:
                return null;
        }
    }

    private Tweener CreateAxisTween(Transform target, Vector3 endValue, float duration,
        System.Func<Transform, float, float, bool, Tweener> xTween,
        System.Func<Transform, float, float, bool, Tweener> yTween,
        System.Func<Transform, float, float, bool, Tweener> zTween)
    {
        switch (tweenAxis)
        {
            case Axis.X:
                return xTween(target, endValue.x, duration, snapToTarget);
            case Axis.Y:
                return yTween(target, endValue.y, duration, snapToTarget);
            case Axis.Z:
                return zTween(target, endValue.z, duration, snapToTarget);
            default:
                return null;
        }
    }

    private Vector3 GetTransformValue(Transform target, TweenType type)
    {
        switch (type)
        {
            case TweenType.Position:
                return target.localPosition;
            case TweenType.Rotation:
                return target.localEulerAngles;
            case TweenType.Scale:
                return target.localScale;
            default:
                return Vector3.zero;
        }
    }
}