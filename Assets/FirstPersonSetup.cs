using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonSetup : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        // VRMFirstPerson.Setup()を削除
        // 必要に応じて独自の設定を記述
        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.updateWhenOffscreen = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
