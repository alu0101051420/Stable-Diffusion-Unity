using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackDropScroll : MonoBehaviour
{
    SpriteRenderer renderer;
    [SerializeField] public float speed = 1;
    float offset = 0;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        offset += Time.deltaTime * speed;
        renderer.material.mainTextureOffset = new Vector2(offset, 0);
    }
}
