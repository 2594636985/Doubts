using System;
using System.IO;

namespace Doubts.AoGraphic.Utils
{
    /// <summary>
    /// 参数检测类
    /// </summary>
    internal static class ArgChecker
    {
        /// <summary>
        /// 检测给定的条件不必须是真的
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void AssertIsTrue<TException>(bool condition, string message) where TException : Exception, new()
        {
            if (!condition)
            {
                throw (TException)Activator.CreateInstance(typeof(TException), message);
            }
        }

        /// <summary>
        /// 检测参数不为NULL
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        public static void AssertArgNotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// 检测参数不为IntPtr.Zero
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        public static void AssertArgNotNull(IntPtr arg, string argName)
        {
            if (arg == IntPtr.Zero)
            {
                throw new ArgumentException("IntPtr argument cannot be Zero", argName);
            }
        }

        /// <summary>
        /// 检测参数不为NULL或空白
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        public static void AssertArgNotNullOrEmpty(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentNullException(argName);
            }
        }


        /// <summary>
        /// 检测参数不为NULL同时是指定的T类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static T AssertArgOfType<T>(object arg, string argName)
        {
            AssertArgNotNull(arg, argName);

            if (arg is T)
            {
                return (T)arg;
            }
            throw new ArgumentException(string.Format("Given argument isn't of type '{0}'.", typeof(T).Name), argName);
        }

        /// <summary>
        /// 检测文件必须存在
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        public static void AssertFileExist(string arg, string argName)
        {
            AssertArgNotNullOrEmpty(arg, argName);

            if (false == File.Exists(arg))
            {
                throw new FileNotFoundException(string.Format("Given file in argument '{0}' not exist.", argName), arg);
            }
        }
    }
}
