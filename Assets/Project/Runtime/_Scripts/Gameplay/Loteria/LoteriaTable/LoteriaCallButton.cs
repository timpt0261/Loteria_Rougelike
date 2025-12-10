using System;
using DG;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoteriaCallButton : MonoBehaviour
{
    // hello
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeColor;
    [SerializeField] private GameObject cubeButton;
    [SerializeField] private Renderer cubeRender;
    [SerializeField] private Button loteriaButton;

    private void Awake()
    {

        loteriaButton.interactable = false;
        cubeRender = this.cubeButton.GetComponent<Renderer>();

        loteriaButton.onClick.AddListener(RoundEnd);

    }

    private void OnEnable()
    {
        EventBus.Subscribe<LoteiaCallEvent>(OnLoteriaCall);

    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<LoteiaCallEvent>(OnLoteriaCall);
    }

    private void OnLoteriaCall(LoteiaCallEvent @event)
    {
        Sequence activate = DOTween.Sequence();
        activate.Append(cubeRender.material.DOBlendableColor(activeColor, duration: duration));
        activate.OnComplete(() =>
        {
            loteriaButton.interactable = true;
        });

    }

    public void RoundEnd()
    {
        EventBus.Raise(new RoundEndEvent(winState: true));
    }



}
