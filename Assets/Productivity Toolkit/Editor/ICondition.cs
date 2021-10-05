// Anthony Ackermans
using System.Collections.Generic;
using UnityEngine;

namespace ToolExtensions
{
    public interface ICondition
    {

        void ShowUI();
        List<GameObject> Select();
        object GetValue();
        void ResetValues();
    }
}