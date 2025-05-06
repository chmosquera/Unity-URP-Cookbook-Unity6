using System.Threading.Tasks;
using UnityEngine;

public class SimpleAsync : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          BeginProcess(); 
    }
    
    async Task BeginProcess() {
        Debug.Log($"[Frame: {Time.frameCount}] Begin processing");
        
        int[] items = new int [] {1, 2, 3, 4, 5};

        LogWhileWaiting("Waiting until 10 frames");
        
        foreach (int item in items)
        {
            await ProcessItem(item);
        }
        Debug.Log($"[Frame: {Time.frameCount}] Done processing");
    }

    async Task ProcessItem(int item)
    {
        Debug.Log($"[Frame: {Time.frameCount}] Processing item: {item}");
        await Task.Delay(100);
        Debug.Log($"[Frame: {Time.frameCount}] Done processing item: {item}");
    }

    async void LogWhileWaiting(string label)
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"[Frame: {Time.frameCount}] Log while waiting: {label}");
            await Task.Yield();
        }
    }
}
