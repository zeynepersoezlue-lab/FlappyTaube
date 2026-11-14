using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
  [SerializeField] private float scrollSpeed = 0.02f;

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

        offset.x += scrollSpeed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}
