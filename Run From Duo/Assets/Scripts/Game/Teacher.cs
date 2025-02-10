using UnityEngine;

public class Teacher : MonoBehaviour
{
    private WebSocketAudioClient wsAudioClient;
    private AudioSource audioSource;
    private Animator anim;

    private void Awake()
    {
        wsAudioClient = GetComponent<WebSocketAudioClient>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckIfTalking();
    }

    private void CheckIfTalking()
    {
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += Mathf.Abs(samples[i]);

        // Set a threshold below which you consider it silent.
        bool isPlayingAudio = sum > 0.01f;
        anim.SetBool("isTalking", isPlayingAudio);
    }

    public void StartTalkToTeacher()
    {
        wsAudioClient.StartSpeech();

        // TODO: SetBool(isListening, true)
    }

    public void StopTalkToTeacher()
    {
        wsAudioClient.StopSpeech();

        // TODO: SetBool(isListening, true)
    }
}
