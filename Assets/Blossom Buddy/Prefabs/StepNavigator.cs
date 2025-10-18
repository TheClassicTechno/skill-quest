using UnityEngine;

public class StepNavigator : MonoBehaviour
{
    public GameObject[] steps;
    private int currentStep = 0;

    void Start()
    {
        for (int i = 0; i < steps.Length; i++)
            steps[i].SetActive(i == 0);
    }

    void Update()
    {
        // A Button → Next Step
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            GoToNextStep();

        // B Button → Previous Step
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            GoToPreviousStep();
    }

    void GoToNextStep()
    {
        if (currentStep < steps.Length - 1)
        {
            steps[currentStep].SetActive(false);
            currentStep++;
            steps[currentStep].SetActive(true);
            Debug.Log($"➡️ Moved to step {currentStep + 1}");
        }
        else
        {
            Debug.Log("✅ Final step reached!");
        }
    }

    void GoToPreviousStep()
    {
        if (currentStep > 0)
        {
            steps[currentStep].SetActive(false);
            currentStep--;
            steps[currentStep].SetActive(true);
            Debug.Log($"⬅️ Returned to step {currentStep + 1}");
        }
    }
}
