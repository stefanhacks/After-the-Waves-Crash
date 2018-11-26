using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadManager : MonoBehaviour {
    
    public float minTime, delay;
    public string sceneToLoad;
    public Animator loadingMarker;
    
    private bool switching = false, fired = false;
    private float timer = 0f;
    
	void Start () {
        loadingMarker.SetTrigger("FadeIn");
    }

	void Update () {
        timer += Time.deltaTime;

        if (timer > minTime && !switching)
        {
            timer = 0;
            switching = true;
            loadingMarker.SetTrigger("FadeOut");
        } else if (timer > delay && switching && !fired)
        {
            fired = true;
            StartCoroutine(loadGameScene(sceneToLoad));
        }
    }

    IEnumerator loadGameScene(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        while (!async.isDone)
        {
            yield return null;
        }
    }
}