using UnityEngine;

public class HandBobbing : MonoBehaviour
{
    [Header("Configurações do Balanço")]
    public float velocidadeCaminhar = 14f;
    public float quantidadeBalançoY = 0.05f; // Movimento vertical (cima/baixo)
    public float quantidadeBalançoX = 0.02f; // Movimento horizontal (esquerda/direita)

    [Header("Suavidade")]
    public float suavidadeRetorno = 6f; 

    private Vector3 posicaoOriginal;
    private float temporizadorSimulado = 0f;
    private CharacterController characterController;

    void Start()
    {
        posicaoOriginal = transform.localPosition;

        // Procura no topo do objeto mais pai de todos (o PlayerCapsule)
        characterController = GetComponentInParent<CharacterController>();

        // SE CONTINUAR NULO, faz uma busca forçada pelo componente na cena inteira:
        if (characterController == null)
        {
            characterController = Object.FindFirstObjectByType<CharacterController>();
        }
    }

    void Update()
    {
        if (characterController == null) return;

        // Calcula a velocidade horizontal real do jogador
        Vector3 velocidadeHorizontal = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        float movimentoAtual = velocidadeHorizontal.magnitude;

        // LINHA DE TESTE: Mostra no Console se o Unity sabe que te estás a mexer
        if(movimentoAtual > 0) Debug.Log($"Velocidade detetada pelo Bobbing: {movimentoAtual}");

        // Se o jogador se mover e estiver no chão, faz o bobbing
        if (movimentoAtual > 0.1f && characterController.isGrounded)
        {
            temporizadorSimulado += Time.deltaTime * velocidadeCaminhar;

            float novoY = Mathf.Sin(temporizadorSimulado) * quantidadeBalançoY;
            float novoX = Mathf.Cos(temporizadorSimulado / 2) * quantidadeBalançoX;

            Vector3 posicaoAlvo = posicaoOriginal + new Vector3(novoX, novoY, 0);
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, posicaoAlvo, Time.deltaTime * velocidadeCaminhar);
        }
        else
        {
            // Se parar, o HandTransform volta suavemente para a posição de descanso original
            temporizadorSimulado = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, posicaoOriginal, Time.deltaTime * suavidadeRetorno);
        }
    }
}