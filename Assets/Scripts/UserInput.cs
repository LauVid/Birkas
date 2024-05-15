using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public GameObject Selected;
    private GameMngr gameMngr;

    private Selectable selectable;

    public List<GameObject> playedCardsList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        Selected=this.gameObject;
        gameMngr =FindObjectOfType<GameMngr>();
        selectable =FindObjectOfType<Selectable>();
        gameMngr.PlayersTurn=false;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseClick();
    }

    void GetMouseClick()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,-10));
            RaycastHit2D hit =Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
            if(hit)
            {
                if(hit.collider.CompareTag("Card"))
                {
                    if(gameMngr.PlayersTurn==true)
                    {
                        ClickedCard(hit.collider.gameObject);                        
                    }

                }
            }
        }
    }

    void ClickedCard(GameObject selected)
    {
        if (selected.transform.parent != null && selected.transform.parent.name.Contains("Player")&&Stackable(selected))
        {

            // Play the card if clicked by a player
            gameMngr.PlayCard(selected);
            gameMngr.PlayersTurn=false;

            if(gameMngr.Players[0].transform.childCount==0)
            {
                Debug.Log("No cards left Congrats you win!");
                gameMngr.WinPanel.SetActive(true);
            }

            StartCoroutine(gameMngr.PcTurn());



            // //OTHER PLAYERS PLAY    MOVE THIS TO THE PATTERN!!!!
            // for(int i=1;i<=3;i++)
            // {

            //     yield return new WaitForSeconds(2f);             
  
            //     GameObject Player = gameMngr.GetPlayer(i);

            //     //If player has any cards left
            //     if(Player.transform.childCount>0)
            //     {
            //         gameMngr.PlayAICard(Player,gameMngr.playLowestCardHandler);

            //         // //If has a card that is playable find the lowest one and play it
            //         // GameObject LowestCard = gameMngr.GetLowestPlayableCard(i);
            //         // if(LowestCard!=null)
            //         // {
            //         //     gameMngr.PlayCard(LowestCard);
            //         // }
            //         // else
            //         // {
            //         //     //gameMngr.Take3Cards(Player);
            //         //     gameMngr.TakeAllCards(Player);
            //         // }                   
            //     }
            //     if (i==3)
            //     {
            //         Debug.Log("Player's turn");
            //         gameMngr.PlayersTurn=true;
            //     }
            // }  
            

        }
    }

    public bool Stackable(GameObject selected)
    {
        Selectable SelectedCard= selected.GetComponent<Selectable>();
        Selectable LastPlayedCard= playedCardsList[playedCardsList.Count-1].GetComponent<Selectable>();

        if( SelectedCard.Strength>=LastPlayedCard.Strength)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}
