using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using TMPEffects.Components;
using System.Linq;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private string keyHint = "[E]";

    private string displayLine;

    [Header("TypeWriter Settings")]
    [SerializeField] private TMPWriter typeWriter;

    [Header("TextBoxDisplay")]
    [SerializeField] private RectTransform textBoxDisplayRect;
    [SerializeField] private float displaySpeed = 0.5f;
    [SerializeField] private AnimationCurve displaySize = AnimationCurve.EaseInOut(0, 0, 1, 1); // depending on text size stretch text box
    private Vector2 _initialSize;
    private void Awake()
    {
        StopAllCoroutines();
        if (textBoxDisplayRect == null)
            textBoxDisplayRect = GameObject.Find("TextBoxDisplay").GetComponent<RectTransform>();


        _initialSize = textBoxDisplayRect.sizeDelta;
        textBoxDisplayRect.sizeDelta = new Vector2(0, _initialSize.y);
        textBoxDisplayRect.gameObject.SetActive(false);


        if (label == null)
            label = textBoxDisplayRect.GetComponentInChildren<TMP_Text>();
        label.text = "";
    }
    public void Hide()
    {
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.OnStart(() =>
        {
            label.text = "";
        });
        hideSequence.Append(
            DOTween.To(() => textBoxDisplayRect.sizeDelta, x => textBoxDisplayRect.sizeDelta = x, new Vector2(0, _initialSize.y), displaySpeed).SetEase(displaySize)
        );

        hideSequence.OnComplete(() => { textBoxDisplayRect.gameObject.SetActive(false); });

    }
    public void Show(IInteractable interactable)
    {
        if (interactable == null)
        {
            Hide();
            return;
        }
        displaySpeed = interactable.OutlineDuration;

        Sequence showSequence = DOTween.Sequence();
        showSequence.OnStart(() =>
        {
            textBoxDisplayRect.gameObject.SetActive(true);
        });

        showSequence.Append(
            DOTween.To(() => textBoxDisplayRect.sizeDelta, x => textBoxDisplayRect.sizeDelta = x, _initialSize, displaySpeed).SetEase(displaySize)
        );

        showSequence.AppendInterval(.1f);

        showSequence.AppendCallback(() =>
        {
            label.text = $"{keyHint} {interactable.DisplayName}";
        });

    }

}