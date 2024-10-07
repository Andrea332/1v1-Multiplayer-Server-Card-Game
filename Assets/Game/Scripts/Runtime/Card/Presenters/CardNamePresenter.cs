using TMPro;
using UnityEngine;

namespace Game
{
    public class CardNamePresenter : ParameterPresenter<string>
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            textMeshProUGUI.text = parameter;
        }
    }
}
