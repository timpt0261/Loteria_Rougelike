using UnityEngine;
using FMODUnity;
using DG.Tweening;
public class Cube : MonoBehaviour
{
	[field: SerializeField] private Rigidbody rigidbody;
	[field: SerializeField] private EventReference interactionSFX;


	void Start()
	{
		if (rigidbody == null) { rigidbody = GetComponent<Rigidbody>(); }
	}


	public void OnInteract()
	{
		AudioManager.Instance.PlayOneShot(interactionSFX, transform.position);
		rigidbody.DOMoveY(2.5f, .75f, true).SetEase(Ease.InOutElastic);
	}
}
