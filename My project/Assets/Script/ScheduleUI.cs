using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleUI : MonoBehaviour
{
    public Transform schedulePanel; // UI???
    public GameObject periodPrefab; // ??????UI???
    public Text dayText;

    void Start()
    {
        // ????????
        ClassScheduleManager.OnPeriodChanged += UpdateScheduleUI;
        UpdateScheduleUI(ClassScheduleManager.Instance.currentEvent);
    }

    void UpdateScheduleUI(ClassEvent currentEvent)
    {
        // ??????
        if (dayText != null)
            dayText.text = $"? {ClassScheduleManager.Instance.currentDay} ?";

        // ??????
        foreach (Transform child in schedulePanel)
        {
            Destroy(child.gameObject);
        }

        // ??????????
        List<ClassEvent> today = ClassScheduleManager.Instance.GetTodaySchedule();
        for (int i = 0; i < today.Count; i++)
        {
            GameObject periodUI = Instantiate(periodPrefab, schedulePanel);
            Text periodText = periodUI.GetComponentInChildren<Text>();

            if (periodText != null)
            {
                string prefix = (i == ClassScheduleManager.Instance.currentTimeSlot) ? "> " : "";
                periodText.text = $"{prefix}{today[i].timeSlotName}: {today[i].className}";

                // ??????
                if (i == ClassScheduleManager.Instance.currentTimeSlot)
                {
                    periodText.color = Color.green;
                }
                else if (today[i].isSpecial)
                {
                    periodText.color = Color.yellow;
                }
            }
        }
    }

    void OnDestroy()
    {
        // ????????
        ClassScheduleManager.OnPeriodChanged -= UpdateScheduleUI;
    }
}