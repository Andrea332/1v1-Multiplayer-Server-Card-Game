using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ParameterComponent<T> : MonoBehaviour
    {
        public T parameter;

        public T Parameter
        {
            get { return parameter; }
            set
            {
                parameter = value;
                onParameterChanged?.Invoke(parameter);
            }
        }

        public T DefaultParameter
        {
            get; 
            set;
        }

        public UnityEvent<T> onParameterChanged;

        public void ResetParameter()
        {
            Parameter = DefaultParameter;
        }
    }
}
