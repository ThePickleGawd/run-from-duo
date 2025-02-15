using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(OutlineOnHover))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class Ammo : MonoBehaviour
{
    [HideInInspector] public int ammo = 0;
    public int maxAmmo = 15;

    // Components
    private XRGrabInteractable interactable;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();

        interactable.selectExited.AddListener(OnSelectExit);

        ResetAmmo();
    }

    public bool IsEmpty() => ammo <= 0;

    public void OnSelectExit(SelectExitEventArgs args)
    {
        if (ammo <= 0)
        {
            Destroy(gameObject, 1f);
            interactable.enabled = false;
        }
    }

    public void ConsumeAmmo()
    {
        ammo--;
    }

    public void ResetAmmo()
    {
        ammo = maxAmmo;
    }
}
