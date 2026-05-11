using UnityEngine;

public class EfeitosSonorosUI : MonoBehaviour
{
    public AudioSource fonteDeSom;
    public AudioClip somDoClique;

    // Esta é a função que os botões vão chamar
    public void TocarClique()
    {
        if (fonteDeSom != null && somDoClique != null)
        {
            // PlayOneShot permite tocar o som várias vezes por cima dele mesmo
            // sem cortar o clique anterior, caso o jogador clique muito rápido!
            fonteDeSom.PlayOneShot(somDoClique);
        }
    }
}