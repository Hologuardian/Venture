﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.Src.SI
{
    public class Watt : SIUnit
    {
        public override string Name()
        { return "watt"; }

        public override string Symbol()
        { return "W"; }

        public static WattHour operator *(Watt w, Hour h)
        { return new WattHour() { Value = w * h }; }

        public static Joule operator *(Watt w, Second s)
        { return new Joule() { Value = w * s }; }

        public static implicit operator Watt(float f)
        { return new Watt() { Value = f }; }
    }
}
