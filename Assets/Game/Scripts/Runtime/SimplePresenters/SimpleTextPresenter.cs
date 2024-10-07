using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class SimpleTextPresenter : ParameterPresenter<string>
    {
        [SerializeField] private TextMeshProUGUI textUI;
        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            textUI.SetText(parameter);
        }
    }
}
