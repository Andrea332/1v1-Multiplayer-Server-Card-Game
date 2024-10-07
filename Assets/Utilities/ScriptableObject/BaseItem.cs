using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public abstract class BaseItem<T> : ScriptableObject, IItemValueable<T>
    {
        public T itemValue;

        public T ItemValue => itemValue;
    }
    public interface IItemValueable<T>
    {
        T ItemValue { get; }
    }
}
