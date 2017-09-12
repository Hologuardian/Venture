﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.Src.SI
{
    public struct Credit : ISIUnit
    {
        public float value;

        public Credit(float f)
        { value = f; }

        public string Name()
        { return Literals.SI.Name.Credit; }

        public string Symbol()
        { return Literals.SI.Symbol.Credit; }

        public float Value()
        { return value; }

        public void Value(float f)
        { value = f; }

        public ISIUnit[] Decompose()
        { return new ISIUnit[] { this }; }

        public static implicit operator Credit(float f)
        { return new Credit(f); }

        public static explicit operator float(Credit c)
        { return c.value; }

        public static implicit operator string(Credit c)
        { return SIUnit.ToString(c); }
    }
}
