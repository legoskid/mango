using UnityEngine;

namespace ModMenuPatch.HarmonyPatches;

internal class BtnCollider : MonoBehaviour
{
	public string relatedText;

	private void OnTriggerEnter(Collider collider)
	{
		if (Time.frameCount >= MenuPatch.framePressCooldown + 30)
		{
			MenuPatch.Toggle(relatedText);
			MenuPatch.framePressCooldown = Time.frameCount;
		}
	}
}
