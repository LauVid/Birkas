using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource soundEffect;
    public AudioClip Card1;
    public AudioClip Card2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator PlaySound(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        soundEffect.PlayOneShot(clip); //does not cancel clips that are already being played 
    }
}
