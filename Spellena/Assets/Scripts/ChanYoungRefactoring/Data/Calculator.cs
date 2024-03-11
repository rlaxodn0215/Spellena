using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CalculatorSystem
{
    public class Calculator
    {
        static public float NormalizeAngle(float angle)
        {
            angle %= 360;
            if (angle > 180)
                angle -= 360;

            return angle;
        }
    }
}
