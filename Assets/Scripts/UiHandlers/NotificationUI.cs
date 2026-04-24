using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text text;
    public GameObject panel;

    private Queue<string> queue = new Queue<string>();
    private bool isShowing = false;

    // public void Initialize(NotificationService service)
    // {
    //     service.OnNotify += HandleNotification;
    // }

    private void HandleNotification(string message)
    {
        queue.Enqueue(message);

        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            string msg = queue.Dequeue();

            panel.SetActive(true);
            text.text = msg;

            yield return new WaitForSeconds(2f);

            panel.SetActive(false);
        }

        isShowing = false;
    }
}