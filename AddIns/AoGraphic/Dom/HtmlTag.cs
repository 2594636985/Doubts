using System.Collections.Generic;
using Hydra.AomiCss.Utils;

namespace Hydra.AomiCss.Dom
{
    /// <summary>
    /// HTML节点类
    /// </summary>
    internal sealed class HtmlTag
    {
        /// <summary>
        /// 节点名字
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// 是否为节点
        /// </summary>
        private readonly bool _isSingle;

        /// <summary>
        /// 属性信合
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        public HtmlTag(string name, bool isSingle, Dictionary<string, string> attributes = null)
        {
            ArgChecker.AssertArgNotNullOrEmpty(name, "name");

            _name = name;
            _isSingle = isSingle;
            _attributes = attributes;
        }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// 属性集合
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// 单节点
        /// </summary>
        public bool IsSingle
        {
            get { return _isSingle; }
        }

        /// <summary>
        /// 是否有属性
        /// </summary>
        /// <returns></returns>
        public bool HasAttributes()
        {
            return _attributes != null && _attributes.Count > 0;
        }

        /// <summary>
        /// 是否有指定attribute
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool HasAttribute(string attribute)
        {
            return _attributes != null && _attributes.ContainsKey(attribute);
        }

        /// <summary>
        /// 获得指定的属性值，如量没有的话，就返回默认的值
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string TryGetAttribute(string attribute, string defaultValue = null)
        {
            return _attributes != null && _attributes.ContainsKey(attribute) ? _attributes[attribute] : defaultValue;
        }

        public override string ToString()
        {
            return string.Format("<{0}>", _name);
        }
    }
}