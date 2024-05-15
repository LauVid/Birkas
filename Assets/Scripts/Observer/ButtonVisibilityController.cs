using UnityEngine;
using UnityEngine.UI;

public class ButtonVisibilityController : MonoBehaviour, IObserver
{
    public Button[] buttonsToControl;

    private void Start()
    {
        GameMngr gameManager = FindObjectOfType<GameMngr>();
        
        if (gameManager != null)
        {
            gameManager.AddObserver(this);
        }
    }

    public void OnPlayersTurnChanged(bool isPlayersTurn)
    {
        foreach (var button in buttonsToControl)
        {
            button.gameObject.SetActive(isPlayersTurn);
        }
    }
}