#region

using System;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace RIS.Core.Helper
{
    public static class Serializer
    {
        /// <summary>
        ///     Serialize an object into an array of bytes
        /// </summary>
        /// <param name="item">The object to serialize</param>
        /// <returns>A byte array representation of the item</returns>
        public static string Serialize(object item)
        {
            try
            {
                if (item == null) return null;

                return JsonConvert.SerializeObject(item, Formatting.Indented);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Serialize from a type and saves to a file
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fileName"></param>
        public static void SerializeToFile(object item, string fileName)
        {
            try
            {
                if (item == null || string.IsNullOrEmpty(fileName)) return;

                // serialize JSON directly to a file
                using (var file = File.CreateText(fileName))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, item);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Deserialize to a type
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string value) where T : class, new()
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return new T();

                var result = JsonConvert.DeserializeObject<T>(value);
                return result ?? new T();
            }
            catch (Exception)
            {
                return new T();
            }
        }

        /// <summary>
        ///     Deserialize from a path to a type
        /// </summary>
        public static T DeserializeFromFile<T>(string fileName) where T : class, new()
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName)) return new T();

                // deserialize JSON directly from a file
                using (var file = File.OpenText(fileName))
                {
                    var serializer = new JsonSerializer();
                    var result = (T) serializer.Deserialize(file, typeof(T));
                    return result ?? new T();
                }
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}