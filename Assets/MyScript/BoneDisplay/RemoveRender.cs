using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovdRender : MonoBehaviour
{
    public Animator animator; // アバターのAnimatorコンポーネント

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        // アバター内のすべてのRendererを無効化（メッシュ非表示）
        foreach (var renderer in animator.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        Debug.Log("All meshes are now hidden. Only bones are visible.");
    }
}

