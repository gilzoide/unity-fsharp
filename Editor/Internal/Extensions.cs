using System;
using System.Collections;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public static class Extensions
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // no-op
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static XmlElement AddElement(this XmlNode xml, string tag)
        {
            var child = (xml is XmlDocument xmlDoc ? xmlDoc : xml.OwnerDocument).CreateElement(tag);
            xml.AppendChild(child);
            return child;
        }

        public static XmlElement AddElement(this XmlNode xml, string tag, string text)
        {
            var child = xml.AddElement(tag);
            child.InnerText = text;
            return child;
        }

        public static XmlElement AddElement(this XmlNode xml, string tag, string attribute, string value)
        {
            var child = xml.AddElement(tag);
            child.SetAttribute(attribute, value);
            return child;
        }

        public static XmlElement AddElement(this XmlNode xml, string tag, string attribute, string value, string text)
        {
            var child = xml.AddElement(tag, attribute, value);
            child.InnerText = text;
            return child;
        }

        public static XmlElement AddElement(this XmlNode xml, string tag, string attribute1, string value1, string attribute2, string value2)
        {
            var child = xml.AddElement(tag);
            child.SetAttribute(attribute1, value1);
            child.SetAttribute(attribute2, value2);
            return child;
        }
    }
}
