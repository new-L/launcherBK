using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimatiopn : MonoBehaviour
{
    public Text currentVersion;
    public void OnUpdate()
    {
        InvokeRepeating("TextCounter", 0f, .05f);
    }
    public void TextCounter()
    {
        int first = Random.Range(0, 10);
        int middle = Random.Range(0, 10);
        int last = Random.Range(0, 10);
        currentVersion.text = $"v{first.ToString()}.{middle.ToString()}.{last.ToString()}";
    }
    public void SetCurrentVersion(string newVersion)
    {
        currentVersion.text = newVersion;
        CancelInvoke();
    }
}
