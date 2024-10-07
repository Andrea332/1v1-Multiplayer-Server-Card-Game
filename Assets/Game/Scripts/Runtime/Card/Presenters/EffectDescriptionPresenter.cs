using TMPro;
using UnityEngine;

namespace Game
{
    public class EffectDescriptionPresenter : ParameterPresenter<string>
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            textMeshProUGUI.text = parameter;
        }
    }
}
