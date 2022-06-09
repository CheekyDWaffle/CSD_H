using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SlidersFunction : MonoBehaviour
{
    public AudioMixer audioMixer;
    public string volumeName;
    [SerializeField]
    private TMP_Text volumeLable;

    private void Start()
    {
        UpdateVolume(gameObject.GetComponent<Slider>().value);
    }
    public void UpdateVolume(float value)
    {
        audioMixer.SetFloat(volumeName, Mathf.Log(value) * 20);
        volumeLable.text = Mathf.Round(value * 100.0f).ToString() + "%";
    }
}
