using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ConcurrentTasks : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BeginProcess();
    }

    async Task BeginProcess() {
        Debug.Log($"[Frame: {Time.frameCount}] Begin processing");
        
        int[] items = new int [] {1, 2, 3, 4, 5};

        // Create a list of tasks
        List<Task> tasks = new List<Task>();
        
        foreach (int item in items)
        {
            // Add each task. This doesn't fire off the tasks yet.
            tasks.Add(ProcessItem(item));
        }
        
        // Now, fire off all the tasks concurrently. Suspend this function until we finish all tasks.
        await Task.WhenAll(tasks);
        
        Debug.Log($"[Frame: {Time.frameCount}] Done processing");
    }

    async Task ProcessItem(int item)
    {
        Debug.Log($"[Frame: {Time.frameCount}] Processing item: {item}");
        int multiplier = Random.Range(1, 5);
        await Task.Delay(1000 * multiplier); // Tasks take different times to complete
        Debug.Log($"[Frame: {Time.frameCount}] Done processing item: {item}");
    }

}
