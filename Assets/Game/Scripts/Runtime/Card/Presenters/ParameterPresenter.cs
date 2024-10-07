using UnityEngine;

namespace Game
{
    public abstract class ParameterPresenter<T> : MonoBehaviour
    {
        private T parameter;

        public virtual void SetParameter(T parameter)
        {
            this.parameter = parameter;
        }
        
        public T GetParameter()
        {
            return parameter;
        }
    }
}
