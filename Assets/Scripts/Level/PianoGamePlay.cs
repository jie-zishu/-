using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PianoGamePlay : MonoBehaviour
{
    [Header("Note Sequence")]
    [Tooltip("Indices of piano keys (0-9) the player must press in order." +
             "\n0=G低 1=A低 2=B低 3=C 4=D 5=E 6=F 7=G 8=A 9=B")]
    [SerializeField] private List<int> noteSequence = new List<int> { 3, 4, 5, 6, 7 };

    [Header("Key References")]
    [SerializeField] private PianoKey[] pianoKeys; // 10 keys, ordered

    [Header("UI")]
    [SerializeField] private Text progressText;
    [SerializeField] private GameObject scoreImage;
    [SerializeField] private Button finishButton;
    [SerializeField] private Button jumpButton;

    [Header("Events")]
    public UnityEvent onSequenceComplete;
    public UnityEvent onNoteCorrect;
    public UnityEvent onNoteWrong;
    public UnityEvent onFinishClicked;

    private int currentIndex;
    private bool isActive;
    private Image lastHighlightedKey;

    public int CurrentIndex => currentIndex;
    public int TotalNotes => noteSequence.Count;
    public bool IsComplete => currentIndex >= noteSequence.Count;

    private bool keysWired;

    private void Start()
    {
        if (progressText == null)
            CreateProgressText();
    }

    private void AutoFindUI()
    {
        var pianoUI = GameObject.Find("InterationPianoGameUI");
        if (pianoUI == null) return;

        if (scoreImage == null)
        {
            var si = pianoUI.transform.Find("曲谱Image");
            if (si != null) scoreImage = si.gameObject;
        }
        if (finishButton == null)
        {
            var fb = pianoUI.transform.Find("Finish");
            if (fb != null) finishButton = fb.GetComponent<Button>();
        }
        if (jumpButton == null)
        {
            var jb = pianoUI.transform.Find("Jump");
            if (jb != null) jumpButton = jb.GetComponent<Button>();
        }
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(SkipToComplete);
        }
    }

    public void SkipToComplete()
    {
        currentIndex = noteSequence.Count;
        isActive = false;
        ClearHighlight();
        UpdateProgressUI();
        if (scoreImage != null) scoreImage.SetActive(false);
        if (finishButton != null) finishButton.gameObject.SetActive(true);
        onSequenceComplete?.Invoke();
    }

    private void EnsureKeysWired()
    {
        if (keysWired) return;

        if (pianoKeys == null || pianoKeys.Length == 0)
            FindPianoKeys();

        if (pianoKeys != null)
        {
            for (int i = 0; i < pianoKeys.Length; i++)
            {
                if (pianoKeys[i] == null) continue;
                int keyIndex = i;
                pianoKeys[i].onKeyPressed.AddListener(() => OnKeyPressed(keyIndex));
            }
        }
        keysWired = true;
    }

    private void CreateProgressText()
    {
        var pianoUI = transform.parent; // this component should be under InterationPianoGameUI
        var go = new GameObject("ProgressText", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(pianoUI, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f); rt.anchoredPosition = new Vector2(0, -20);
        rt.sizeDelta = new Vector2(300, 40);
        progressText = go.GetComponent<Text>();
        progressText.fontSize = 24; progressText.color = Color.white;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.8f);
        ol.effectDistance = new Vector2(2, -2);
    }

    private void FindPianoKeys()
    {
        var names = new string[] {
            "Key (5)_Low","Key (6)_Low","Key (7)_Low",
            "Key (1)","Key (2)","Key (3)","Key (4)",
            "Key (5)","Key (6)","Key (7)"
        };
        pianoKeys = new PianoKey[10];
        for (int i = 0; i < names.Length; i++)
        {
            var go = GameObject.Find(names[i]);
            if (go != null) pianoKeys[i] = go.GetComponent<PianoKey>();
        }
    }

    public void StartGame()
    {
        AutoFindUI();
        EnsureKeysWired();
        currentIndex = 0;
        isActive = true;
        // Reset UI to playing state
        if (scoreImage != null) scoreImage.SetActive(true);
        if (finishButton != null) { finishButton.gameObject.SetActive(false); finishButton.onClick.RemoveAllListeners(); finishButton.onClick.AddListener(() => onFinishClicked?.Invoke()); }
        UpdateProgressUI();
        HighlightNextKey();
    }

    public void StopGame()
    {
        isActive = false;
        ClearHighlight();
    }

    private void OnKeyPressed(int keyIndex)
    {
        if (!isActive) return;
        if (currentIndex >= noteSequence.Count) return;

        int expected = noteSequence[currentIndex];

        if (keyIndex == expected)
        {
            // 弹正确
            currentIndex++;
            onNoteCorrect?.Invoke();

            if (currentIndex >= noteSequence.Count)
            {
                isActive = false;
                ClearHighlight();
                UpdateProgressUI();

                if (scoreImage != null) scoreImage.SetActive(false);
                if (finishButton != null) finishButton.gameObject.SetActive(true);

                onSequenceComplete?.Invoke();
            }
            else
            {
                UpdateProgressUI();
                HighlightNextKey();
            }
        }
        else
        {
            // 错键
            onNoteWrong?.Invoke();
            FlashWrongKey(keyIndex);
        }
    }

    private void HighlightNextKey()
    {
        ClearHighlight();
        if (currentIndex < noteSequence.Count)
        {
            int target = noteSequence[currentIndex];
            if (target >= 0 && target < pianoKeys.Length && pianoKeys[target] != null)
            {
                var img = pianoKeys[target].GetComponent<Image>();
                if (img != null)
                {
                    lastHighlightedKey = img;
                    var origColor = img.color;
                    StartCoroutine(HighlightPulse(img));
                }
            }
        }
    }

    private System.Collections.IEnumerator HighlightPulse(Image img)
    {
        Color orig = img.color;
        float t = 0f;
        while (lastHighlightedKey == img && isActive && currentIndex < noteSequence.Count)
        {
            t += Time.deltaTime * 3f;
            float a = 0.5f + Mathf.Sin(t) * 0.3f;
            img.color = new Color(0.6f, 1f, 0.6f, a);
            yield return null;
        }
        img.color = orig;
    }

    private void ClearHighlight()
    {
        lastHighlightedKey = null;
    }

    private void FlashWrongKey(int index)
    {
        if (index >= 0 && index < pianoKeys.Length && pianoKeys[index] != null)
        {
            var img = pianoKeys[index].GetComponent<Image>();
            if (img != null)
                StartCoroutine(FlashRed(img));
        }
    }

    private System.Collections.IEnumerator FlashRed(Image img)
    {
        Color orig = img.color;
        img.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        img.color = orig;
    }

    private void UpdateProgressUI()
    {
        if (progressText != null)
        {
            if (currentIndex >= noteSequence.Count)
                progressText.text = "演奏完成!";
            else
                progressText.text = "音符 " + (currentIndex + 1) + " / " + noteSequence.Count;
        }
    }

    public void ResetGame()
    {
        currentIndex = 0;
        isActive = false;
        ClearHighlight();
        UpdateProgressUI();
    }
}
