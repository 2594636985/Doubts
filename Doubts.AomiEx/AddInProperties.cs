using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;

namespace Doubts.AomiEx
{
    public class AddInProperties
    {
        private AddInProperties parent;
        private object syncRoot;
        private Dictionary<string, object> dict = new Dictionary<string, object>();
        private bool isDirty;
   
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                lock (syncRoot)
                {
                    if (value)
                        MakeDirty();
                    else
                        CleanDirty();
                }
            }
        }

        public string this[string key]
        {
            get
            {
                lock (syncRoot)
                {
                    object val;
                    dict.TryGetValue(key, out val);
                    return val as string ?? string.Empty;
                }
            }
            set
            {
                Set(key, value);
            }
        }

        public AddInProperties()
        {
            this.syncRoot = new object();
        }

        private AddInProperties(AddInProperties parent)
        {
            this.parent = parent;
            this.syncRoot = parent.syncRoot;
        }

        public T Get<T>(string key, T defaultValue)
        {
            lock (syncRoot)
            {
                object val;
                if (dict.TryGetValue(key, out val))
                {
                    try
                    {
                        return (T)Deserialize(val, typeof(T));
                    }
                    catch (SerializationException ex)
                    {
                        return defaultValue;
                    }
                }
                else
                {
                    return defaultValue;
                }
            }
        }


        object Deserialize(object serializedVal, Type targetType)
        {
            if (serializedVal == null)
                return null;
            XElement element = serializedVal as XElement;
            if (element != null)
            {
                using (var xmlReader = element.Elements().Single().CreateReader())
                {
                    return XamlServices.Load(xmlReader);
                }
            }
            else
            {
                string text = serializedVal as string;
                if (text == null)
                    throw new InvalidOperationException("Cannot read a properties container as a single value");
                TypeConverter c = TypeDescriptor.GetConverter(targetType);
                return c.ConvertFromInvariantString(text);
            }
        }

        public void Set<T>(string key, T value)
        {
            object serializedValue = Serialize(value, typeof(T), key);
            SetSerializedValue(key, serializedValue);
        }

        void SetSerializedValue(string key, object serializedValue)
        {
            if (serializedValue == null)
            {
                Remove(key);
                return;
            }
            lock (syncRoot)
            {
                object oldValue;
                if (dict.TryGetValue(key, out oldValue))
                {
                    if (object.Equals(serializedValue, oldValue))
                        return;
                    HandleOldValue(oldValue);
                }
                dict[key] = serializedValue;
            }
        }

        object Serialize(object value, Type sourceType, string key)
        {
            if (value == null)
                return null;
            TypeConverter c = TypeDescriptor.GetConverter(sourceType);

            if (c != null && c.CanConvertTo(typeof(string)) && c.CanConvertFrom(typeof(string)))
            {
                return c.ConvertToInvariantString(value);
            }

            var element = new XElement("SerializedObject");
            if (key != null)
            {
                element.Add(new XAttribute("key", key));
            }
            using (var xmlWriter = element.CreateWriter())
            {
                XamlServices.Save(xmlWriter, value);
            }
            return element;
        }


        public bool Remove(string key)
        {
            bool removed = false;

            lock (syncRoot)
            {
                object oldValue;
                if (dict.TryGetValue(key, out oldValue))
                {
                    removed = true;
                    HandleOldValue(oldValue);
                    MakeDirty();
                    dict.Remove(key);
                }
            }

            return removed;
        }

        void HandleOldValue(object oldValue)
        {
            AddInProperties p = oldValue as AddInProperties;
            if (p != null)
            {
                Debug.Assert(p.parent == this);
                p.parent = null;
            }
        }

        void MakeDirty()
        {
            if (!isDirty)
            {
                isDirty = true;
                if (parent != null)
                    parent.MakeDirty();
            }
        }

        void CleanDirty()
        {
            if (isDirty)
            {
                isDirty = false;
                foreach (var properties in dict.Values.OfType<AddInProperties>())
                {
                    properties.CleanDirty();
                }
            }
        }

        /// <summary>
        /// 读取插件节点的属性信息
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static AddInProperties ReadFromAttributes(XmlReader reader)
        {
            AddInProperties properties = new AddInProperties();

            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);

                    string val = reader.NameTable.Add(reader.Value);

                    properties[reader.Name] = val;
                }

                reader.MoveToElement();
            }
            return properties;
        }

    }
}
