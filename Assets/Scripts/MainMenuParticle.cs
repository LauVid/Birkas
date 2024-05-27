using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuParticle : MonoBehaviour
{
    public List<Material> materials;
    public ParticleSystem particleSystem1;
    public ParticleSystem particleSystem2;
    public ParticleSystem particleSystem3;
    public ParticleSystem particleSystem4;

    private Renderer renderer1;
    private Renderer renderer2;
    private Renderer renderer3;
    private Renderer renderer4; 

    // Start is called before the first frame update
    void Start()
    {
        renderer1=particleSystem1.GetComponent<Renderer>();
        renderer2=particleSystem2.GetComponent<Renderer>();
        renderer3=particleSystem3.GetComponent<Renderer>();
        renderer4=particleSystem4.GetComponent<Renderer>();
        int rand=Random.Range(0,361);
        Quaternion rotate= Quaternion.Euler(0, 0,rand);
        transform.rotation=transform.rotation*rotate;
        StartCoroutine(RotateAndChangeSprite());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RotateAndChangeSprite()
    {
        int rand=Random.Range(0,361);
        Quaternion rotate= Quaternion.Euler(0, 0,rand);
        yield return new WaitForSeconds(4.9f);
        transform.rotation=transform.rotation*rotate;
        rand=Random.Range(0,materials.Count);
        renderer1.material=materials[rand];
        rand=Random.Range(0,materials.Count);
        renderer2.material=materials[rand];
        rand=Random.Range(0,materials.Count);
        renderer3.material=materials[rand];
        rand=Random.Range(0,materials.Count);
        renderer4.material=materials[rand];
        StartCoroutine(RotateAndChangeSprite());
    }
}
