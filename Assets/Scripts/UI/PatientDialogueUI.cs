
using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientDialogueUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text patientNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button examinationButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button treatButton;

    private string[] _dialogues;
    private int _dialogueIndex;
    private bool _isOpen;

    private Action _onExamination;
    private Action _onReject;
    private Action _onTreat;

    private InputController _input;

    private void Awake()
    {
        dialoguePanel.SetActive(false);

        nextButton.onClick.AddListener(NextDialogue);
        examinationButton.onClick.AddListener(OnExaminationClicked);
        rejectButton.onClick.AddListener(OnRejectClicked);
        treatButton.onClick.AddListener(OnTreatClicked);
    }

    private void Start()
    {
        _input = InputController.Instance;

        if (_input != null)
        {
            _input.CloseUIPressed += Close;
        }
    }

    public void OpenIntroduction(
        SO_PatientCaseData caseData,
        Action onExamination,
        Action onReject)
    {
        if (_isOpen || caseData == null)
        {
            return;
        }

        _onExamination = onExamination;
        _onReject = onReject;
        _onTreat = null;

        _dialogues = caseData.ExaminationDialogues;
        _dialogueIndex = 0;

        patientNameText.text = caseData.PatientName;

        HideButtons();
        Open();

        if (_dialogues == null || _dialogues.Length == 0)
        {
            dialogueText.text = "Hasta bir şikayet belirtmedi.";
            ShowIntroductionOptions();
            return;
        }

        dialogueText.text = _dialogues[0];
        nextButton.gameObject.SetActive(true);
    }

    public void OpenExaminationDecision(
        SO_PatientCaseData caseData,
        Action onTreat,
        Action onReject)
    {
        if (_isOpen || caseData == null)
        {
            return;
        }

        _onExamination = null;
        _onTreat = onTreat;
        _onReject = onReject;

        patientNameText.text = caseData.PatientName;
        dialogueText.text = "Muayene sonucuna göre kararını ver.";

        HideButtons();

        treatButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);

        Open();
    }

    private void NextDialogue()
    {
        if (!_isOpen || _dialogues == null)
        {
            return;
        }

        _dialogueIndex++;

        if (_dialogueIndex >= _dialogues.Length)
        {
            ShowIntroductionOptions();
            return;
        }

        dialogueText.text = _dialogues[_dialogueIndex];
    }

    private void ShowIntroductionOptions()
    {
        nextButton.gameObject.SetActive(false);

        examinationButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
    }

    private void HideButtons()
    {
        nextButton.gameObject.SetActive(false);
        examinationButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
        treatButton.gameObject.SetActive(false);
    }

    private void OnExaminationClicked()
    {
        Action action = _onExamination;

        Close();
        action?.Invoke();
    }

    private void OnRejectClicked()
    {
        Action action = _onReject;

        Close();
        action?.Invoke();
    }

    private void OnTreatClicked()
    {
        Action action = _onTreat;

        Close();
        action?.Invoke();
    }

    private void Open()
    {
        _isOpen = true;
        dialoguePanel.SetActive(true);

        if (_input != null)
        {
            _input.EnterUIMode();
        }
    }

    public void Close()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        dialoguePanel.SetActive(false);

        _onExamination = null;
        _onReject = null;
        _onTreat = null;

        if (_input != null)
        {
            _input.ExitUIMode();
        }
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.CloseUIPressed -= Close;
        }

        nextButton.onClick.RemoveListener(NextDialogue);
        examinationButton.onClick.RemoveListener(OnExaminationClicked);
        rejectButton.onClick.RemoveListener(OnRejectClicked);
        treatButton.onClick.RemoveListener(OnTreatClicked);
    }
}
