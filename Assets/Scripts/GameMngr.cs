using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using DG.Tweening;
using UnityEngine.Scripting.APIUpdating;

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
    private AudioManager audioManager;

    public GameObject CurrentWinStreakText;
    public GameObject BestWinStreakText;
    public int ChanceToTakeAll=0;

    public bool playersTurn;

    private float CardMovementDuration=1f;
    public float ComputerTurnDelaySeconds=2f;
    private bool Lost=false;
    public bool GameEnded=false;
    public bool AISkipPakison=false;
    private float cardMovementDuration;
    private bool AIPlayedACard=false;
    private List<IObserver> observers = new List<IObserver>();

    public bool PlayersTurn
    {
        get { return playersTurn; }
        set
        {
            if (playersTurn != value)
            {
                playersTurn = value;
                if(value&&AIPlayedACard)
                {
                    cardMovementDuration=CardMovementDuration;                    
                }
                else
                {
                    cardMovementDuration=0;
                }
                
                if(!value&&userInput.AlreadyActivatingButtons)
                {
                    userInput.StopActivatingButtons=true;
                    StartCoroutine(ChangeStopActivatingButtons());
                }
                else
                {
                    StartCoroutine(userInput.ButtonsSetActive(value,cardMovementDuration));                    
                }
                ColorCardsByPlayable();
            }
        }
    }

    private IEnumerator ChangeStopActivatingButtons()
    {
        yield return new WaitForSeconds(.9f);
        userInput.StopActivatingButtons=false;
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
            (List[n], List[k]) = (List[k], List[n]);
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
        GameObject TopDeckCard = Instantiate(CardPrefab,new Vector3(0,0,20),new Quaternion(0,0,0,0));
    for (int i = 0; i < Deck.Count; i++)
    {
        if(i%4==0)
        {
            StartCoroutine(audioManager.PlaySound(audioManager.Card1,0f));             
        }
       
        string card = Deck[i];
        int playerIndex = i % Players.Length; 

        GameObject currentPlayer = Players[playerIndex];

        
        Vector3 localCardPosition = new Vector3(xOffset, yOffset, zOffset);

       
        localCardPosition = currentPlayer.transform.rotation * localCardPosition;

       
        Vector3 cardPosition = currentPlayer.transform.position + localCardPosition;
        
        GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity, currentPlayer.transform);
        newCard.name = card;

        newCard.transform.DOMove(cardPosition,.3f);
        newCard.transform.DORotateQuaternion(currentPlayer.transform.rotation,.3f);
        
        selectable.CheckCardParent(newCard);

        if(i==Deck.Count-2)
        {
            Destroy(TopDeckCard);
        }

        xOffset+=0.1f;
        zOffset-=0.01f;

        yield return new WaitForSeconds(0.05f);
    }
    Play9OfDiamonds();
    for(int i=0;i<Players.Count();i++)
    {
        ReorganizeCards(Players[i],CardMovementDuration);        
    }

}
    public void Play9OfDiamonds()
    {
        StartCoroutine(audioManager.PlaySound(audioManager.Card1,0f));
        GameObject NineOfDiamonds = GameObject.Find("9 Of Diamonds");
        NineOfDiamonds.GetComponent<Selectable>().FaceUp=true;
        int playerid = GetPlayerId(NineOfDiamonds.transform.parent.gameObject);
        Debug.Log(playerid+"    <---- This guy had the 9");
        StartCoroutine(PlayCardWithoutChecking(NineOfDiamonds,playerid));
        ColorCardsByPlayable();
    }

    public void PlayCard(GameObject card)
    {
        AIPlayedACard=true;
        StartCoroutine(audioManager.PlaySound(audioManager.Card1,CardMovementDuration-0.4f));
        if(userInput.Stackable(card))
        {
            GameObject WhoPlayedIt=card.transform.parent.gameObject;           
            float randomRotation = Random.Range(-45f, 45f);
            
            card.transform.parent = PlayedCards.transform;

            Vector3 Pos=new(PlayedCards.transform.position.x, PlayedCards.transform.position.y, PlayedCards.transform.position.z - currentZOffset);
            card.transform.DORotate(new Vector3(0,0,randomRotation), CardMovementDuration);            
            card.transform.DOMove(Pos,CardMovementDuration);
            ReorganizeCards(WhoPlayedIt,CardMovementDuration);            
            currentZOffset+=0.01f;   
            userInput.playedCardsList.Add(card);   
            card.GetComponent<Selectable>().FaceUp=true;
            card.GetComponent<Selectable>().sprite.color=Color.white;
            if(WhoPlayedIt.transform.childCount==0)
            {
                WhoPlayedIt.GetComponent<ParticleManager>().SpawnWinParticles();
            }
        }
    }

    public void PlayMultipleCards(List<GameObject> cards)
    {
        AIPlayedACard=true;
        StartCoroutine(audioManager.PlaySound(audioManager.Card1,CardMovementDuration-0.4f));
        GameObject WhoPlayedIt=cards[0].transform.parent.gameObject;
        float Rotation = Random.Range(-45f,45f);        
        foreach(GameObject card in cards)
        {
            card.transform.parent = PlayedCards.transform;
            Vector3 pos = new Vector3(
                PlayedCards.transform.position.x,
                PlayedCards.transform.position.y,
                PlayedCards.transform.position.z - currentZOffset
            );    
            Vector3 rot =new(0,0,Rotation);
            Rotation+=5f;
            card.transform.DOMove(pos,CardMovementDuration);
            card.transform.DORotate(rot, CardMovementDuration);
            currentZOffset+=0.01f;   
            userInput.playedCardsList.Add(card);   
            card.GetComponent<Selectable>().FaceUp=true;
            card.GetComponent<Selectable>().sprite.color=Color.white;         
        }
        ReorganizeCards(WhoPlayedIt,CardMovementDuration);
        if(WhoPlayedIt.transform.childCount==0)
        {
            WhoPlayedIt.GetComponent<ParticleManager>().SpawnWinParticles();
        }
    }

    public void Play3Nines(List<GameObject> cards)
    {
        AIPlayedACard=true;
        StartCoroutine(audioManager.PlaySound(audioManager.Card1,CardMovementDuration-0.4f));
        GameObject WhoPlayedIt=cards[0].transform.parent.gameObject;
        float rotation=15f;
        float ZOffSet=1f;
        foreach(GameObject card in cards)
        {
            card.GetComponent<Selectable>().IsPartOfPakison=true;
            rotation+=15f;
            card.transform.parent = PlayedCards.transform;
            Vector3 position = new Vector3(
                PlayedCards.transform.position.x-2,
                PlayedCards.transform.position.y,
                PlayedCards.transform.position.z + ZOffSet
            );
            Vector3 rot =new(0,0,rotation);
            card.transform.DOMove(position,CardMovementDuration);
            card.transform.DORotate(rot, CardMovementDuration);
            ZOffSet+=0.01f;   
            userInput.playedCardsList.Insert(1,card);   
            card.GetComponent<Selectable>().FaceUp=true;
            card.GetComponent<Selectable>().sprite.color=Color.white;                   
        }         
        ReorganizeCards(WhoPlayedIt,CardMovementDuration);
        if(WhoPlayedIt.transform.childCount==0)
        {
            WhoPlayedIt.GetComponent<ParticleManager>().SpawnWinParticles();
        }
    }

    public IEnumerator PlayCardWithoutChecking(GameObject card,int WhoHad9Id)
    {
        card.transform.parent = PlayedCards.transform;

            
            Vector3 position = new Vector3(
                PlayedCards.transform.position.x,
                PlayedCards.transform.position.y,
                PlayedCards.transform.position.z
            );
            card.transform.DOMove(position,CardMovementDuration);
        userInput.playedCardsList.Add(card);


        ReorganizeCards(Players[WhoHad9Id],CardMovementDuration);
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

    public GameObject GetLowestNotPakison(GameObject Player)
    {
        GameObject LowestCard= null;
        int LowestCardStrength=int.MaxValue;

        foreach (Transform Card in Player.transform)
        {
            GameObject card = Card.gameObject;
            if(userInput.Stackable(card)&&card.GetComponent<Selectable>().IsPartOfPakison==false)
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
        {
            return null;            
        }

    }

    public void Take3Cards(GameObject player)
    {
        AIPlayedACard=false;
        StartCoroutine(audioManager.PlaySound(audioManager.Card2,0f));
        int CardListCount = userInput.playedCardsList.Count;
        int CardsToTake = Mathf.Min(3, CardListCount - 1); 

        GameObject Card;
        for (int i = CardListCount - 1; i >= CardListCount - CardsToTake; i--)
        {
            if (i <= 0)
            {
                return; 
            }

            Card=userInput.playedCardsList[i];
            CheckForPakison(player);
            if(Card.GetComponent<Selectable>().IsPartOfPakison)
            {
                List<Transform> PakisonCards=new();

                Transform nine=PlayedCards.transform.Find("9 Of Hearts");
                if(nine!=null)
                {
                    PakisonCards.Add(nine);                    
                }

                nine=PlayedCards.transform.Find("9 Of Clubs");
                if(nine!=null)
                {
                    PakisonCards.Add(nine);                    
                }
                nine=PlayedCards.transform.Find("9 Of Spades");
                if(nine!=null)
                {
                    PakisonCards.Add(nine);                    
                }

                foreach(Transform card in PakisonCards)
                {
                    card.parent = player.transform;
                    if(player.name!="Player")
                    {
                        card.GetComponent<Selectable>().FaceUp=false;
                    }
                    card.DORotateQuaternion(player.transform.rotation,CardMovementDuration);
                    userInput.playedCardsList.Remove(card.gameObject);
                    if(userInput.playedCardsList.Count==1)
                    {
                        ReorganizeCards(player,CardMovementDuration);
                        CheckForPakison(player);
                        if (player.name=="Player")
                        {
                            StartCoroutine(PcTurn());
                        }
                        return;
                    }
                }
            }
            else if(Card!=userInput.playedCardsList[0])
            {
                Debug.Log("Player:" +player.name+"has taken a card: " + Card.name);
                Card.transform.parent = player.transform;
                if(player.name!="Player")
                {
                    Card.GetComponent<Selectable>().FaceUp=false;
                }
                Card.transform.DORotateQuaternion(player.transform.rotation,CardMovementDuration);
                userInput.playedCardsList.Remove(Card);
            }
        }

        ReorganizeCards(player,CardMovementDuration);
        CheckForPakison(player);
        if (player.name=="Player")
        {
            StartCoroutine(PcTurn());
        }
    }

    public bool HasPakison(GameObject player)
    {
        Transform nine=player.transform.Find("9 Of Hearts");
        if(nine!=null)
        {
            if(nine.GetComponent<Selectable>().IsPartOfPakison==true)
            {
                return true;
            }
        }
        return false;
    }
    public List<GameObject> GetPakison(GameObject Player)
    {
        List<GameObject> Pakison=new();
        Transform nine1=Player.transform.Find("9 Of Hearts");
        if(nine1!=null)
        {
            Transform nine2=Player.transform.Find("9 Of Clubs");      
            if(nine2!=null)
            {
                Transform nine3=Player.transform.Find("9 Of Spades"); 
                if(nine3!=null)
                {
                    Pakison.Add(nine1.gameObject);
                    Pakison.Add(nine2.gameObject);
                    Pakison.Add(nine3.gameObject);
                    return Pakison;
                }                                   
            }                         
        }
        return null;
    }

    public GameObject HasANine(GameObject Player)
    {
        Transform nine=Player.transform.Find("9 Of Hearts");
        if(nine!=null)
        {
            return nine.gameObject;
        }
        nine=Player.transform.Find("9 Of Clubs");      
        if(nine!=null)
        {
            return nine.gameObject;
        }
        nine=Player.transform.Find("9 Of Spades"); 
        if(nine!=null)
        {
            return nine.gameObject;
        }
        return null;                                                                    
    }
    public void CheckForPakison(GameObject player)
    {
        Transform nine1=player.transform.Find("9 Of Hearts");
        if(nine1!=null)
        {
            Transform nine2=player.transform.Find("9 Of Clubs");      
            if(nine2!=null)
            {
                Transform nine3=player.transform.Find("9 Of Spades"); 
                if(nine3!=null)
                {
                    nine1.GetComponent<Selectable>().IsPartOfPakison=true;
                    nine2.GetComponent<Selectable>().IsPartOfPakison=true;
                    nine3.GetComponent<Selectable>().IsPartOfPakison=true;
                }                                   
            }                         
        }

    }
    public IEnumerator PcTurn()
    {
        int HasNoCardsCount;
        if(!GameEnded)
        {
            HasNoCardsCount=0;            
        }
        else
        {
            HasNoCardsCount=1;
        }
        PlayersTurn=false;
        for(int i=1;i<=3;i++)
        {

            GameObject Player = GetPlayer(i);

            //If player has any cards left
            if(Player.transform.childCount>0)
            {
                if(!GameEnded)
                {
                    ComputerTurnDelaySeconds=1.2f+Random.Range(0f,1f);
                }
                yield return new WaitForSeconds(ComputerTurnDelaySeconds);                
                PlayAICard(Player,playLowestCardHandler);
            }
            else
            {
                HasNoCardsCount++;
            }
            if(HasNoCardsCount>=2&&GameEnded)
            {
                ChanceToTakeAll=6;
            }
            if(HasNoCardsCount>=2||GetPlayer(0).transform.childCount>0)
            {
                if(Players[1].transform.childCount==0&&Players[2].transform.childCount==0&&Players[3].transform.childCount==0&&!GameEnded&&!Lost)
                {
                    GameEnded=true;
                    ComputerTurnDelaySeconds=0.5f;
                    CardMovementDuration=0.49f;
                    Lost=true;
                    ManageCurrentWinStreak(false);
                    LosePanel.SetActive(true);
                    Debug.Log("You lost the game loser");
                }

                if (i==3&&!GameEnded)
                {
                    Debug.Log("Player's turn");
                    PlayersTurn=true;
                }
                if(i==3&&GameEnded&&!Lost&&HasNoCardsCount!=3)
                {
                    StartCoroutine(PcTurn());
                }
            }
            else
            {
                if(i==3&&GameEnded&&!Lost&&HasNoCardsCount!=3)
                {
                    StartCoroutine(PcTurn());
                }                
            }

        }
    }

    public void TakeAllCards(GameObject player)
    {
        AIPlayedACard=false;
        StartCoroutine(audioManager.PlaySound(audioManager.Card2,0f));
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

                Card.transform.DORotateQuaternion(player.transform.rotation,CardMovementDuration);
                userInput.playedCardsList.Remove(Card);
            }
        }
        CheckForPakison(player);
        ReorganizeCards(player,CardMovementDuration);
        if (player.name=="Player")
        {
            StartCoroutine(PcTurn());
        }
    }


    public void ReorganizeCards(GameObject player,float duration)
    {
        Transform playerTransform = player.transform;
        List<Transform> cards = new();
        int j=1;
        Vector3 AddToPositionBehind=new(0f,0f,-1f);
        float xOffset=0f;
        float yOffset=0f;
        float zOffset=-0.1f;
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

        if(player.name=="Player")
        {
            AddToPositionBehind=new(-0.3f,0f,0.01f);
            xOffset=2f;
            zOffset=-0.5f;
        }
        else if(player.name=="PC1")
        {
            AddToPositionBehind=new(0f,-0.2f,-0.01f);
            yOffset=-0.6f;
            zOffset=-0.5f;
        }
        else if(player.name=="PC2")
        {
            AddToPositionBehind=new(-0.2f,0f,-0.01f);
            xOffset=-0.6f;
            zOffset=-0.5f;
        }
        else if(player.name=="PC3")
        {
            AddToPositionBehind=new(0f,0.2f,-0.01f);
            yOffset=0.6f;
            zOffset=-0.5f;
        }

        Vector3 PreviousCardPosition=new(0,0,0);
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 AddToPosition=new(xOffset*j,yOffset*j,zOffset*j);
            
            if(i==0)
            {
                cards[i].transform.DOMove(playerTransform.position,duration);
                //StartCoroutine(MoveObjectToLocation(cards[i].transform,playerTransform.position,speed));
                PreviousCardPosition=playerTransform.position;
            }
            else if(cards[i-1].GetComponent<Selectable>().Strength == cards[i].GetComponent<Selectable>().Strength)
            {
                //PLACE CARD BEHIND IT
                cards[i].transform.DOMove(PreviousCardPosition+AddToPositionBehind,duration);
                //StartCoroutine(MoveObjectToLocation(cards[i].transform,PreviousCardPosition+AddToPositionBehind,speed));
                PreviousCardPosition+=AddToPositionBehind;
            }
            else
            {
                //GO TO THE SIDE AND PLACE IT THERE
                cards[i].transform.DOMove(playerTransform.position+AddToPosition,duration);
                //StartCoroutine(MoveObjectToLocation(cards[i].transform,playerTransform.position+AddToPosition,speed));
                PreviousCardPosition=playerTransform.position+AddToPosition;
                j++;
            }
        }
    }

    public IEnumerator MoveObjectToLocation(Transform Card, Vector3 EndLocation,float speed)
    {   
		float distanceToTarget = Vector3.Distance(Card.transform.position, EndLocation);
		// Continue the loop as long as the distance to target is greater than a small value to avoid floating point precision issues.
		while (distanceToTarget > 0.001f)
		{
			Card.transform.position = Vector3.Lerp(Card.transform.position, EndLocation, speed * Time.deltaTime / distanceToTarget);

			distanceToTarget = Vector3.Distance(Card.transform.position, EndLocation);


			yield return null;
		}
		Card.transform.position = EndLocation;
    }


    public void PlayAICard(GameObject Player, AbstractCardHandler handler)
    {
        handler.Handle(Player);
    }


    public void ColorCardsByPlayable()
    {
        if(PlayersTurn==true)
        {
            foreach(Transform card in Players[0].transform)
            {
                Selectable Card =card.GetComponent<Selectable>();
                Card.ChooseClickableColors(userInput.playedCardsList.Last().GetComponent<Selectable>().Strength);
            }                    
        }
        else
        {
            foreach(Transform card in Players[0].transform)
            {
                card.GetComponent<Selectable>().sprite.color=Color.gray;
            }    
        }
    }

    public List<GameObject> Find4OfSame(GameObject player,int SkipThisNumber)
    {
        List<GameObject> Same4=new();
        int SameStrengthCount=0;
        for(int i=1;i<=6;i++)
        {   
            if(i!=SkipThisNumber&&i>=userInput.playedCardsList.Last().GetComponent<Selectable>().Strength)
            {
                foreach(Transform card in player.transform)
                {
                    Selectable Card =card.GetComponent<Selectable>();
                    if(Card.Strength==i)
                    {
                        SameStrengthCount++;
                        Same4.Add(card.gameObject);
                    }
                } 
                if(SameStrengthCount==4)
                {
                    return Same4;
                }
                else
                {
                    SameStrengthCount=0;
                    Same4.Clear();
                }
            }
        }
        Same4.Clear();
        return Same4;
    }

    public void ManageCurrentWinStreak(bool WonGame)
    {
        int CurrentWinStreak=PlayerPrefs.GetInt("CurrentWinStreak",0);
        if(WonGame)
        {
            CurrentWinStreak++;
            PlayerPrefs.SetInt("CurrentWinStreak",CurrentWinStreak);

        }
        else
        {
            PlayerPrefs.SetInt("CurrentWinStreak",0);           
        }
        CurrentWinStreakText.GetComponent<TextMeshProUGUI>().text="Current Win Streak: "+PlayerPrefs.GetInt("CurrentWinStreak",0);
        ManageBestWinStreak();
        CurrentWinStreakText.SetActive(true);
    }

    public void ManageBestWinStreak()
    {
        int CurrentWinStreak=PlayerPrefs.GetInt("CurrentWinStreak",0);
        int BestWinStreak=PlayerPrefs.GetInt("BestWinStreak",0);
        if(CurrentWinStreak>BestWinStreak)
        {
            PlayerPrefs.SetInt("BestWinStreak",CurrentWinStreak);
        }
        BestWinStreakText.GetComponent<TextMeshProUGUI>().text="Best Win Streak: "+PlayerPrefs.GetInt("BestWinStreak",0);
        BestWinStreakText.SetActive(true);   
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
        audioManager=GameObject.Find("AudioManager").GetComponent<AudioManager>();
        Deck=GenerateDeck();
        PlayersTurn=false;
        DealCards();
       
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
