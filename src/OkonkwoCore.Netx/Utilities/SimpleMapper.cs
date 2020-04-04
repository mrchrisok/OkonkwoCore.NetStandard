using System;
using System.Linq;

namespace OkonkwoCore.Netx.Utilities
{
    public static class SimpleMapper
    {
        public static void PropertyMap<T, U>(T source, U destination)
           where T : class, new()
           where U : class, new()
        {
            var sourceProperties = source.GetType().GetProperties().ToList();
            var destinationProperties = destination.GetType().GetProperties().ToList();

            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.Find(item => item.Name == sourceProperty.Name);

                if (destinationProperty != null)
                {
                    try
                    {
                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }
    }
}
