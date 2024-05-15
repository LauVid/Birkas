using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public class PlayLowestCardHandler : AbstractCardHandler
{
    private GameMngr gameMngr;
    public PlayLowestCardHandler(GameMngr gameManager)
    {
        this.gameMngr = gameManager;
    }
    public override GameObject Handle(GameObject Player)
    {
        GameObject LowestCard = gameMngr.GetLowestPlayableCard(gameMngr.GetPlayerId(Player));
        if (LowestCard!= null)
        {
            gameMngr.PlayCard(LowestCard);
            return null;
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
    public Take3CardsHandler(GameMngr gameManager)
    {
        this.gameMngr = gameManager;
    }
    public override GameObject Handle(GameObject Player)
    {
        if (Player.transform.childCount<7)
        {
            gameMngr.Take3Cards(Player);
            return null;
        }
        else
        {
            return base.Handle(Player);
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
        if (Player.transform.childCount>=7)
        {
            gameMngr.TakeAllCards(Player);
            return null;
        }
        else
        {
            return base.Handle(Player);
        }
    }
}
