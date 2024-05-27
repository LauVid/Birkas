using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public class PlayLowestCardHandler : AbstractCardHandler
{
    private GameMngr gameMngr;
    private UserInput userInput;
    public PlayLowestCardHandler(GameMngr gameManager)
    {
        gameMngr = gameManager;
        userInput= gameManager.gameObject.GetComponent<UserInput>();
    }
    public override GameObject Handle(GameObject Player)
    {
        GameObject LowestCard = gameMngr.GetLowestPlayableCard(gameMngr.GetPlayerId(Player));
        List<GameObject> SameStrengthCards=new();
        int SameStrengthCount=0;
        bool HasPakison=gameMngr.HasPakison(Player);
        //If 1 nine in hand and 2 on table take them
        GameObject HasANine=gameMngr.HasANine(Player);
        if(userInput.playedCardsList.Count==3&&HasANine!=null&&!HasPakison)
        {
            int HowManyNines=0;
            for(int i=userInput.playedCardsList.Count-1;i>=userInput.playedCardsList.Count-2;i--)
            {
                if(userInput.playedCardsList[i].GetComponent<Selectable>().Strength==1)
                {
                    HowManyNines++;
                }
            }
            if(HowManyNines==2)
            {
                return base.Handle(Player);
            }
        }

        //If 2 aces are played play pakison
        if(userInput.playedCardsList.Count>=3)
        {
            int HowManyAces=0;
            if(gameMngr.HasPakison(Player))
            {
                for(int i=userInput.playedCardsList.Count-1;i>=userInput.playedCardsList.Count-2;i--)
                {
                    if(userInput.playedCardsList[i].GetComponent<Selectable>().Strength==6)
                    {
                        HowManyAces++;
                    }
                }
                if(HowManyAces==2)
                {
                    return base.Handle(Player);
                }
            }

        }

        //If 3 aces are played take it if not last card/pakison in hand
        if(userInput.playedCardsList.Count>=4)
        {
            int HowManyAces=0;
            if(Player.transform.childCount>1)
            {
                if(HasPakison&&Player.transform.childCount==3)
                {
                    return base.Handle(Player);
                }
                for(int i=userInput.playedCardsList.Count-1;i>=userInput.playedCardsList.Count-3;i--)
                {
                    if(userInput.playedCardsList[i].GetComponent<Selectable>().Strength==6)
                    {
                        HowManyAces++;
                    }
                }
                if(HowManyAces==3)
                {
                    gameMngr.AISkipPakison=true;
                    return base.Handle(Player);
                }
            }

        }

        if(LowestCard!=null)
        {
            foreach(Transform card in Player.transform)
            {
                if(card.GetComponent<Selectable>().Strength==LowestCard.GetComponent<Selectable>().Strength)
                {
                    SameStrengthCards.Add(card.gameObject);
                    SameStrengthCount++;
                }            
            }

            if(SameStrengthCount==4 &&
            !(SameStrengthCards.First().GetComponent<Selectable>().Strength==6&&Player.transform.childCount>4&&!HasPakison))
            {
                gameMngr.PlayMultipleCards(SameStrengthCards);
                SameStrengthCards.Clear();
                return null;
            }
            else if(SameStrengthCards.First().GetComponent<Selectable>().IsPartOfPakison)
            {
                if(userInput.playedCardsList.Count==1)
                {
                    LowestCard = gameMngr.GetLowestNotPakison(Player);
                    if(LowestCard!=null)
                    {
                        gameMngr.PlayCard(LowestCard);
                        return null;                        
                    }
                }
                return base.Handle(Player);
            }
            else
            {
                gameMngr.PlayCard(LowestCard);
                return null;
            }               
        }
        else
        {
            return base.Handle(Player);
        }
    }

}

class Take3CardsHandler : AbstractCardHandler
{
    private GameMngr gameMngr;
    private UserInput userInput;
    public Take3CardsHandler(GameMngr gameManager)
    {
        gameMngr = gameManager;
        userInput= gameManager.gameObject.GetComponent<UserInput>();
    }
    public override GameObject Handle(GameObject Player)
    {
        if(gameMngr.HasPakison(Player)&&!gameMngr.AISkipPakison)
        {
            gameMngr.Play3Nines(gameMngr.GetPakison(Player));
            return null;
        }
        gameMngr.AISkipPakison=false;
        if (userInput.playedCardsList.Count==7)
        {
            if(userInput.playedCardsList[1].GetComponent<Selectable>().IsPartOfPakison==true)
            {
                return base.Handle(Player);
            }
            gameMngr.Take3Cards(Player);
            return null;
        }

        if(Random.Range(0,100)<gameMngr.ChanceToTakeAll)
        {
            return base.Handle(Player);
        }
        else
        {
            gameMngr.Take3Cards(Player);
            return null;
        }
    }
}

class TakeAllCardsHandler : AbstractCardHandler
{
    private GameMngr gameMngr;
    public TakeAllCardsHandler(GameMngr gameManager)
    {
        this.gameMngr = gameManager;
    }
     public override GameObject Handle(GameObject Player)
    {
        gameMngr.TakeAllCards(Player);
        return null;
    }
}
