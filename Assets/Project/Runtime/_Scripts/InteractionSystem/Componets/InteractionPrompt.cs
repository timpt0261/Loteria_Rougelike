using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private string keyHint = "[E]";
    [SerializeField] private float textSpeed;

    [Header("TextBoxDisplay")]
    [SerializeField] private RectTransform textBoxDisplayRect;
    [SerializeField] private float displaySpeed = 0.5f;
    [SerializeField] private AnimationCurve displaySize = AnimationCurve.EaseInOut(0, 0, 1, 1); // depending on text size stretch text box
    private Vector2 initialSize;

    private void Awake()
    {
        if (textBoxDisplayRect == null)
            textBoxDisplayRect = GameObject.Find("TextBoxDisplay").GetComponent<RectTransform>();


        initialSize = textBoxDisplayRect.sizeDelta;
        textBoxDisplayRect.sizeDelta = new Vector2(0, initialSize.y);
        textBoxDisplayRect.gameObject.SetActive(false);


        if (label == null)
            label = textBoxDisplayRect.GetComponentInChildren<TMP_Text>();
        label.text = "";


        // Hide();
    }
    public void Hide()
    {
        label.text = "";
        if (textBoxDisplayRect.sizeDelta.x > 0)
        {
            DOTween.To(() => textBoxDisplayRect.sizeDelta, x => textBoxDisplayRect.sizeDelta = x, new Vector2(0, initialSize.y), displaySpeed).SetEase(displaySize)
                            .OnComplete(() => { textBoxDisplayRect.gameObject.SetActive(false); });
        }

    }
    public void Show(IInteractable interactable)
    {
        if (interactable == null)
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
            DOTween.To(() => textBoxDisplayRect.sizeDelta, x => textBoxDisplayRect.sizeDelta = x, initialSize, displaySpeed).SetEase(displaySize)
        );

        showSequence.JoinCallback(() =>
        {

            label.text = $"{keyHint} {interactable.DisplayName}";
        });


    }

    IEnumerable DisplayText(float duration)
    {
        yield return new WaitForSeconds(duration);
    }
}