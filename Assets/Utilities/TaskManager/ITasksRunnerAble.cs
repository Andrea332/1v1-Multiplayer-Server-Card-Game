using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TasksManager
{

    public interface ITasksRunnerAble
    {
        public UniTask MainAsyncFunction();
    }
}
