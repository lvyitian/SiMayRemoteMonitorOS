namespace SiMay.ReflectCache
{
    /// <summary>
    /// Abstraction of the function of accessing member of a object at runtime.
    /// </summary>
    public interface IMemberAccessor
    {
        /// <summary>
        /// Get the member value of an object.
        /// </summary>
        /// <param name="instance">The object to get the member value from.</param>
        /// <param name="memberName">The member name, could be the name of a property of field. Must be public member.</param>
        /// <returns>The member value</returns>
        object GetValue(object instance, string memberName);

        /// <summary>
        /// Set the member value of an object.
        /// </summary>
        /// <param name="instance">The object to get the member value from.</param>
        /// <param name="memberName">The member name, could be the name of a property of field. Must be public member.</param>
        /// <param name="newValue">The new value of the property for the object instance.</param>
        void SetValue(object instance, string memberName, object newValue);
    }
}
