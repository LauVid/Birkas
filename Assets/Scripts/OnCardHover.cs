using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCardHover : MonoBehaviour
{
    public float popOutDistance = 5f;

    public Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    void OnMouseOver()
    {
        if(transform.CompareTag("Card"))
        {
            if(transform.parent != null && transform.parent.name == "Player")
            {
                transform.position = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z - popOutDistance);
            }            
        }
    }

    void OnMouseExit()
    {
        if(transform.CompareTag("Card"))
        {
            if(transform.parent != null && transform.parent.name == "Player")
            {
                transform.position = originalPosition;
            }            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
