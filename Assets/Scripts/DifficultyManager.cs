using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    private Button button;
    public int difficulty; // Difficulty level for the button
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>(); // Find the GameManager in the scene
        button.onClick.AddListener(SetDifficulty); // Add listener for button click
    }

    void SetDifficulty()
    {
        gameManager.StartGame(difficulty); // Start the game with the selected difficulty
    }
}

