using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameMngr : MonoBehaviour
{
    
    private static GameMngr instance;

    public static GameMngr Instance
    {
        get
        {
            // If the instance is null, try to find an existing instance in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<GameMngr>();

                // If no instance is found, log an error message
                if (instance == null)
                {
                    Debug.LogError("No GameMngr instance found in the scene.");
                }
            }

            return instance;
        }
    }

    public Sprite[] CardFaces;
    public GameObject CardPrefab;
    public GameObject[] Players;
    public static string[] Suits = new string[] {"Hearts","Diamonds","Clubs","Spades"};
    public static string[] Values = new string[]{"9","10","Jack","Queen","King","Ace"};
    public List<string> Deck;

    private Selectable selectable;

    public GameObject PlayedCards;

    private UserInput userInput;

    public bool playersTurn;
    private List<IObserver> observers = new List<IObserver>();

    public bool PlayersTurn
    {
        get { return playersTurn; }
        set
        {
            if (playersTurn != value)
            {
                playersTurn = value;
                NotifyObservers();
            }
        }
    }

    public void AddObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnPlayersTurnChanged(playersTurn);
        }
    }

    public PlayLowestCardHandler playLowestCardHandler;
    

    public GameObject WinPanel;
    public GameObject LosePanel;

    private float currentZOffset = 0.01f;
    public static List<string> GenerateDeck()
    {
        List<string> NewDeck= new List<string>();

        foreach(string suit in Suits)
        {
            foreach(string value in Values)
            {
                NewDeck.Add(value+" Of "+suit);
            }
        }
        return NewDeck;
    }

    void ShuffleDeck<T>(List<T> List)
    {
        System.Random random =new System.Random();
        int n = List.Count;
        while (n>1)
        {
            int k =random.Next(n);
            n--;
            T temp = List[k];
            List[k] = List[n];
            List[n] = temp;
        }
    }
public void DealCards()
{
    float xOffset = 0f; 
    float yOffset = 0.0f; 
    float zOffset = 0.03f;

    Deck = GenerateDeck();
    ShuffleDeck(Deck);

    StartCoroutine(DealCards(xOffset, yOffset, zOffset));
}

