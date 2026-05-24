using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;
    public static bool isReading = false; 

    [Header("A Tua UI")]
    public GameObject painelDoJornal; 
    public Image imagemDaNota; 
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

    // A folha 3D no chão chama esta função (1 Nota Simples)
    public void PickUpAndReadNote(Sprite novaNota)
    {
        if (!notasColecionadas.Contains(novaNota))
        {
            notasColecionadas.Add(novaNota);
        }
        
        AbrirJornal(notasColecionadas.IndexOf(novaNota));
    }

    // O Livro de Tutorial chama esta função (Múltiplas Notas)
    public void PickUpTutorial(Sprite[] paginasDoTutorial)
    {
        int indexDaPrimeiraPagina = notasColecionadas.Count;

        foreach (Sprite pagina in paginasDoTutorial)
        {
            if (!notasColecionadas.Contains(pagina))
            {
                notasColecionadas.Add(pagina);
            }
        }
        
        AbrirJornal(indexDaPrimeiraPagina);
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
        imagemDaNota.sprite = notasColecionadas[paginaAtual];

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

    // =================================================================
    // LIGAÇÃO AO CRAFTING (VERIFICAR DESBLOQUEIOS)
    // =================================================================
    public bool TemNota(Sprite notaProcurada)
    {
        // Se a receita não precisa de nota (está vazio no Inspector), deixamos construir logo!
        if (notaProcurada == null) return true;

        // Se precisa de nota, verifica se o sprite já está no nosso diário
        return notasColecionadas.Contains(notaProcurada);
    }
}