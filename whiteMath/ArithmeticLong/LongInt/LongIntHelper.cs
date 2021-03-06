﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using whiteMath.General;

using WhiteMath = whiteMath.WhiteMath<int, whiteMath.CalcInt>;
using System.Diagnostics.Contracts;

namespace whiteMath.ArithmeticLong
{
    public partial class LongInt<B> where B: IBase, new()
    {
        public static partial class Helper
        {
            /// <summary>
            /// Складывает два числа с заданным смещением.
            /// </summary>
            private static void SumPrivate(int BASE, IList<int> result, IList<int> op1, int shift1, IList<int> op2, int shift2)
            {
                int i;
                long tmp;
                int carry = 0;

                for (i = 0; i < result.Count; i++)
                {
                    tmp = carry;

                    if (i - shift1 >= 0 && i - shift1 < op1.Count)
                        tmp += op1[i - shift1];
                    if (i - shift2 >= 0 && i - shift2 < op2.Count)
                        tmp += op2[i - shift2];

                    result[i] = (int)(tmp % BASE);
                    carry = (int)(tmp / BASE);
                }

                return;
            }
            
            //---------------------------------------------------------------
            //---------------------NUMBER DIVISION---------------------------
            //---------------------------------------------------------------

            /// <summary>
            /// Performs the integral division of two long integer numbers.
            /// </summary>
            /// <param name="one">The first LongInt number.</param>
            /// <param name="two">The second LongInt number.</param>
            /// <returns>The result of integral division without the remainder.</returns>
            public static LongInt<B> Div(LongInt<B> one, LongInt<B> two)
            {
                Contract.Requires<ArgumentNullException>(one != null, "one");
                Contract.Requires<ArgumentNullException>(two != null, "two");

                Contract.Ensures(Contract.ForAll(Contract.Result<LongInt<B>>().Digits, x => (x >= 0)));

                LongInt<B> junk;                
                
                return 
                    Div(one, two, out junk);
            }

            /// <summary>
            /// Performs the division (with remainder) of two long integer numbers.
            /// </summary>
            /// <param name="one">The first LongInt number.</param>
            /// <param name="two">The second LongInt number.</param>
            /// <param name="remainder">The reference to contain the remainder.</param>
            /// <returns>The result of integral division.</returns>
            public static LongInt<B> Div(LongInt<B> one, LongInt<B> two, out LongInt<B> remainder)
            {
                Contract.Requires<ArgumentNullException>(one != null, "one");
                Contract.Requires<ArgumentNullException>(two != null, "two");

                Contract.Ensures(Contract.ForAll(Contract.Result<LongInt<B>>().Digits, x => (x >= 0)));

                // Проверка на тупые случаи - деление на большее число или деление на единичную цифру.

                if (one.Length < two.Length) 
                {
                    remainder = one.Clone() as LongInt<B>;
                    return new LongInt<B>();                                  // zero number should be returned
                }
                if (two.Length == 1)
                {
                    LongInt<B> result = new LongInt<B>(one.Length);
                    result.Digits.AddRange(new int[one.Length]);

                    int rem = LongIntegerMethods.DivideByInteger(LongInt<B>.BASE, result.Digits, one.Digits, two[0]);

                    // Разберемся с negativeness.

                    remainder = (LongInt<B>)rem;    // convert the result to LongInt...

                    result.Negative = one.Negative ^ two.Negative;
                    remainder.Negative = one.Negative;

                    result.DealWithZeroes();
                    remainder.DealWithZeroes();

                    return result;
                }

                LongInt<B> res = new LongInt<B>();
                res.Digits.AddRange(new int[one.Length - two.Length + 1]);

                remainder = new LongInt<B>(two.Length);

                // ----- big bada-boom!

                IList<int> remDigits = LongIntegerMethods.Div(LongInt<B>.BASE, res.Digits, one.Digits, two.Digits);
                
                // --------------------

                remainder.Digits.AddRange(remDigits);

                // deal with negativeness

                res.Negative = one.Negative ^ two.Negative;
                remainder.Negative = one.Negative;

                // deal with zeroes

                res.DealWithZeroes();
                remainder.DealWithZeroes();

                return res;
            }

