using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;
    public static bool isReading = false; 

    [Header("A Tua UI")]
    public GameObject painelDoJornal; // O teu NoteReadingUI
    public Image imagemDaNota; // Onde a nota aparece
    public GameObject botaoEsquerda;
    public GameObject botaoDireita;

    [Header("O Teu Diário (Invisível)")]
    public List<Sprite> notasColecionadas = new List<Sprite>();
    private int paginaAtual = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        // SE ESTIVERES A LER:
        if (painelDoJornal.activeSelf)
        {
            // Fechar com ESC, F ou J
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.jKey.wasPressedThisFrame)
            {
                FecharJornal();
            }
            
            // Podes mudar de página com as setas do teclado ou com o A/D!
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                ProximaPagina();
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                PaginaAnterior();
        }
        // SE NÃO ESTIVERES A LER:
        else
        {
            // Abre o jornal com o J (Mas só se já tiveres apanhado alguma nota!)
            if (Keyboard.current.jKey.wasPressedThisFrame && notasColecionadas.Count > 0)
            {
                AbrirJornal(paginaAtual);
            }
        }
    }

    // A folha 3D no chão chama esta função!
    public void PickUpAndReadNote(Sprite novaNota)
    {
        // Se é uma nota nova, guarda no diário
        if (!notasColecionadas.Contains(novaNota))
        {
            notasColecionadas.Add(novaNota);
        }
        
        // Abre logo o ecrã na página da nota que acabaste de apanhar
        AbrirJornal(notasColecionadas.IndexOf(novaNota));
    }

    public void AbrirJornal(int numeroDaPagina)
    {
        if (notasColecionadas.Count == 0) return;

        paginaAtual = numeroDaPagina;
        AtualizarEcra();
        
        painelDoJornal.SetActive(true);
        CongelarJogo(true);
    }

    public void FecharJornal()
    {
        painelDoJornal.SetActive(false);
        CongelarJogo(false);
    }

    // Funções para as tuas setinhas!
    public void ProximaPagina()
    {
        if (paginaAtual < notasColecionadas.Count - 1)
        {
            paginaAtual++;
            AtualizarEcra();
        }
    }

    public void PaginaAnterior()
    {
        if (paginaAtual > 0)
        {
            paginaAtual--;
            AtualizarEcra();
        }
    }

    private void AtualizarEcra()
    {
        // Mostra a imagem correspondente à página atual
        imagemDaNota.sprite = notasColecionadas[paginaAtual];

        // Se estiveres na última página, esconde a seta da direita. Se estiveres na primeira, esconde a da esquerda!
        if (botaoDireita != null) botaoDireita.SetActive(paginaAtual < notasColecionadas.Count - 1);
        if (botaoEsquerda != null) botaoEsquerda.SetActive(paginaAtual > 0);
    }

    private void CongelarJogo(bool congelar)
    {
        isReading = congelar; 
        Time.timeScale = congelar ? 0f : 1f; 
        Cursor.visible = congelar;
        Cursor.lockState = congelar ? CursorLockMode.None : CursorLockMode.Locked;
    }
}