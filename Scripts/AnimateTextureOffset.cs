using UnityEngine;

public class AnimateTextureOffset : MonoBehaviour
{
    public int materialIndex = 0;
    public float speed = 0.5f;
    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * speed;
        foreach (var rend in renderers)
        {
            rend.materials[materialIndex].SetTextureOffset("_MainTex", new Vector2(offset, 0));
        }
    }
}