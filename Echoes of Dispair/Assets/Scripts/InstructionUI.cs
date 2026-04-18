using System.Collections;
using UnityEngine;
using TMPro;

public class InstructionUI : MonoBehaviour
{
    public static InstructionUI Instance;

    public TextMeshProUGUI instructionText;

    Coroutine currentRoutine;

    void Awake()
    {
        Instance = this;
        ClearInstruction();
    }

    public void ShowInstruction(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(ClearAfterTime(5f));
        }
    }

    IEnumerator ClearAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ClearInstruction();
    }

    public void ClearInstruction()
    {
        if (instructionText != null)
            instructionText.text = "";
    }
}