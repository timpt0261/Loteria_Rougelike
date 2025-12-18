using System;
using DG.Tweening;
using PlasticGui.WorkspaceWindow;
using UnityEngine;

public interface IInteractable
{
    Transform transform { get; }
    string DisplayName { get; }

    Ease OutlineScaleEase { get; }

    Color OutlineColor { get; }
    float OutlineScale { get; }
    float OutlineDuration { get; }

    bool CanInteract();
    void Interact(GameObject interactor);
    void OnFocusGained();
    void OnFocusLost();

}