            //---------------------------------------------------------------
            //---------------------NUMBER MULTIPLICATION---------------------
            //---------------------------------------------------------------

            /// <summary>
            /// Performs a simple, square-complex multiplication of two LongInt numbers.
            /// </summary>
            /// <param name="one"></param>
            /// <param name="two"></param>
            /// <returns></returns>
            public static LongInt<B> MultiplySimple(LongInt<B> one, LongInt<B> two)
            {
                // the resulting number can have double length.

                LongInt<B> res = new LongInt<B>(one.Length + two.Length);
                res.Digits.AddRange(new int[one.Length + two.Length]);
                res.Negative = one.Negative ^ two.Negative;

                // The big one

                LongIntegerMethods.MultiplySimple(LongInt<B>.BASE, res.Digits, one.Digits, two.Digits);

                // Cut the leading zeroes

                res.DealWithZeroes();
                return res;
            }

            // --------------------------------
            // -------- INTEGER SQRT ----------
            // --------------------------------

            // Надо вычислить статически,
            // является ли BASE полным квадратом.

            private static readonly int  BASE_SQRT_FLOOR     = WhiteMath<int, CalcInt>.SquareRootInteger(LongInt<B>.BASE, LongInt<B>.BASE);
            private static readonly bool BASE_IS_FULL_SQUARE = (BASE_SQRT_FLOOR * BASE_SQRT_FLOOR == LongInt<B>.BASE);
            
            /// <summary>
            /// Returns the integer part of the square root
            /// of the number.
            /// </summary>
            /// <remarks>This method works only for integer numeric types.</remarks>
            /// <param name="number">A strictly positive number for which the integer part of its square root is to be found.</param>
            /// <returns>The integer part of the square root of the <paramref name="number"/>.</returns>
            public static LongInt<B> SquareRootInteger(LongInt<B> number)
            {
                Contract.Requires<ArgumentNullException>(number != null, "number");
                Contract.Requires<ArgumentOutOfRangeException>(number > 0);

                LongInt<B> firstEstimate = number;

                // Имеет смысл находить лучшее, чем само число,
                // первое приближение, только в том случае,
                // если число имеет длину более 1.

                if (number.Length > 1)
                {
                    // Для курсача - доказать, что это хуже, чем то, что незакомментировано
                    //
                    // firstEstimate = LongInt<B>.CreatePowerOfBase((number.Length + 1) / 2);

                    if (!BASE_IS_FULL_SQUARE || number.Length % 2 == 0)
                        firstEstimate = LongInt<B>.CreatePowerOfBase((number.Length + 1) / 2);
                    else
                    {
                        firstEstimate = LongInt<B>.CreatePowerOfBase(number.Length / 2);
                        firstEstimate[firstEstimate.Length - 1] = LongInt<B>.Helper.BASE_SQRT_FLOOR;
                    }
                }

                return
                    WhiteMath<LongInt<B>, CalcLongInt<B>>.SquareRootInteger(number, firstEstimate);
            }

            // --------------------------------
            // -------- EXPONENTIATION --------
            // --------------------------------

            [Obsolete]
            public static LongInt<B> PowerIntegerModularSlow<B>(LongInt<B> number, LongInt<B> power, LongInt<B> modulus) where B : IBase, new()
            {
                Contract.Requires<ArgumentNullException>(number != null, "number");
                Contract.Requires<ArgumentNullException>(power != null, "power");
                Contract.Requires<ArgumentNullException>(modulus != null, "modulus");

                if (power == 0)
                    return 1;

                LongInt<B> res = number % modulus;

                for (LongInt<B> i = 1; i < power; ++i)
                    res = res * number % modulus;

                return res;
            }

            /// <summary>
            /// Performs fast modular exponentiation of a <c>LongInt</c> number modulo another number.
            /// </summary>
            /// <typeparam name="B">A type specifying the numeric base of <c>LongInt</c> digits.</typeparam>
            /// <param name="number">A <c>LongInt</c> number to be exponentiated.</param>
            /// <param name="power">A non-negative exponent.</param>
            /// <param name="modulus">The modulus of the operation.</param>
            /// <returns>The result of raising <paramref name="number"/> to power <paramref name="power"/> modulo <paramref name="modulus"/>.</returns>
            public static LongInt<B> PowerIntegerModular<B>(LongInt<B> number, ulong power, LongInt<B> modulus) where B : IBase, new()
            {
                Contract.Requires<ArgumentNullException>(number != null, "number");
                Contract.Requires<ArgumentNullException>(modulus != null, "modulus");

                LongInt<B> result = 1;                                  // результат возведения в степень
                
                while (power > 0)
                {
                    if ((power & 1) == 1)
                        result = (result * number) % modulus;

                    power >>= 1;
                    number = (number * number) % modulus;
                }

                return result;
            }

