using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace TasksManager
{
    public class TasksRunner : MonoBehaviour
    {
        public List<UniTask> taskList;
        public UnityEvent OnBeforeTasksRunned;
        public bool executeTasksOnStart;
        public List<MonoBehaviour> monoBehavioursTasks;
        public List<ScriptableObject> scriptableObjectTasks;
        public UnityEvent OnTasksRunned;

        private void Start()
        {
            if(!executeTasksOnStart) return;
            ExecuteTasks();
        }

        public void ExecuteTasks()
        {
            OnBeforeTasksRunned?.Invoke();
            taskList = new List<UniTask>();
            FillTasksQueue(monoBehavioursTasks);
            FillTasksQueue(scriptableObjectTasks);
            RunTasks();
        }

        public void FillTasksQueue<T>(List<T> listOfTask)
        {
            foreach (var task in listOfTask)
            {
                if(task is ITasksRunnerAble mainAsyncable)
                {
                    taskList.Add(mainAsyncable.MainAsyncFunction());
                    continue;
                }
                
                Debug.LogWarning($"{task} does not implement IMainAsyncAble");
            }
        }

        private void RunTasks()
        {
            RunTasksAsync().Forget();
        }

        private async UniTaskVoid RunTasksAsync()
        {
            await UniTask.WhenAll(taskList);
            OnTasksRunned?.Invoke();
        }
    }
}
