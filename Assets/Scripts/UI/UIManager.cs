using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins;

/// <summary>
/// Well...it manages UI
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public PlayerAxisController player;
    public RectTransform canvas;
    public GameObject gameOverScreen;
    public GameObject succesfullDeliveryScreen;
    public GameObject menu;
    public GameObject timeToHidePanel;
    public GameObject deliveryUI;
    public GameObject timeToReadyScreen;
    public GameObject playerHiddenUI;
    public GameObject playerHiddenHereIconPrefab;
    public GameObject takeVehicleUI;
    public GameObject inVehicleUI;
    public GameObject safespaceCameraHint;
    public AbilityButton sprintButton;
    public GameObject blockClicksPanel;
    public GameObject skipFlyByButton;
    public GameObject hitByACarScreen;
    public GameObject gameCompleteScreen;
    public GameObject testAd;
    public GameObject dragPanel;
    public GameObject leftJoystick;

    public Text deliveryTimeLeftText;
    public Text deliveryTimeTitleText;
    public Text fineOrTipTextTitle;
    public Text fineOrTipText;
    public Text gameCompleteScreenLevelText;
    public Text loadedLevelText;

    private RectTransform playerHiddenIcon;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHitByACarScreenActive(bool active)
    {
        hitByACarScreen.SetActive(active);
    }

    public void ShowGameCompleteScreen()
    {
        gameCompleteScreen.SetActive(true);

        int level;

        if(PlayerPrefs.GetInt("levelBlockIsLoaded") == 1)
        {
            level = PlayerPrefs.GetInt("levelBlockLoaded");
        }
        else
        {
            level = PlayerPrefs.GetInt("level");
        }

        gameCompleteScreenLevelText.text = "Level " + level;
    }

    public void ShowCurrentLevel(int level)
    {
        loadedLevelText.gameObject.SetActive(true);
        loadedLevelText.text = "Level " + level + ".";

        Sequence sequence = DOTween.Sequence();
        sequence.Append(loadedLevelText.DOFade(0, 0));
        sequence.Append(loadedLevelText.DOFade(1, 1));
        sequence.AppendInterval(2f);
        sequence.Append(loadedLevelText.DOFade(0, 1));

        DOTween.Play(loadedLevelText);
    }

    public void SetPlayerControlsActive(bool active)
    {
        dragPanel.SetActive(active);
        leftJoystick.SetActive(active);
    }

    public void SetBlockClicksPanelActive(bool active)
    {
        blockClicksPanel.SetActive(active);
    }

    public void SetSafespaceHintActive(bool active)
    {
        safespaceCameraHint.SetActive(active);
    }

    public void SetSkipFlyByButtonActive(bool active)
    {
        skipFlyByButton.SetActive(active);
    }

    public void SprintButton()
    {
        player.Sprint();
    }

    public void ShowSuccesfullDeliveryScreen()
    {
        succesfullDeliveryScreen.SetActive(true);

        UIManager.Instance.SetPlayerControlsActive(false);
    }

    public void ShowPlayerHiddenHereIcon(Vector3 position)
    {
        StartCoroutine(UpdatePlayerHiddenHereIconPosition(position));
    }

    private IEnumerator UpdatePlayerHiddenHereIconPosition(Vector3 worldPosition)
    {
        yield return null;
        playerHiddenIcon = Instantiate(playerHiddenHereIconPrefab, Vector3.zero, 
        Quaternion.identity, canvas.transform).GetComponent<RectTransform>();
        playerHiddenIcon.rotation = canvas.rotation;


        while (playerHiddenIcon != null)
        {
            Vector3 positionWithOffset = worldPosition;
            positionWithOffset.y += 3.5f;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(positionWithOffset);
            screenPosition.x *= canvas.rect.width / (float)Camera.main.pixelWidth;
            screenPosition.y *= canvas.rect.height / (float)Camera.main.pixelHeight;
            playerHiddenIcon.anchoredPosition = screenPosition - canvas.sizeDelta / 2f;
            playerHiddenIcon.localPosition = new Vector3(playerHiddenIcon.localPosition.x, playerHiddenIcon.localPosition.y, 0);
            yield return null;
        }
    }

    public void ShowTakeVehicleUI()
    {
        takeVehicleUI.SetActive(true);
    }

    public void HideTakeVehicleUI()
    {
        takeVehicleUI.SetActive(false);
    }

    public void ShowInVehicleUI()
    {
        inVehicleUI.SetActive(true);
    }

    public void HideInVehicleUI()
    {
        inVehicleUI.SetActive(false);
    }

    public void HidePlayerHiddenHereIcon()
    {
        if (playerHiddenIcon != null)
            Destroy(playerHiddenIcon.gameObject);
    }

    public void ShowPlayerHiddenUI()
    {
        playerHiddenUI.SetActive(true);
    }

    public void HidePlayerHiddenUI()
    {
        playerHiddenUI.SetActive(false);
    }

    public void UnHidePlayerButton()
    {
        player.UnHide();
    }

    public void ShowDeliveryUI()
    {
        deliveryUI.SetActive(true);
    }

    public void HideDeliveryUI()
    {
        deliveryUI.SetActive(false);
    }

    public void UpdateDeliveryTimeLeftText(int seconds, string titleText = "")
    {
        deliveryTimeLeftText.text = Mathf.Abs(seconds).ToString();

        if (titleText != "")
        {
            deliveryTimeTitleText.text = titleText;
        }
    }

    public void ShowFineOrTipText(float value)
    {

        if (value < 0)
        {
            fineOrTipTextTitle.text = "Fine:";
            fineOrTipText.text = $"-{Mathf.Abs(Mathf.Round(value)).ToString()}$";
        }
        else if (value > 0)
        {
            fineOrTipTextTitle.text = "Tip:";
            fineOrTipText.text = $"+{Mathf.Abs(Mathf.Round(value)).ToString()}$";
        }
        else if (value == 0)
        {
            fineOrTipTextTitle.text = "No tips.";
            fineOrTipText.text = "";
        }
    }

    public void ShowTimeToReadyScreen()
    {
        timeToReadyScreen.SetActive(true);
    }

    public void HideTimeToReadyScreen()
    {
        timeToReadyScreen.SetActive(false);
    }

    public void HideSuccesfullDeliveryScreenButton()
    {
        succesfullDeliveryScreen.SetActive(false);

        if(GameManager.Instance.HasMoreDeliveriesToDo())
        {
            ShowDeliveryUI();
            GameManager.Instance.OnDeliveryContinued();
        }
        else
        {
            testAd.SetActive(true);
        }
    }

    public void ShowSkipFlyByButton()
    {
        skipFlyByButton.SetActive(true);
    }

    public void OnPlayerCatched()
    {
        gameOverScreen.SetActive(true);
    }

    public void ShowStartGamePanel()
    {
        menu.SetActive(true);
    }

    public void ShowTimeToHidePanel()
    {
        timeToHidePanel.SetActive(true);
    }

    public void HideTimeToHidePanel()
    {
        timeToHidePanel.SetActive(false);
    }
}