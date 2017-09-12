﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.Src.SI
{
    public struct Pascal : ISIUnit
    {
        public float value;

        public Pascal(float f)
        { value = f; }

        public ISIUnit[] Decompose()
        { //TODO SIUnit Pascal implement Decompose()
            return new ISIUnit[] { }; }

        public string Name()
        { return Literals.SI.Name.Pascal; }

        public string Symbol()
        { return Literals.SI.Symbol.Pascal; }

        public float Value()
        { return value; }

        public void Value(float f)
        { value = f; }

        public static implicit operator Pascal(float f)
        { return new Pascal(f); }

        public static explicit operator float(Pascal Pa)
        { return Pa.value; }

        public static implicit operator string(Pascal Pa)
        { return SIUnit.ToString(Pa); }
    }
}
