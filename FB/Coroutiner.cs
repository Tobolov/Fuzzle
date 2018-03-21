using UnityEngine;
using System.Collections;

public class Coroutiner {
    public static Coroutine StartCoroutine(IEnumerator iterationResult) {
        GameObject routineHandlerGo = new GameObject("Coroutiner");
        CoroutinerInstance routineHandler = routineHandlerGo.AddComponent(typeof(CoroutinerInstance)) as CoroutinerInstance;
        return routineHandler.ProcessWork(iterationResult);
    }
}

public class CoroutinerInstance : MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(this);
    }

    public Coroutine ProcessWork(IEnumerator iterationResult) {
        return StartCoroutine(DestroyWhenComplete(iterationResult));
    }

    public IEnumerator DestroyWhenComplete(IEnumerator iterationResult) {
        yield return StartCoroutine(iterationResult);
        Destroy(gameObject);
    }
}