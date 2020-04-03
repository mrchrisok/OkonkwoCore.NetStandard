using System;
using System.Reflection;

namespace OkonkwoCore.Netx.Utilities
{
    /// <summary>
    /// The PropertyHelper provides multiple operations that use reflection to act upon an object's
    /// properties and methods.
    /// </summary>
    public static class PropertyHelper
    {
        #region Property operations

        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="TProp">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static TProp GetPrivatePropertyValue<TProp>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo pi = obj.GetType().GetProperty(propName, bindingFlags);

            if (pi == null)
            {
                var message = $"Property {propName} was not found in Type {obj.GetType().FullName}";
                throw new ArgumentOutOfRangeException("propName", message);
            }

            return (TProp)pi.GetValue(obj, null);
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="TProp">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<TProp>(this object obj, string propName, TProp val)
        {
            Type t = obj.GetType();

            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
            {
                throw new ArgumentOutOfRangeException("propName",
                                      string.Format("Property {0} was not found in Type {1}", propName,
                                                    obj.GetType().FullName));
            }

            t.InvokeMember(propName,
                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty |
                           BindingFlags.Instance, null, obj, new object[] { val });
        }

        #endregion

        #region Field operations

        /// <summary>
        /// Returns a private Field Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="TField">Type of the Field</typeparam>
        /// <param name="obj">Object from where the Field Value is returned</param>
        /// <param name="propName">Field Name as string.</param>
        /// <returns>FieldValue</returns>
        public static TField GetPrivateFieldValue<TField>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Type t = obj.GetType();
            FieldInfo fi = null;

            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }

            if (fi == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propName),
                                      string.Format("Field {0} was not found in Type {1}", propName,
                                                    obj.GetType().FullName));
            }

            return (TField)fi.GetValue(obj);
        }

        /// <summary>
        /// Set a private Field Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="TField">Type of the Field</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Field name as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static void SetPrivateFieldValue<TField>(this object obj, string propName, TField val)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Type t = obj.GetType();
            FieldInfo fi = null;

            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }

            if (fi == null)
            {
                string message = string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName);
                throw new ArgumentOutOfRangeException("propName", message);
            }

            fi.SetValue(obj, val);
        }

        #endregion
    }
}
