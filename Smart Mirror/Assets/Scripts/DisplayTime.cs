using UnityEngine;
using UnityEngine.UI;
using System;

public class DisplayTime : MonoBehaviour
{
    [SerializeField]
    private Text displayTime;
    [SerializeField]
    private Text displayDate;

    private void Update()
    {
        DateTime time = DateTime.Now;
        string hour = LeadingZero(time.Hour);
        string minute = LeadingZero(time.Minute);

        displayTime.text = hour + ":" + minute;
        string temp = System.DateTime.Today.ToString("MM/dd/yyyy");
        displayDate.text = temp.ToString();
    }

    private string LeadingZero(int time)
    {
        return time.ToString().PadLeft(2, '0');
    }
}