using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(OutlineOnHover))]
public class AmmoCrate : MonoBehaviour
{
    private Health health;
    private XRGrabInteractable interactable;
    private Rigidbody rb;

    private int ammoAmmount = 0;
    private bool shouldDestroyOnCollision = false;
    private bool createdQuiz = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        health.OnDeath.AddListener(async () => await CreatePopupQuiz());

        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectExited.AddListener(args => OnDropped());
    }

    public async Task CreatePopupQuiz()
    {
        if (createdQuiz) return;

        createdQuiz = true;

        PopupQuiz popupQuiz = await GameManager.instance.CreatePopupQuiz(transform);
        popupQuiz.OnCorrectAnswer.AddListener(OnCorrectAnswer);
        popupQuiz.OnWrongAnswer.AddListener(OnWrongAnswer);
    }

    private void OnCorrectAnswer()
    {
        Debug.Log("CORRECT!");
        Destroy(gameObject);
    }

    private void OnWrongAnswer()
    {
        Debug.Log("WRONG");
        Destroy(gameObject);
    }

    private void OnDropped()
    {
        shouldDestroyOnCollision = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (shouldDestroyOnCollision && !createdQuiz)
            _ = CreatePopupQuiz();
    }
}