private IEnumerator DealCards(float xOffset, float yOffset, float zOffset)
{
    for (int i = 0; i < Deck.Count; i++)
    {
        string card = Deck[i];
        int playerIndex = i % Players.Length; 

        GameObject currentPlayer = Players[playerIndex];

        
        Vector3 localCardPosition = new Vector3(xOffset, yOffset, zOffset);

       
        localCardPosition = currentPlayer.transform.rotation * localCardPosition;

       
        Vector3 cardPosition = currentPlayer.transform.position + localCardPosition;

        GameObject newCard = Instantiate(CardPrefab, cardPosition, currentPlayer.transform.rotation, currentPlayer.transform);
        newCard.name = card;

        
        selectable.CheckCardParent(newCard);


        xOffset+=0.1f;
        zOffset-=0.01f;

        yield return new WaitForSeconds(0.05f);
    }
    Play9OfDiamonds();
}
    public void Play9OfDiamonds()
    {
        GameObject NineOfDiamonds = GameObject.Find("9 Of Diamonds");
        NineOfDiamonds.GetComponent<Selectable>().FaceUp=true;
        int playerid = GetPlayerId(NineOfDiamonds.transform.parent.gameObject);
        Debug.Log(playerid+"    <---- This guy had the 9");
        StartCoroutine(PlayCardWithoutChecking(NineOfDiamonds,playerid));

    }

    public void PlayCard(GameObject card)
    {
        if(userInput.Stackable(card))
        {
            Debug.Log("The played card's strength is:"+card.GetComponent<Selectable>().Strength);            
            float randomRotation = Random.Range(-45f, 45f);

            ReorganizeCards(card.transform.parent.gameObject,0.4f,0f,-0.01f);
            
            card.transform.parent = PlayedCards.transform;

            
            card.transform.position = new Vector3(
                PlayedCards.transform.position.x,
                PlayedCards.transform.position.y,
                PlayedCards.transform.position.z - currentZOffset
            );
            card.transform.Rotate(Vector3.forward, randomRotation);
            currentZOffset+=0.01f;   
            userInput.playedCardsList.Add(card);   
            card.GetComponent<Selectable>().FaceUp=true;
        }

    }

    public IEnumerator PlayCardWithoutChecking(GameObject card,int WhoHad9Id)
    {
            ReorganizeCards(card.transform.parent.gameObject,0.4f,0f,-0.01f);
           
            card.transform.parent = PlayedCards.transform;

            
            card.transform.position = new Vector3(
                PlayedCards.transform.position.x,
                PlayedCards.transform.position.y,
                PlayedCards.transform.position.z
            );          
        userInput.playedCardsList.Add(card);


        
        for(int i = WhoHad9Id+1;i<4;i++)
        {
            yield return new WaitForSeconds(2f);
            PlayAICard(Players[i],playLowestCardHandler);
        }
        PlayersTurn=true;
        
    }

    public GameObject GetPlayer(int i)
    {
        return Players[i];
    }

    public int GetPlayerId(GameObject Player)
    {
        for(int i=0;i<4;i++)
        {
            if(Players[i]==Player)
            {
                return i;
            }
        }
        return -1;
    }

    public GameObject GetLowestPlayableCard(int i)
    {
        GameObject LowestCard= null;
        int LowestCardStrength=int.MaxValue;

        foreach (Transform Card in Players[i].transform)
        {
            GameObject card = Card.gameObject;
            if(userInput.Stackable(card))
            {
                if(card.GetComponent<Selectable>().Strength < LowestCardStrength)
                {
                    LowestCardStrength=card.GetComponent<Selectable>().Strength;
                    LowestCard=card;
                }
            }
        }
        if(LowestCard!=null)
        {
        return LowestCard;             
        }       
        else
        return null;
    }

    public void Take3Cards(GameObject player)
    {
        int CardListCount = userInput.playedCardsList.Count;
        int CardsToTake = Mathf.Min(3, CardListCount - 1); 

        GameObject Card;
        for (int i = CardListCount - 1; i >= CardListCount - CardsToTake; i--)
        {
        if (i <= 0)
        {
            break; 
        }

            Card=userInput.playedCardsList[i];
            if(Card!=userInput.playedCardsList[0])
            {
                Debug.Log("Player:" +player.name+"has taken a card: " + Card.name);
                Card.transform.parent = player.transform;
                if(player.name!="Player")
                {
                    Card.GetComponent<Selectable>().FaceUp=false;
                }

                Card.transform.rotation = Quaternion.identity;
                userInput.playedCardsList.Remove(Card);
            }
        }
        ReorganizeCards(player.gameObject,0.4f,0f,-0.01f);

        if(player.name=="Player")
        {
            StartCoroutine(PcTurn());
        }
    }

    public IEnumerator PcTurn()
    {
        PlayersTurn=false;
        for(int i=1;i<=3;i++)
        {

            GameObject Player = GetPlayer(i);

            //If player has any cards left
            if(Player.transform.childCount>0)
            {
                yield return new WaitForSeconds(2f);                
                PlayAICard(Player,playLowestCardHandler);                
            }

            if(Players[1].transform.childCount==0&&Players[2].transform.childCount==0&&Players[3].transform.childCount==0)
            {
                LosePanel.SetActive(true);
                Debug.Log("You lost the game loser");
            }

            if (i==3)
            {
                Debug.Log("Player's turn");
                PlayersTurn=true;
            }
        }  
    }

    public void TakeAllCards(GameObject player)
    {

        int CardListCount = userInput.playedCardsList.Count;

        GameObject Card;
        for (int i = CardListCount - 1; i > 0; i--)
        {
        if (i <= 0)
        {
            break;
        }

            Card=userInput.playedCardsList[i];
            if(Card!=userInput.playedCardsList[0])
            {
                Debug.Log("Player:" +player.name+"has taken a card: " + Card.name);
                Card.transform.parent = player.transform;
                if(player.name!="Player")
                {
                    Card.GetComponent<Selectable>().FaceUp=false;                         
                }

                Card.transform.rotation = Quaternion.identity;
                userInput.playedCardsList.Remove(Card);
            }
        }
        ReorganizeCards(player.gameObject,0.4f,0f,-0.01f);

        if(player.name=="Player")
        {
            StartCoroutine(PcTurn());
        }
    }


    public void ReorganizeCards(GameObject player, float xOffset,float yOffset, float zOffset)
    {
        Debug.Log("Sorting cards of player:" + player.name);
        Transform playerTransform = player.transform;
        List<Transform> cards = new List<Transform>();

        for (int i = 0; i < playerTransform.childCount; i++)
        {
            Transform cardTransform = playerTransform.GetChild(i);
            cards.Add(cardTransform);
        }

        
        cards.Sort((a, b) =>
        {
            
            int strengthA = a.GetComponent<Selectable>().Strength;
            int strengthB = b.GetComponent<Selectable>().Strength;

            return strengthA.CompareTo(strengthB);
        });

        for (int i = 0; i < cards.Count; i++)
        {
            Transform cardTransform = cards[i];

            Vector3 localCardPosition = new Vector3(xOffset*i, yOffset, zOffset*i);

           
            localCardPosition = playerTransform.transform.rotation * localCardPosition;

            
            Vector3 cardPosition = playerTransform.transform.position + localCardPosition;

            cardTransform.position = cardPosition;
        }


    }

    public void PlayAICard(GameObject Player, AbstractCardHandler handler)
    {
        handler.Handle(Player);
    }


    // Start is called before the first frame update
    void Start()
    {
        
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        
        instance = this;
        userInput=GetComponent<UserInput>();
        selectable=GetComponent<Selectable>();
        Deck=GenerateDeck();
        PlayersTurn=false;
        playersTurn=false;
        DealCards();
        for(int i=0;i<4;i++)
        {
            ReorganizeCards(Players[i].gameObject,0.4f,0f,-0.01f);
        }
        
       
        playLowestCardHandler = new PlayLowestCardHandler(this);
        Take3CardsHandler take3CardsHandler = new Take3CardsHandler(this);
        TakeAllCardsHandler takeAllCardsHandler = new TakeAllCardsHandler(this);

        // Build the chain: PlayLowestCardHandler -> Take3CardsHandler -> TakeAllCardsHandler
        playLowestCardHandler.SetNext(take3CardsHandler).SetNext(takeAllCardsHandler);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
