using UnityEngine;

public class ForcarVisao : MonoBehaviour
{
    public float distanciaVisao = 5000f;
    private Camera cam;

    void Start() { cam = GetComponent<Camera>(); }

    void LateUpdate() // O LateUpdate corre DEPOIS dos outros scripts
    {
        if (cam != null) cam.farClipPlane = distanciaVisao;
    }
}