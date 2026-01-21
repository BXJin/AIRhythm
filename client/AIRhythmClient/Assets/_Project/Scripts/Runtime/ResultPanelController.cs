using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultPanelController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject resultPanelRoot;

    [Header("Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text accText;
    [SerializeField] private TMP_Text maxComboText;

    public void Hide()
    {
        if (resultPanelRoot != null)
            resultPanelRoot.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void UpdateResult(int score, float acc01, int maxCombo)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (accText != null) accText.text = $"Acc: {(acc01 * 100f):0.0}%";
        if (maxComboText != null) maxComboText.text = $"MaxCombo: {maxCombo}";
    }

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
