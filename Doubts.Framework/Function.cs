using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework
{
    public abstract class Function<TObject>
    {
        /// <summary>
        /// 方法名
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 方法标签
        /// </summary>
        public string Label { set; get; }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract TObject Execute(params object[] args);

    }

    public abstract class Function : Function<object>
    {

    }
}
