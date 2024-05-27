using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public GameObject Selected;
    private GameMngr gameMngr;
    private ButtonMngr buttonMngr;

    private Selectable selectable;
    public bool CardHasBeenSelected=false;

    public List<GameObject> SameStrengthCards=new();
    private List<GameObject> AnotherMultipleOf4=new();
    public bool HasMultipleGroupsOf4=false;

    public List<GameObject> playedCardsList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        Selected=this.gameObject;
        gameMngr =FindObjectOfType<GameMngr>();
        selectable =FindObjectOfType<Selectable>();
        buttonMngr=GameObject.Find("UI").GetComponent<ButtonMngr>();
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseClick();
        if(Input.GetKeyDown(KeyCode.Escape)&&!gameMngr.GameEnded)
        {
            if(buttonMngr.QuitScreen.activeSelf==false)
            {
                buttonMngr.QuitScreen.SetActive(true);
            }
            else
            {
                buttonMngr.QuitScreen.SetActive(false);
            }
        }
    }

    void GetMouseClick()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit =Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
            if(hit)
            {
                if(hit.collider.CompareTag("Card"))
                {
                    if(CardHasBeenSelected)
                    {
                        return;                
                    }
                    if(gameMngr.PlayersTurn==true&&!HasMultipleGroupsOf4)
                    {
                        ClickedCard(hit.collider.gameObject);                        
                    }

                }

                else if(hit.collider.CompareTag("SelectedCard"))
                {
                    if(CardHasBeenSelected&&SameStrengthCards.First().GetComponent<Selectable>().Strength!=1)
                    {
                        ControlsTextSetActive(false);
                        SetTagOfCards(SameStrengthCards,"Card");
                        AnotherMultipleOf4=gameMngr.Find4OfSame(gameMngr.Players[0],SameStrengthCards[0].GetComponent<Selectable>().Strength);
                        if(AnotherMultipleOf4.Count==4)
                        {
                            gameMngr.PlayMultipleCardsQuickReorganize(SameStrengthCards);
                        }
                        else
                        {
                            gameMngr.PlayMultipleCards(SameStrengthCards);                            
                        }
                        DeselectCard(SameStrengthCards);
                        if(gameMngr.Players[0].transform.childCount==0)
                        {
                            gameMngr.GameEnded = true;
                            gameMngr.ComputerTurnDelaySeconds=0.5f;
                            gameMngr.ManageCurrentWinStreak(true);
                            gameMngr.Players[0].GetComponent<ParticleManager>().SpawnWinParticles();
                            Debug.Log("No cards left Congrats you win!");
                            gameMngr.WinPanel.SetActive(true);
                        }
                        SameStrengthCards=gameMngr.Find4OfSame(gameMngr.Players[0],-1);
                        if(SameStrengthCards.Count==4)
                        {
                            HasMultipleGroupsOf4=true;
                            CardHasBeenSelected=true;
                            ButtonsSetActive(false);
                            foreach(GameObject Card in SameStrengthCards)
                            {  
                                Card.tag="SelectedCard";
                                Card.transform.GetComponent<SpriteRenderer>().color=Color.yellow;
                            }
                            buttonMngr.ControlsBG.SetActive(true);
                            buttonMngr.ControlsToPlayMultipleMultiples.SetActive(true);
                            return;     
                        }
                        else
                        {
                            HasMultipleGroupsOf4=false;
                        }
                        gameMngr.PlayersTurn=false;
                        StartCoroutine(gameMngr.PcTurn());
                        if(SameStrengthCards.Count>0)
                        {
                            SameStrengthCards.Clear();                            
                        }

                        return;
                    }
                    else if(CardHasBeenSelected)
                    {
                        ControlsTextSetActive(false);
                        SetTagOfCards(SameStrengthCards,"Card");
                        gameMngr.Play3Nines(SameStrengthCards);
                        DeselectCard(SameStrengthCards);

                        if(gameMngr.Players[0].transform.childCount==0)
                        {
                            gameMngr.GameEnded = true;
                            gameMngr.ComputerTurnDelaySeconds=0.5f;
                            gameMngr.ManageCurrentWinStreak(true);
                            gameMngr.Players[0].GetComponent<ParticleManager>().SpawnWinParticles();
                            Debug.Log("No cards left Congrats you win!");
                            gameMngr.WinPanel.SetActive(true);
                        }
                        gameMngr.PlayersTurn=false;
                        StartCoroutine(gameMngr.PcTurn());
                        SameStrengthCards.Clear();
                        return;
                    }
                }
                else if(CardHasBeenSelected)
                {
                    if(SameStrengthCards!=null)
                    {
                        DeselectCard(SameStrengthCards);                        
                    }
                }
            }
        }
        if(Input.GetMouseButtonDown(1))
        {
            if(CardHasBeenSelected)
            {
                if(gameMngr.PlayersTurn==true&&!HasMultipleGroupsOf4&&Stackable(SameStrengthCards.First()))
                {
                    gameMngr.PlayCard(SameStrengthCards.First());
                    gameMngr.PlayersTurn=false;
                    DeselectCard(SameStrengthCards);
                    ControlsTextSetActive(false);
                    gameMngr.ColorCardsByPlayable();
                    StartCoroutine(gameMngr.PcTurn());                    
                }
            }
        }
        if(Input.GetMouseButtonDown(2)&&CardHasBeenSelected)
        {
            ButtonsSetActive(true);
            DeselectCard(SameStrengthCards);

            if(HasMultipleGroupsOf4)
            {
                gameMngr.PlayersTurn=false;
                StartCoroutine(gameMngr.PcTurn());
                HasMultipleGroupsOf4=false;
            }
        }
    }
    IEnumerator MarkCardAfterDelay(GameObject Card, float delay)
    {
        
        yield return new WaitForSeconds(delay);
        Card.transform.GetComponent<SpriteRenderer>().color=Color.yellow;
    }

    IEnumerator PlayMultipleAfterDelay(List<GameObject> cards,float delay)
    {
        yield return new WaitForSeconds(delay);
        gameMngr.PlayMultipleCards(cards);
    }
    void SetTagOfCards(List<GameObject> Cards, string Tag)
    {
        foreach (GameObject Card in Cards)
        {
            Card.tag=Tag;
        }
    }
    void DeselectCard(List<GameObject> cards)
    {
        foreach(GameObject Card in cards)
        {
            Card.tag="Card";
            Card.transform.GetComponent<SpriteRenderer>().color=Color.white;
        }
        ControlsTextSetActive(false);
        CardHasBeenSelected=false;
    }

    public void ControlsTextSetActive(bool b)
    {
        buttonMngr.ControlsBG.SetActive(b);
        buttonMngr.ControlsToPlayMultiple.SetActive(b);
        buttonMngr.ControlsToPlayPakison.SetActive(b);
        buttonMngr.ControlsToPlayMultipleMultiples.SetActive(b);
    }

    public void ButtonsSetActive(bool b)
    {
        buttonMngr.Take3cards.gameObject.SetActive(b);
        buttonMngr.TakeAllcards.gameObject.SetActive(b);
    }

    void ClickedCard(GameObject selected)
    {
        GameObject Player=selected.transform.parent.gameObject;

        if(selected.GetComponent<Selectable>().Strength==1)
        {
            if(SameStrengthCards.Count>0)
            {
                SameStrengthCards.Clear();                
            }

            int SameStrengthCount=0;
            for(int i=0;i<Player.transform.childCount;i++)
            {
                if(Player.transform.GetChild(i).GetComponent<Selectable>().Strength==1)
                {
                    SameStrengthCards.Add(selected.transform.parent.GetChild(i).gameObject);
                    SameStrengthCount++;
                }
            }
            if(SameStrengthCount==3)
            {
                CardHasBeenSelected=true;
                ButtonsSetActive(false);
                foreach(GameObject Card in SameStrengthCards)
                {  
                    Card.tag="SelectedCard";
                    Card.transform.GetComponent<SpriteRenderer>().color=Color.yellow;
                    buttonMngr.ControlsBG.SetActive(true);
                    buttonMngr.ControlsToPlayPakison.SetActive(true);
                }
                return;
            }
        }
        if (Player != null && Player.name.Contains("Player")&&Stackable(selected))
        {
            if(!CardHasBeenSelected)
            {
                if(SameStrengthCards.Count>0)
                {
                    SameStrengthCards.Clear();                
                }

                int SameStrengthCount=0;
                for(int i=0;i<Player.transform.childCount;i++)
                {
                    if(Player.transform.GetChild(i).GetComponent<Selectable>().Strength==selected.GetComponent<Selectable>().Strength)
                    {
                        SameStrengthCards.Add(selected.transform.parent.GetChild(i).gameObject);
                        SameStrengthCount++;
                    }
                }

                if(SameStrengthCount==4)
                {
                    CardHasBeenSelected=true;
                    ButtonsSetActive(false);
                    foreach(GameObject Card in SameStrengthCards)
                    {  
                        Card.tag="SelectedCard";
                        Card.transform.GetComponent<SpriteRenderer>().color=Color.yellow;
                    }
                    buttonMngr.ControlsBG.SetActive(true);
                    buttonMngr.ControlsToPlayMultiple.SetActive(true);
                    return;
                }            
            }

            
                // Play the card if clicked by a player
                gameMngr.PlayCard(selected);
                
                gameMngr.PlayersTurn=false;

                if(gameMngr.Players[0].transform.childCount==0)
                {
                    gameMngr.GameEnded=true;
                    gameMngr.ComputerTurnDelaySeconds=0.5f;
                    gameMngr.ManageCurrentWinStreak(true);
                    gameMngr.Players[0].GetComponent<ParticleManager>().SpawnWinParticles();
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
