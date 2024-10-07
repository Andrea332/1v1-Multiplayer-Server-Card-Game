using TMPro;
using UnityEngine;

namespace Game
{
    public class MaxDeckablePresenter : ParameterPresenter<int>
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        public override void SetParameter(int parameter)
        {
            base.SetParameter(parameter);
            textMeshProUGUI.text = parameter.ToString();
        }
        
    }
}
