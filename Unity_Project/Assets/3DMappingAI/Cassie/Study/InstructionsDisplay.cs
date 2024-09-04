using MappingAI;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using static UnityEngine.GraphicsBuffer;

public class InstructionsDisplay : MonoBehaviour
{
    public GameObject CalibrartionPosition;
    public Transform cameraTransform;

    public RectTransform rectTransform;
    public Text label;
    public Image image;

    private string mainText;

    private float countdownStartTime;
    private float timeLimit;
    private bool displayCountdown;
    private int counting;
    private Vector3 offset = new Vector3(0f, 0f, 0f);
    private InputController inputController;
    private new Camera camera;
    private bool countDownFinished = false;
    // Start is called before the first frame update
    void Start()
    {
        inputController = FindAnyObjectByType<InputController>();
        camera = cameraTransform.GetComponent<Camera>();
        rectTransform = GetComponentInChildren<RectTransform>();
        label = GetComponentInChildren<Text>();
        offset = new Vector3(0f, -0.3f, 0.1f);
        ResetLocation();

        label.color = Color.white;
        label.enabled = true;
        label.text = "";
        // Hide cheatsheets
        image.enabled = false;
        counting = 0;
    }

    private void ResetLocation()
    {
        rectTransform.gameObject.SetActive(false);
        rectTransform.LookAt(cameraTransform.position);
        // Keep the y position of the canvas constant
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 newPosition = cameraTransform.position + cameraForward * offset.z;
        newPosition.y = CalibrartionPosition.transform.position.y + offset.y;

        // Calculate the distance between the current position and the new position
        float distanceToNewPosition = Vector3.Distance(transform.position, newPosition);

        // Update the position only if the distance is greater than 1
        if (distanceToNewPosition > 0.08f)
        {
            // Set the position of the canvas
            transform.position = newPosition;
            Quaternion lookRotation = Quaternion.LookRotation(cameraForward.normalized);

            // Apply the rotation with alignment to the target's up direction
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
        rectTransform.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        counting++;
        if (counting % 20 == 0 && inputController.Idle())
        {
            ResetLocation();
        }

        if (displayCountdown)
            UpdateCountdown();
    }

    public void SetText(string text, bool modalMode = false)
    {
        mainText = text;
        label.text = text;
        label.color = Color.white;
        image.enabled = true;
        if (modalMode)
        {
            rectTransform.anchoredPosition3D = new Vector3(0f, 0.75f, 4f);
            rectTransform.LookAt(cameraTransform.position);
            label.fontSize = 18;
        }
        else
        {
            rectTransform.anchoredPosition3D = new Vector3(0f, 0.75f, 4f);
            rectTransform.LookAt(cameraTransform.position);
            label.fontSize = 18;
        }
    }
    public void SetCountdown(float timeLimit)
    {
        if (timeLimit != 0f)
        {
            countdownStartTime = Time.time;
            this.timeLimit = timeLimit;
            displayCountdown = true;
            countDownFinished = false;
        }
        else
        {
            displayCountdown = false;
        }

    }

    public void PauseCountdown()
    {
        displayCountdown = false;

    }

    public void UnpauseCountdown(float timeToIgnore)
    {
        if (timeLimit != 0f)
        {
            countdownStartTime += timeToIgnore;
            displayCountdown = true;
        }
    }

    private void UpdateCountdown()
    {
        float timeElapsed = (Time.time - countdownStartTime);
        float timeRemaining = Mathf.Max(0f, timeLimit - timeElapsed);


        TimeSpan t = TimeSpan.FromSeconds(timeRemaining);
        string instructions = mainText;
        instructions += "\n";
        instructions += "Time remaining: " + string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

        if (timeRemaining == 0f)
        {
            countDownFinished = true;
            instructions += "\n";
            instructions += "Time for this task is elapsed, please finish your drawing now and end the task.";
            label.color = Color.red;
        }

        label.text = instructions;
    }

    public bool CountDownFinished()
    {
        return countDownFinished;
    }
}
