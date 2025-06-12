using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameObject instructionPanel;

    // Called when the "Instruction" button is clicked
    public void ShowInstruction()
    {
        instructionPanel.SetActive(true);
    }

    // Called when the "Back" or "Close" button is clicked
    public void HideInstruction()
    {
        instructionPanel.SetActive(false);
    }
}