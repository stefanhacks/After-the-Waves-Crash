using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageCaster : MonoBehaviour {

    private GameManager gm;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void DoingFlash()
    {
        gm.DoWhileFlash();
    }
}
