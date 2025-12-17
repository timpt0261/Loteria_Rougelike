using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class InteractionPrompt : MonoBehaviour
{
    [field: Header("Text")]
    [field: SerializeField] private TMP_Text label;
    [field: SerializeField] private string keyHint = "[E]";

    private string displayLine;


    [field: Header("TypeWriter Settings")]
    [field: SerializeField] private float textSpeed = 0.5f;

    [field: Header("TextBoxDisplay")]
    [field: SerializeField] private RectTransform textBoxDisplayRect;
    [field: SerializeField] private float displaySpeed = 0.5f;
    [field: SerializeField] private AnimationCurve displaySize = AnimationCurve.EaseInOut(0, 0, 1, 1); // depending on text size stretch text box
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
        if (interactable == null || interactable.DisplayName == "")
        {
            Hide();
            return;
        }
        float size =
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
            displayLine = $"{keyHint} {interactable.DisplayName}";
            StartDisplayText();
        });


    }

    private void StartDisplayText()
    {
        label.text = "";
        StartCoroutine(DisplayText());
    }
    IEnumerator DisplayText()
    {
        foreach (char c in displayLine.ToCharArray())
        {
            label.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}