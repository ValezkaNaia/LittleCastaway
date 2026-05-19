using UnityEngine;
using System.Collections;

public class FerramentaAtaque : MonoBehaviour
{
    public bool ehMachado;
    public bool ehLanca;
    public Transform modeloVisual;
    private Vector3 posicaoOriginal;
    private Quaternion rotacaoOriginal;
    private bool estaAnimando = false;

    void Start()
    {
        // Se te esqueceres de arrastar no Inspector, o script assume o próprio objeto!
        if (modeloVisual == null) 
        {
            modeloVisual = this.transform;
        }

        posicaoOriginal = modeloVisual.localPosition;
        rotacaoOriginal = modeloVisual.localRotation;
    }

    public void JogarAnimacaoGatilho()
    {
        if (!estaAnimando) StartCoroutine(ExecutarGolpe());
    }

    IEnumerator ExecutarGolpe()
    {
        estaAnimando = true;
        float tempo = 0f;
        while (tempo < 0.08f)
        {
            tempo += Time.deltaTime;
            if (modeloVisual != null)
            {
                modeloVisual.localRotation = Quaternion.Lerp(rotacaoOriginal, rotacaoOriginal * Quaternion.Euler(50, 0, 0), tempo / 0.08f);
                modeloVisual.localPosition = Vector3.Lerp(posicaoOriginal, posicaoOriginal + new Vector3(0, 0, 0.2f), tempo / 0.08f);
            }
            yield return null;
        }
        tempo = 0f;
        while (tempo < 0.12f)
        {
            tempo += Time.deltaTime;
            if (modeloVisual != null)
            {
                modeloVisual.localRotation = Quaternion.Lerp(modeloVisual.localRotation, rotacaoOriginal, tempo / 0.12f);
                modeloVisual.localPosition = Vector3.Lerp(modeloVisual.localPosition, posicaoOriginal, tempo / 0.12f);
            }
            yield return null;
        }
        estaAnimando = false;
    }
}