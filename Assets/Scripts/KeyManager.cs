using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class KeyManager : MonoBehaviour
{

    public List<Cheats[]> cheatsToKick;
    public float sequenceDelay = 2f, holdExpectedTime = 3f, manyExpectedTime = 3f;
    public int manyExpectedPresses = 10;

    public Sprite cursorE, cursorP, cursorS;
    public GameObject cursorPrefab;
    public GameObject[] setas;
    public bool gamePlaySucess = false;
    
    private Vector2 spawnPointInitial, spawnPointAdjusted;
    private List<GameObject> setasSpawnadas, cursoresSpawnados;
    private GameObject setaClone, cursorClone, arrowSpot, arrowText;

    private float delayTimer = 0, holdTimer = 0, manyTimer = 0;
    private int currentCheatList = 0, currentCheatListIndex = 0, currentCodePointer = 0, quantidadeSetas = 0, pressedTimes = 0;

    private void Start()
    {
        setasSpawnadas = new List<GameObject>();
        cursoresSpawnados = new List<GameObject>();
        arrowSpot = GameObject.Find("ArrowSpot");
        arrowText = GameObject.Find("ArrowText");

        this.PopulateCheats();
    }

    public void TryCheat()
    {
        UpdateArrows();
        KeyCode[] currentCode = cheatsToKick[currentCheatList][currentCheatListIndex].ReturnCode();
        KeyCode expectedKey = currentCode[currentCodePointer];

        switch (cheatsToKick[currentCheatList][currentCheatListIndex].Type)
        {
            case Cheats.CheatType.Sequence:
                delayTimer += Time.deltaTime;
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown(expectedKey))
                    {
                        currentCodePointer++;
                        delayTimer = 0f;
                    }
                    else
                    {
                        currentCodePointer = 0;
                    }
                        
                } else if (delayTimer > sequenceDelay)
                {
                    currentCodePointer = 0;
                    delayTimer = 0f;
                }

                if (currentCodePointer == currentCode.Length)
                {
                    currentCodePointer = 0;
                    delayTimer = 0f;
                    Cheated();
                }
                break;

            case Cheats.CheatType.Hold:
                if (Input.GetKey(expectedKey))
                {
                    holdTimer += Time.deltaTime;
                }
                else
                {
                    holdTimer = 0f;
                }

                if (holdTimer >= holdExpectedTime)
                {
                    holdTimer = 0f;
                    Cheated();
                }
                break;
            case Cheats.CheatType.Many:
                manyTimer += Time.deltaTime;

                if (manyTimer <= manyExpectedTime)
                {
                    if (Input.GetKeyDown(expectedKey)) pressedTimes++;
                }
                else
                {
                    manyTimer = 0;
                    pressedTimes = 0;
                }

                if (pressedTimes >= manyExpectedPresses)
                {
                    pressedTimes = 0;
                    Cheated();
                }
                break;
            default:
                break;
        }
    }

    public void SpawnArrows()
    {
        int arrowIndex = 0;
        KeyCode[] currentCode = cheatsToKick[currentCheatList][currentCheatListIndex].ReturnCode();
        spawnPointInitial = new Vector2(0 - currentCode.Length / 2 * 0.20f, 0);

        switch (cheatsToKick[currentCheatList][currentCheatListIndex].Type)
        {
            case Cheats.CheatType.Sequence:
                arrowText.GetComponent<TextMeshProUGUI>().text = "\"... follow ...\"";
                break;
            case Cheats.CheatType.Hold:
                arrowText.GetComponent<TextMeshProUGUI>().text = "\"... hold strong ...\"";
                break;
            case Cheats.CheatType.Many:
                arrowText.GetComponent<TextMeshProUGUI>().text = "\"... be fast ...\"";
                break;
            default:
                break;
        }


        foreach (KeyCode cheatCode in currentCode)
        {
            switch (cheatCode)
            {
                case KeyCode.UpArrow:
                    arrowIndex = 0;
                    break;
                case KeyCode.DownArrow:
                    arrowIndex = 1;
                    break;
                case KeyCode.LeftArrow:
                    arrowIndex = 2;
                    break;
                case KeyCode.RightArrow:
                    arrowIndex = 3;
                    break;
                default:
                    break;
            }

            spawnPointAdjusted = new Vector2(spawnPointInitial.x + (quantidadeSetas * 0.2f), 0);

            setaClone = Instantiate(setas[arrowIndex], spawnPointAdjusted, Quaternion.identity, arrowSpot.transform);
            setaClone.transform.localPosition = new Vector2(setaClone.transform.localPosition.x, 0);

            if (cheatsToKick[currentCheatList][currentCheatListIndex].Type == Cheats.CheatType.Hold) {
                setaClone.transform.Find("hold").GetComponent<Animator>().enabled = true;
            } else if (cheatsToKick[currentCheatList][currentCheatListIndex].Type == Cheats.CheatType.Many)
            {
                setaClone.transform.Find("many").GetComponent<Animator>().enabled = true;
            }
                

            cursorClone = Instantiate(cursorPrefab, spawnPointAdjusted, Quaternion.identity, arrowSpot.transform);
            cursorClone.transform.localPosition = new Vector2(cursorClone.transform.localPosition.x, -35);

            setasSpawnadas.Add(setaClone);
            cursoresSpawnados.Add(cursorClone);

            quantidadeSetas++;
        }
    }

    public void UpdateArrows()
    {
        for (int i = 0; i < cursoresSpawnados.Count; i++)
        {
            if (i > currentCodePointer)
                cursoresSpawnados[i].GetComponent<SpriteRenderer>().sprite = cursorE;
            else if (i == currentCodePointer)
                cursoresSpawnados[i].GetComponent<SpriteRenderer>().sprite = cursorS;
            else
                cursoresSpawnados[i].GetComponent<SpriteRenderer>().sprite = cursorP;
        }
    }

    public void Cheated()
    {
        foreach (GameObject seta in setasSpawnadas) Destroy(seta.gameObject);
        foreach (GameObject cursor in cursoresSpawnados) Destroy(cursor.gameObject);

        quantidadeSetas = 0;
        setasSpawnadas.Clear();
        cursoresSpawnados.Clear();

        if (currentCheatListIndex + 1 == cheatsToKick[currentCheatList].Length) {
            arrowText.GetComponent<TextMeshProUGUI>().text = "";
            gamePlaySucess = true;
            currentCheatList += 1;
            currentCheatListIndex = 0;
        } else {
            currentCheatListIndex += 1;
            SpawnArrows();
        }
    }

    private void PopulateCheats()
    {
        int randomMarker1 = 0, randomMarker2 = 0;
        cheatsToKick = new List<Cheats[]>();
        for (int i = 0; i < 3; i++)
        {
            Cheats[] cheatSequence = new Cheats[3 + (i * 2)];
            for (int j = 0; j < 3 + (i * 2); j++) cheatSequence[j] = new CheatCodeSequence();

            cheatsToKick.Add(cheatSequence);
        }

        randomMarker1 = Random.Range(0, 3);
        cheatsToKick[0][randomMarker1] = new CheatCodeHold();

        randomMarker1 = Random.Range(0, 5);
        cheatsToKick[1][randomMarker1] = new CheatCodeMany();

        randomMarker1 = Random.Range(0, 7);
        do { randomMarker2 = Random.Range(0, 7); } while (randomMarker1 == randomMarker2);
        
        cheatsToKick[2][randomMarker1] = new CheatCodeHold();
        cheatsToKick[2][randomMarker2] = new CheatCodeMany();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3 + (i * 2); j++)
            {
                Cheats cheat = cheatsToKick[i][j];

                if (cheat.Type != Cheats.CheatType.Sequence)
                    cheat.CreateCheat();
                else
                    cheat.CreateCheat(3 + (i * 2));
            }
        }
    }

    public abstract class Cheats
    {
        public enum CheatType { Sequence, Hold, Many };
        private readonly KeyCode[] arrows = { KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow };
        public abstract CheatType Type { get; }

        public KeyCode returnRandomArrow() {
            return arrows[Random.Range(0, 4)];
        }

        abstract public void CreateCheat(int size = 1);
        abstract public KeyCode[] ReturnCode();
    }

    public class CheatCodeSequence : Cheats
    {
        public KeyCode[] cheatCode;

        public override CheatType Type
        {
            get
            {
                return CheatType.Sequence;
            }
        }

        public override void CreateCheat(int size = 1)
        {
            cheatCode = new KeyCode[size];
            
            for (int i = 0; i < size; i++)
                cheatCode[i] = this.returnRandomArrow();
        }

        public override KeyCode[] ReturnCode()
        {
            return cheatCode;
        }
    }

    public class CheatCodeHold : Cheats
    {
        public KeyCode cheatCode;

        public override CheatType Type
        {
            get
            {
                return CheatType.Hold;
            }
        }

        public override void CreateCheat(int size = 1)
        {
            cheatCode = returnRandomArrow();
        }

        public override KeyCode[] ReturnCode()
        {
            KeyCode[] code = new KeyCode[1] { cheatCode };
            return code;
        }
    }

    public class CheatCodeMany : Cheats
    {
        public KeyCode cheatCode;

        public override CheatType Type
        {
            get
            {
                return CheatType.Many;
            }
        }

        public override void CreateCheat(int size = 1)
        {
            cheatCode = returnRandomArrow();
        }

        public override KeyCode[] ReturnCode()
        {
            KeyCode[] code = new KeyCode[1] { cheatCode };
            return code;
        }
    }
    
}