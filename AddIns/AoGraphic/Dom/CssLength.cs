using System;
using System.Globalization;
using Hydra.AomiCss.Entities;
using Hydra.AomiCss.Parse;

namespace Hydra.AomiCss.Dom
{
    /// <summary>
    /// CSS长度类
    /// </summary>
    internal sealed class CssLength
    {
        private readonly float _number;
        private readonly bool _isRelative;
        private readonly CssUnit _unit;
        private readonly string _length;
        private readonly bool _isPercentage;
        private readonly bool _hasError;

        public CssLength(string length)
        {
            _length = length;
            _number = 0f;
            _unit = CssUnit.None;
            _isPercentage = false;

            //Return zero if no length specified, zero specified
            if (string.IsNullOrEmpty(length) || length == "0") return;

            //If percentage, use ParseNumber
            if (length.EndsWith("%"))
            {
                _number = CssValueParser.ParseNumber(length, 1);
                _isPercentage = true;
                return;
            }

            //If no units, has error
            if (length.Length < 3)
            {
                float.TryParse(length, out _number);
                _hasError = true;
                return;
            }

            //Get units of the length
            string u = length.Substring(length.Length - 2, 2);

            //Number of the length
            string number = length.Substring(0, length.Length - 2);

            //TODO: Units behave different in paper and in screen!
            switch (u)
            {
                case CssConstants.Em:
                    _unit = CssUnit.Ems;
                    _isRelative = true;
                    break;
                case CssConstants.Ex:
                    _unit = CssUnit.Ex;
                    _isRelative = true;
                    break;
                case CssConstants.Px:
                    _unit = CssUnit.Pixels;
                    _isRelative = true;
                    break;
                case CssConstants.Mm:
                    _unit = CssUnit.Milimeters;
                    break;
                case CssConstants.Cm:
                    _unit = CssUnit.Centimeters;
                    break;
                case CssConstants.In:
                    _unit = CssUnit.Inches;
                    break;
                case CssConstants.Pt:
                    _unit = CssUnit.Points;
                    break;
                case CssConstants.Pc:
                    _unit = CssUnit.Picas;
                    break;
                default:
                    _hasError = true;
                    return;
            }

            if (!float.TryParse(number, System.Globalization.NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _number))
            {
                _hasError = true;
            }

        }


        #region Props

        /// <summary>
        /// 获得长度的数字
        /// </summary>
        public float Number
        {
            get { return _number; }
        }

        /// <summary>
        /// 是否出错
        /// </summary>
        public bool HasError
        {
            get { return _hasError; }
        }


        /// <summary>
        /// 是否为百分比
        /// </summary>
        public bool IsPercentage
        {
            get { return _isPercentage; }
        }


        /// <summary>
        /// 是否为相对的
        /// </summary>
        public bool IsRelative
        {
            get { return _isRelative; }
        }

        /// <summary>
        /// 长度的单位
        /// </summary>
        public CssUnit Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// 原长度
        /// </summary>
        public string Length
        {
            get { return _length; }
        }


        #endregion

        #region Methods

        /// <summary>
        /// If length is in Ems, returns its value in points
        /// </summary>
        /// <param name="emSize">Em size factor to multiply</param>
        /// <returns>Points size of this em</returns>
        /// <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        public CssLength ConvertEmToPoints(float emSize)
        {
            if (HasError) throw new InvalidOperationException("Invalid length");
            if (Unit != CssUnit.Ems) throw new InvalidOperationException("Length is not in ems");

            return new CssLength(string.Format("{0}pt", Convert.ToSingle(Number * emSize).ToString("0.0", NumberFormatInfo.InvariantInfo)));
        }

        /// <summary>
        /// If length is in Ems, returns its value in pixels
        /// </summary>
        /// <param name="pixelFactor">Pixel size factor to multiply</param>
        /// <returns>Pixels size of this em</returns>
        /// <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        public CssLength ConvertEmToPixels(float pixelFactor)
        {
            if (HasError) throw new InvalidOperationException("Invalid length");
            if (Unit != CssUnit.Ems) throw new InvalidOperationException("Length is not in ems");

            return new CssLength(string.Format("{0}px", Convert.ToSingle(Number * pixelFactor).ToString("0.0", NumberFormatInfo.InvariantInfo)));
        }

        /// <summary>
        /// Returns the length formatted ready for CSS interpreting.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (HasError)
            {
                return string.Empty;
            }
            else if (IsPercentage)
            {
                return string.Format(NumberFormatInfo.InvariantInfo, "{0}%", Number);
            }
            else
            {
                string u = string.Empty;

                switch (Unit)
                {
                    case CssUnit.None:
                        break;
                    case CssUnit.Ems:
                        u = "em";
                        break;
                    case CssUnit.Pixels:
                        u = "px";
                        break;
                    case CssUnit.Ex:
                        u = "ex";
                        break;
                    case CssUnit.Inches:
                        u = "in";
                        break;
                    case CssUnit.Centimeters:
                        u = "cm";
                        break;
                    case CssUnit.Milimeters:
                        u = "mm";
                        break;
                    case CssUnit.Points:
                        u = "pt";
                        break;
                    case CssUnit.Picas:
                        u = "pc";
                        break;
                }

                return string.Format(NumberFormatInfo.InvariantInfo, "{0}{1}", Number, u);
            }
        }

        #endregion
    }
}