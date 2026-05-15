using UnityEngine;
using System.Collections;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animaisPermitidos;
    
    [Header("Configurações de Quantidade")]
    public int maxAnimaisNoMapa = 25; // Aumentámos de 15 para 40
    public int quantidadeInicial = 15; // Quantos animais aparecem mal dás Play
    public float intervaloDeSpawn = 3f; // Agora verifica de 3 em 3 segundos (era 15)
    
    [Header("Área de Spawn")]
    public float raioDeSpawn = 150f; // Aumentei o raio para cobrir mais ilha

    void Start()
    {
        // 1. POVOAR A ILHA IMEDIATAMENTE
        for (int i = 0; i < quantidadeInicial; i++)
        {
            GerarAnimalAleatorio();
        }

        // 2. COMEÇAR O CICLO DE REPOSIÇÃO MAIS RÁPIDO
        StartCoroutine(CicloDeSpawn());
    }

    IEnumerator CicloDeSpawn()
    {
        while (true)
        {
            GameObject[] animaisAtuais = GameObject.FindGameObjectsWithTag("Animal");
            
            // Se faltarem muitos animais, ele pode criar mais do que um de cada vez
            if (animaisAtuais.Length < maxAnimaisNoMapa)
            {
                GerarAnimalAleatorio();
            }
            
            yield return new WaitForSeconds(intervaloDeSpawn);
        }
    }

    void GerarAnimalAleatorio()
    {
        if (animaisPermitidos.Length == 0) return;

        int indexAleatorio = Random.Range(0, animaisPermitidos.Length);
        
        // Criar posição aleatória
        Vector2 circuloAleatorio = Random.insideUnitCircle * raioDeSpawn;
        Vector3 posicaoSpawn = new Vector3(transform.position.x + circuloAleatorio.x, 100f, transform.position.z + circuloAleatorio.y);
        
        RaycastHit hit;
        // O laser agora procura o chão com mais precisão
        if (Physics.Raycast(posicaoSpawn, Vector3.down, out hit, 200f))
        {
            // Garante que não faz spawn dentro de água se o teu chão estiver bem tagueado
            Instantiate(animaisPermitidos[indexAleatorio], hit.point, Quaternion.identity);
        }
    }
}