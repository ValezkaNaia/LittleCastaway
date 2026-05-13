using UnityEngine;
using System.Collections;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animaisPermitidos; // Arrastas os 5 prefabs para aqui
    public int maxAnimaisNoMapa = 15;
    public float raioDeSpawn = 80f;

    void Start()
    {
        StartCoroutine(CicloDeSpawn());
    }

    IEnumerator CicloDeSpawn()
    {
        while (true)
        {
            // Conta quantos animais com a tag "Animal" existem
            GameObject[] animaisAtuais = GameObject.FindGameObjectsWithTag("Animal");
            
            if (animaisAtuais.Length < maxAnimaisNoMapa)
            {
                GerarAnimalAleatorio();
            }
            
            yield return new WaitForSeconds(15f); // Verifica de 15 em 15 segundos
        }
    }

    void GerarAnimalAleatorio()
    {
        // Escolhe um animal ao calhas (Chicken, Deer, Tiger, Dog, Kitty)
        int indexAleatorio = Random.Range(0, animaisPermitidos.Length);
        
        // Escolhe um ponto aleatório
        Vector3 pontoAleatorio = transform.position + Random.insideUnitSphere * raioDeSpawn;
        pontoAleatorio.y = 100f; // Começa alto para fazer um raycast para baixo
        
        RaycastHit hit;
        if (Physics.Raycast(pontoAleatorio, Vector3.down, out hit, 200f))
        {
            Instantiate(animaisPermitidos[indexAleatorio], hit.point, Quaternion.identity);
        }
    }
}