using UnityEngine;

public class SomDosPassos : MonoBehaviour
{
    [Header("Configuração de Áudio")]
    public AudioSource audioSource;
    public AudioClip somAndar;
    public AudioClip somCorrer;

    [Header("Ritmo dos Passos (Segundos)")]
    public float tempoAndar = 0.5f; 
    public float tempoCorrer = 0.3f; 

    private float timer;

    void Update()
    {
        bool estaAMover = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
        
        bool tentarCorrer = Input.GetKey(KeyCode.LeftShift);
        bool estaACorrer = tentarCorrer && SurvivalManager.instance != null && !SurvivalManager.instance.isExhausted;

        if (estaAMover)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                audioSource.clip = estaACorrer ? somCorrer : somAndar;
                
                // TRUQUE PROFISSIONAL: Altera o pitch entre 0.9 (mais grave) e 1.1 (mais agudo)
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                
                audioSource.Play();

                timer = estaACorrer ? tempoCorrer : tempoAndar;
            }
        }
        else
        {
            timer = 0f; 
        }
    }
}