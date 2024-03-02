using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 
    public int[] pointThresholds;
    public Sprite[] stageSprites;
    public Image[] mushroomImages;
    public GameObject timestonePrefab, restartPopup, tutorialPopup, winPopup;
    public Image timerImage, timestoneHolder, seedImage;
    public int roundsPerStage = 5;
    public float roundLength;

    public float pointTotal = 0;

    [SerializeField]
    AudioClip successAudio;
    [SerializeField]
    Color activeTimerColor, inactiveTimerColor;

    float timer;
    private int currStage = 0;
    public bool roundStarted = false;
    [SerializeField]
    private int roundsLeft;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        timer = roundLength;
        roundsLeft = roundsPerStage;
        
    }
    public void StartGame()
    {
        StartStage(0);
        tutorialPopup.SetActive(false);
    }
    public void StartRound()
    {
        roundStarted = true;
        roundsLeft--;
        foreach (Transform child in timestoneHolder.transform)
        {
            Destroy(child.gameObject);
            break;
        }
    }
    public void EndRound()
    {
        roundStarted = false;
        timer = roundLength;
        timerImage.color = inactiveTimerColor;
        timerImage.fillAmount = timer / roundLength;
        if(pointTotal > pointThresholds[currStage])
        {
            StartCoroutine(NextStage());
        }
        else if(roundsLeft == 0)
        {
            StartCoroutine(RestartGame());
        }

    }

    private IEnumerator NextStage()
    {
        seedImage.rectTransform.DOShakeScale(3f);
        AudioManager.Instance.clipSource.PlayOneShot(successAudio);

        yield return new WaitForSeconds(3f);
        StartStage(currStage + 1);
    }

    public void StartStage(int stageIndex)
    {
        if(stageIndex == 9)
        {
            winPopup.SetActive(true);
            return;
        }
        pointTotal = 0;
        currStage = stageIndex;
        TouchControls.Instance.isControlsActive = true;
        seedImage.sprite = stageSprites[stageIndex];
        foreach(Transform child in timestoneHolder.transform)
        {
            Destroy(child.gameObject);
        }
        roundsLeft = roundsPerStage;
        for(int i =0; i< 5; i++)
        {
            Instantiate(timestonePrefab, timestoneHolder.transform);
        }
        for(int i = 0; i< mushroomImages.Length; i++)
        {
            mushroomImages[i].fillAmount = 0;
            mushroomImages[i].gameObject.SetActive(currStage >= i);
        }
    }

    public void AddPoints(float amt)
    {
        pointTotal += amt;
        for (int i = 0; i < mushroomImages.Length; i++)
        {
            mushroomImages[i].fillAmount = pointTotal / pointThresholds[currStage];
        }
    }

    private void Update()
    {
        if (roundStarted)
        {
            timerImage.color = activeTimerColor;
            timer -= Time.deltaTime;
            timerImage.fillAmount = timer / roundLength;
            if (timer <= 0)
            {
                TouchControls.Instance.ForceDropOrb();
            }

        }
        
    }

    private IEnumerator RestartGame()
    {
        restartPopup.SetActive(true);
        TouchControls.Instance.isControlsActive = false;
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }

    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }

}
