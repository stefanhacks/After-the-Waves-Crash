using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Title Animation Settings")]
    public int entranceIterations = 2;
    public float entranceWarmUp = 0.75f, entrancePreambleDelay = 1.25f, entranceDelay = 2, entrancePressKeyDelay = 2, firstTimeDelay = 3, thunderInterval = 10;

    [Header ("Game Animation Settings")]
    public int firstTimeIterations = 1;
    public float firstTimePreamblePreambleDelay = 1.25f;
    public float afterGamePlayDelay = 1.25f;
    public float endGameDelay = 2.5f;
    
    [Header("Game Object References")]
    public Sprite[] spritesRaio;
    public AudioClip raioClip, krakenClip;
    public GameObject raio, flash, textBox, title, textPressAnyKey, credits, backGround1, backGround2, telaPreta, kraken, barcoInteiro, barcoQuebrado, krakenFadeA, krakenFadeB, bubbleGenerator;
    
    [HideInInspector]
    public enum GameState { Inicio, Historia, Transicao, GamePlay, GameEnd, GameOver }
    public GameState currentState = GameState.Inicio;

    private Animator anim;
    private KeyManager kManager;
    private TextManager tManager;
    private SoundManager sdManager;
    private ParticleSystem.EmissionModule bubbles;

    private bool chamaHistoria = false, endedStart = false, titleAnimationOver = false, titlePreambleDone = false, titleShows = false, setUpFlash = false, firstTime = true;
    private float thunderTimer = 0, entranceTimer = 0, firstTimeTimer = 0, afterGameTimer = 0, endGameTimer = 0;
    private int iterationsDone = 0, firstTimeIterationsDone = 0;

    void Start()
    {
        sdManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        tManager = GameObject.FindGameObjectWithTag("TextManager").GetComponent<TextManager>();
        kManager = GameObject.FindGameObjectWithTag("KeyManager").GetComponent<KeyManager>();
        anim = this.GetComponent<Animator>();
        bubbles = bubbleGenerator.GetComponent<ParticleSystem>().emission;

        thunderTimer = Random.Range(3, 6);

        sdManager.WakeUp();
    }

    void Update()
    {
        if (currentState == GameState.Inicio)
        {
            if (!titleAnimationOver)
            {
                entranceTimer += Time.deltaTime;

                if (!titlePreambleDone && entranceTimer >= entranceWarmUp + entrancePreambleDelay * (iterationsDone + 1))
                {
                    DoTheThunder();
                    iterationsDone += 1;

                    if (iterationsDone == entranceIterations)
                        titlePreambleDone = true;
                }
                else if (!titleShows && entranceTimer >= entranceWarmUp + entrancePreambleDelay * entranceIterations + entranceDelay)
                {
                    setUpFlash = true;
                    Vector3 thunderPoint = new Vector3(Random.Range(-Screen.width / 4.75f, Screen.width / 4.75f), Screen.height / 2, 0);
                    DoTheThunder(Random.Range(0, 4), 1, thunderPoint);
                    titleShows = true;
                }
                else if (title.activeSelf && entranceTimer >= entranceWarmUp + entrancePreambleDelay * entranceIterations + entranceDelay + entrancePressKeyDelay)
                {
                    entranceTimer += Time.deltaTime;
                    credits.GetComponent<Animator>().SetTrigger("FadeIn");
                    textPressAnyKey.GetComponent<Animator>().SetTrigger("FadeIn");
                    textPressAnyKey.GetComponent<Animator>().SetTrigger("IdleOn");
                    titleAnimationOver = true;
                }
            }
            else if (titleAnimationOver)
            {
                if (Input.anyKeyDown && !endedStart)
                {
                    anim.SetTrigger("FimInicio");
                    endedStart = true;
                }

                this.CheckForThunder();
            }
        }
        else if (currentState == GameState.Historia)
        {
            if (tManager.historiaCompleta)
            {
                if (tManager.parteHistoria == 3)
                {
                    barcoInteiro.SetActive(false);
                    barcoQuebrado.SetActive(true);
                    textBox.SetActive(false);
                    telaPreta.GetComponent<Animator>().SetTrigger("FadeOut");
                    anim.SetTrigger("Fim");
                    tManager.historiaCompleta = true;
                    currentState = GameState.Transicao;
                }
                else
                {
                    anim.SetTrigger("Troca");
                    textBox.SetActive(false);
                    currentState = GameState.Transicao;
                }
            }
            else if (!chamaHistoria)
            {

                if (tManager.parteHistoria < 3) sdManager.PlayOverlay(tManager.parteHistoria);
                else sdManager.PlayOverlay(3, true);

                if (firstTime) {
                    if (firstTimeTimer >= firstTimePreamblePreambleDelay)
                    {
                        if (firstTimeIterations == firstTimeIterationsDone)
                            firstTime = false;
                        else
                            DoTheThunder();

                        firstTimeTimer = 0;
                        firstTimeIterationsDone += 1;
                    }
                    else
                        firstTimeTimer += Time.deltaTime;

                    return;
                }
                
                StartCoroutine(tManager.Type());
                textBox.SetActive(true);
                chamaHistoria = true;

                if (tManager.parteHistoria == 3)
                    telaPreta.GetComponent<Animator>().SetTrigger("FadeIn");
            }
        }
        else if (currentState == GameState.GamePlay)
        {
            tManager.historiaCompleta = false;
            chamaHistoria = false;
        
            if (!kManager.gamePlaySucess)
                kManager.TryCheat();
            else
            {
                if (afterGameTimer > afterGamePlayDelay)
                {
                    afterGameTimer = 0;
                    tManager.parteHistoria += 1;
                    currentState = GameState.Historia;
                    kManager.gamePlaySucess = false;

                    this.SetBubbles(tManager.parteHistoria * 2);
                }
                else
                {
                    afterGameTimer += Time.deltaTime;

                    if (tManager.parteHistoria == 0)
                            backGround1.GetComponent<Animator>().SetTrigger("FadeIn");
                    else if (tManager.parteHistoria == 1)
                            backGround2.GetComponent<Animator>().SetTrigger("FadeIn");
                }
            }    
        }
        else if (currentState == GameState.GameEnd)
        {
            if (endGameTimer > endGameDelay)
            {
                currentState = GameState.GameOver;
                title.GetComponent<Animator>().speed = 0.5f;
                credits.GetComponent<Animator>().speed = 0.5f;
                title.GetComponent<Animator>().SetTrigger("FadeIn");
                credits.GetComponent<Animator>().SetTrigger("FadeIn");
                this.SetBubbles(1);
            }
            else
                endGameTimer += Time.deltaTime;
        }
    }

    private void SetBubbles(int intensity)
    {
        ParticleSystem.MinMaxCurve newCurve = bubbles.rateOverTime;
        newCurve.constantMin = 0.8f * (intensity);
        newCurve.constantMax = intensity;
        bubbles.rateOverTime = newCurve;
    }

    private void CheckForThunder()
    {
        if (thunderTimer >= thunderInterval)
        {
            thunderTimer = Random.Range(0, 4);
            DoTheThunder();
        }
        else
        {
            thunderTimer += Time.deltaTime;
        }
    }

    private void DoTheThunder(int thunderIndex = -1, int orientation = 0, Vector3 point = new Vector3())
    {
        if (thunderIndex < 0)
            thunderIndex = Random.Range(0, spritesRaio.Length);

        if (point == new Vector3())
            point = new Vector3(Random.Range(-Screen.width / 3.75f, Screen.width / 3.75f), Screen.height / 2, 0);

        if (orientation == 0)
            orientation = Random.Range(0, 2) * 2 - 1;

        raio.GetComponent<SpriteRenderer>().sprite = spritesRaio[thunderIndex];
        raio.transform.localPosition = point;
        raio.transform.localScale = new Vector3(orientation, raio.transform.localScale.y, raio.transform.localScale.z);
        raio.GetComponent<Animation>().Play();
        flash.GetComponent<Animation>().Play();
        sdManager.RandomizeSfx(raioClip);
    }

    public void DoWhileFlash()
    {
        if (setUpFlash)
        {
            title.SetActive(true);
            setUpFlash = false;
        }
    }

    public void Inicio()
    {
        title.GetComponent<Animator>().SetTrigger("FadeOut");
        credits.GetComponent<Animator>().SetTrigger("FadeOut");
        textPressAnyKey.GetComponent<Animator>().ResetTrigger("IdleOn");
        textPressAnyKey.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    public void FimInicio()
    {
        currentState = GameState.Historia;
    }

    public void Troca()
    {
        this.DoTheThunder();
    }

    public void AcabouTroca()
    {
        currentState = GameState.GamePlay;
        kManager.SpawnArrows();
    }

    public void FimBarco()
    {
        barcoQuebrado.GetComponent<Animation>().Play();
    }

    public void FimKraken()
    {
        sdManager.RandomizeSfx(krakenClip);
        kraken.SetActive(true);
        krakenFadeA.GetComponent<Animation>().Play();
        krakenFadeB.GetComponent<Animation>().Play();
    }
    
    public void FimFade()
    {
        telaPreta.GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public void FimFinal()
    {
        currentState = GameState.GameEnd;
        kraken.SetActive(false);
        barcoQuebrado.SetActive(false);
        backGround1.SetActive(false);
        backGround2.SetActive(false);
        telaPreta.GetComponent<Animator>().SetTrigger("FadeOut");
    }
}
