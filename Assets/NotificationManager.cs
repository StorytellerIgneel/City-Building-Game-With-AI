using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance; //singleton
    public GameObject notificationPanel; //prefab for the notification
    public TextMeshProUGUI notificationText; //text component to display the notification
    
    public int blinkCount = 3; //number of times to blink the notification

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //do not destroy this object when loading a new scene
        }
        else
        {
            Destroy(gameObject); //if instance already exists, destroy this object
        }
    }
    public void ShowNotification(string Message, float duration, Boolean blink)
    {
        int i = 0; //initialize counter for blinking
        notificationText.text = Message; //set the text of the notification
        if (blink)
        {
            while (i < blinkCount) //check if the counter is less than the blink count
            {
                i++; //increment the counter
                notificationPanel.SetActive(!notificationPanel.activeSelf); //toggle the notification panel visibility
                StartCoroutine(HideNotificationAfterSeconds(duration)); //start the coroutine to blink the notification
            }
        }
        else
        {
            notificationPanel.SetActive(!notificationPanel.activeSelf); //show the notification panel 
            StartCoroutine(HideNotificationAfterSeconds(duration)); //start the coroutine to blink the notification
        }
       
    }

    private IEnumerator HideNotificationAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds); //wait for the specified seconds
        notificationPanel.SetActive(!notificationPanel.activeSelf); //hide the notification
    }
}