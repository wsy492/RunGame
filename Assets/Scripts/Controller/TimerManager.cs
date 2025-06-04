using System;
using System.Collections;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static TimerManager _instance;
    public static TimerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("TimerManager");
                _instance = go.AddComponent<TimerManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void StartTimer(float duration, Action callback)
    {
        StartCoroutine(TimerCoroutine(duration, callback));
    }

    private IEnumerator TimerCoroutine(float duration, Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }
}