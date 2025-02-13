using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Outline))]
public class OutlineOnHover : MonoBehaviour
{
  private XRGrabInteractable interactable;
  private Outline outline;

  private void Awake()
  {
    interactable = GetComponent<XRGrabInteractable>();
    outline = GetComponent<Outline>();

    interactable.hoverEntered.AddListener(HoverEnterWeapon);
    interactable.hoverExited.AddListener(HoverExitWeapon);
  }

  private void HoverEnterWeapon(HoverEnterEventArgs args)
  {
    outline.OutlineWidth = 2f;
  }

  private void HoverExitWeapon(HoverExitEventArgs args)
  {
    outline.OutlineWidth = 0;
  }
}
