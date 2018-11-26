using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextManager : MonoBehaviour {
    [Header("History Settings")]
    public TextMeshProUGUI textDisplay;
    public float typingSpeed, modifier;
    public Historia[] historia;

    [Header("Object References")]
    public GameObject continueButton;
    public Animator animator;

    [HideInInspector]
    public int parteHistoria = 0;
    public bool historiaCompleta = false;

    private bool typing = true;
    private int index;
    private float actualTypingSpeed;

	void Update () {
        if (!typing) continueButton.SetActive(true);
        if (!typing && Input.anyKeyDown) this.NextSentence();
	}

    public IEnumerator Type()
    {
        if (historia[parteHistoria].special[index])
        {
            actualTypingSpeed = typingSpeed * modifier;
            textDisplay.alignment = TMPro.TextAlignmentOptions.Center;
        } else
        {
            actualTypingSpeed = typingSpeed;
            textDisplay.alignment = TMPro.TextAlignmentOptions.Left;
        }

        textDisplay.text += "<#464646>";
        foreach (char letter in historia[parteHistoria].talker[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(actualTypingSpeed);
        }
        
        textDisplay.text += "</color><color=white>";
        foreach (char letter in historia[parteHistoria].sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(actualTypingSpeed);
        }

        textDisplay.text += "</color>";
        typing = false;
    }

    public void NextSentence()
    {
        typing = true;
        animator.SetTrigger("Change");
        continueButton.SetActive(false);

        if(index < historia[parteHistoria].sentences.Length - 1)
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(Type());
        }
        else
        {
            index = 0;
            textDisplay.text = "";
            historiaCompleta = true;
        }
    }

    [System.Serializable]
    public class Historia
    {
        public string[] talker;
        public string[] sentences;
        public bool[] special;
    }

}
