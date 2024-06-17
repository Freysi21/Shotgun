using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shotgun.Expressions
{
    public static class Include
    {
        public static Func<IQueryable<T>, IQueryable<T>> GetNavigations<T>() where T : class
        {
            var type = typeof(T);
            var navigationProperties = new List<string>();

            //get navigation properties
            GetNavigationProperties(type, type, string.Empty, navigationProperties);

            Func<IQueryable<T>, IQueryable<T>> includes = (query =>
            {
                return navigationProperties.Aggregate(query, (current, inc) => current.Include(inc));
            });

            return includes;
        }

        public static List<string> GetNavigationsAsString<T>() where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var navigationPropertyInfoList = properties.Where(
                prop =>
                    prop.IsDefined(typeof(NavigationPropertyAttribute)) ||
                    prop.IsDefined(typeof(SingleNavigationPropertyAttribute))
            );
            var propsNames = new List<string>();
            if (navigationPropertyInfoList.Count() > 0)
            {
                foreach (PropertyInfo prop in navigationPropertyInfoList)
                {
                    propsNames.Add(prop.Name);
                }
            }

            return propsNames;
        }

        public static List<string> GetCollectionNavigationsAsStrings<T>() where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var navigationPropertyInfoList = properties.Where(
                prop =>
                    prop.IsDefined(typeof(NavigationPropertyAttribute))
            );
            var propsNames = new List<string>();
            if (navigationPropertyInfoList.Count() > 0)
            {
                foreach (PropertyInfo prop in navigationPropertyInfoList)
                {
                    propsNames.Add(prop.Name);
                }
            }

            return propsNames;
        }

        public static List<string> GetSingleNavigationsAsStrings<T>() where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var navigationPropertyInfoList = properties.Where(
                prop =>
                    prop.IsDefined(typeof(SingleNavigationPropertyAttribute))
            );
            var propsNames = new List<string>();
            if (navigationPropertyInfoList.Count() > 0)
            {
                foreach (PropertyInfo prop in navigationPropertyInfoList)
                {
                    propsNames.Add(prop.Name);
                }
            }

            return propsNames;
        }

        private static void GetNavigationProperties(Type baseType, Type type, string parentPropertyName, IList<string> accumulator)
        {
            //get navigation properties
            var properties = type.GetProperties();
            var navigationPropertyInfoList = properties.Where(
                prop =>
                    prop.IsDefined(typeof(NavigationPropertyAttribute)) ||
                    prop.IsDefined(typeof(SingleNavigationPropertyAttribute))
            );


            foreach (PropertyInfo prop in navigationPropertyInfoList)
            {
                var propertyType = prop.PropertyType;
                var elementType = propertyType.GetTypeInfo().IsGenericType ? propertyType.GetGenericArguments()[0] : propertyType;

                //Prepare navigation property in {parentPropertyName}.{propertyName} format and push into accumulator
                var properyName = string.Format("{0}{1}{2}", parentPropertyName, string.IsNullOrEmpty(parentPropertyName) ? string.Empty : ".", prop.Name);
                if (elementType != baseType)
                {
                    accumulator.Add(properyName);
                }

                //Skip recursion of propert has JsonIgnore attribute or current property type is the same as baseType
                var isJsonIgnored = prop.IsDefined(typeof(JsonIgnoreAttribute));
                if (!isJsonIgnored)
                {
                    GetNavigationProperties(baseType, elementType, properyName, accumulator);
                }
            }
        }
    }
}