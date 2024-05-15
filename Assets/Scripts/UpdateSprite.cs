using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    public Sprite CardFace;
    public Sprite CardBack;

    private SpriteRenderer spriteRenderer;
    private Selectable selectable;
    private GameMngr gameMngr;

    private UserInput userInput;

    // Start is called before the first frame update
    void Start()
    {
        List<string> deck = GameMngr.GenerateDeck();
        gameMngr = FindObjectOfType<GameMngr>();

        int i = 0;
        foreach(string Card in deck)
        {
            if(this.name == Card)
            {
                CardFace =gameMngr.CardFaces[i];
                break;
            }
            i++;
        }
        spriteRenderer =GetComponent<SpriteRenderer>();
        selectable=GetComponent<Selectable>();
        userInput=GetComponent<UserInput>();
    }

    // Update is called once per frame
    void Update()
    {

        if(selectable.FaceUp==true)
        {
            spriteRenderer.sprite = CardFace;
        }
        else
        {
            spriteRenderer.sprite=CardBack;
        }
    }
}