            /// <summary>
            /// Performs a fast exponentiation of a <c>LongInt</c> number modulo another number.
            /// </summary>
            /// <typeparam name="B">A type specifying the numeric base of exponentiated number.</typeparam>
            /// <typeparam name="T">A type specifying the numeric base of the exponent. It is very recommended that it be an integer power of two for performance reasons.</typeparam>
            /// <remarks>If <typeparamref name="T"/> <c>IBase</c> type stands for a base that is an integer power of two, the algorithm will speed up significantly.</remarks>
            /// <param name="number">A <c>LongInt</c> number to be exponentiated.</param>
            /// <param name="power">A non-negative exponent.</param>
            /// <param name="modulus">The modulus of the operation.</param>
            /// <returns>The result of raising <paramref name="number"/> to power <paramref name="power"/> modulo <paramref name="modulus"/>.</returns>
            public static LongInt<B> PowerIntegerModular<B, T>(LongInt<B> number, LongInt<T> power, LongInt<B> modulus) 
                where B : IBase, new()
                where T : IBase, new()
            {
                Contract.Requires<ArgumentNullException>(number != null, "number");
                Contract.Requires<ArgumentNullException>(power != null, "power");
                Contract.Requires<ArgumentNullException>(modulus != null, "modulus");
                
                Contract.Requires<ArgumentException>(modulus > 0, "The modulus should be a positive number.");
                Contract.Requires<ArgumentException>(!power.Negative, "The power should be a non-negativen number.");

                LongInt<B> result = 1;                                  // результат возведения в степень
                
                while (power > 0)
                {
                    if ((power[0] & 1) == 1)
                        result = (result * number) % modulus;

                    power >>= 1;
                    number = (number * number) % modulus;
                }

                return result;
            }

            // ----------------------------------------------------------------------------------------
            // ------------------------------------------- CHECKER METHODS ----------------------------
            // ----------------------------------------------------------------------------------------

            private static bool multiplyCheck(int BASE, IList<int> res, IList<int> op1, IList<int> op2)
            {
                LongInt<B> result = new LongInt<B>(BASE, res, false);
                LongInt<B> oper1 = new LongInt<B>(BASE, op1, false);
                LongInt<B> oper2 = new LongInt<B>(BASE, op2, false);

                if (oper1 * oper2 != result)
                {
                    // Console.WriteLine("MUL.FAIL: {0} * {1} != {2}", oper1, oper2, result);
                    return false;
                }

                return true;
            }

            private static bool sumCheck(int BASE, IList<int> res, IList<int> op1, IList<int> op2)
            {
                LongInt<B> result = new LongInt<B>(BASE, res, false);
                LongInt<B> oper1 = new LongInt<B>(BASE, op1, false);
                LongInt<B> oper2 = new LongInt<B>(BASE, op2, false);

                if (oper1 + oper2 != result)
                {
                    // Console.WriteLine("SUM.FAIL: {0} + {1} != {2}", oper1, oper2, result);
                    return false;
                }

                return true;
            }

            private static bool difCheck(int BASE, IList<int> res, IList<int> op1, IList<int> op2)
            {
                LongInt<B> result = new LongInt<B>(BASE, res, false);
                LongInt<B> oper1 = new LongInt<B>(BASE, op1, false);
                LongInt<B> oper2 = new LongInt<B>(BASE, op2, false);

                if (oper1 - oper2 != result)
                {
                    // Console.WriteLine("DIF.FAIL: {0} - {1} != {2}", oper1, oper2, result);
                    return false;
                }

                return true;
            }

            private static LongInt<B> atoi(int BASE, IList<int> arr)
            {
                return new LongInt<B>(BASE, arr, false);
            }
        }
    }
}
