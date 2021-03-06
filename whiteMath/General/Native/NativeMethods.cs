﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace whiteMath.General
{
    public static class NativeMethods
    {
        /// <summary>
        /// Returns the current value of CPU performance counter in ticks.
        /// The value is written to the variable passed.
        /// </summary>
        /// <param name="x">The variable to store the query result.</param>
        /// <returns>Negative value if the call is unsuccessfull, and a positive value in ticks otherwise.</returns>
        [DllImport("kernel32.dll")]
        public extern static int QueryPerformanceCounter(ref long x);

        /// <summary>
        /// Return the current CPU frequency, in ticks per second.
        /// The value is written to the variable passed.
        /// </summary>
        /// <param name="x">The variable to store the query result.</param>
        /// <returns>Negative value if the call is unsuccessful, and a positive value otherwise.</returns>
        [DllImport("kernel32.dll")]
        public extern static int QueryPerformanceFrequency(ref long x);

    }
}
