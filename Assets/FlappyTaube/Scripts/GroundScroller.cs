using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [SerializeField] private float speedFactor = 0.1f;

    private Material material;
    private Vector2 offset;

    void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
        if (GameManager.Instance.GameState == GameState.GameOver)
            return;

        offset.x += speedFactor * GameManager.Instance.PipeSpeed * Time.deltaTime;

        material.mainTextureOffset = offset;
    }
}
