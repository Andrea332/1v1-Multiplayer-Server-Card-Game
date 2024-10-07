using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class SimpleIntPresenter : ParameterPresenter<int>
    {
        [SerializeField] private TextMeshProUGUI textUI;
        public override void SetParameter(int parameter)
        {
            base.SetParameter(parameter);
            textUI.SetText(parameter.ToString());
        }
    }
}
