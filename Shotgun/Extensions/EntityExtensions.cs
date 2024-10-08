using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Shotgun.Entity;


namespace Shotgun.EntityExtensions
{
    //Base interface for all models used on api too implement
    //Each model will implement this interface so the generic repository
    //implementation can make queries for id fields on our models


    public static class EntityExtensions
    {
        public static async Task GetSingleNavigations<T>(this IEntity<T> entity, DbContext context)
        {
            var singles = entity.GetSingleNavigationsAsStrings<T>();

            foreach (var prop in singles)
            {
                await context.Entry(entity).Reference(prop).LoadAsync();
                var propEntry = entity.GetType().GetProperty(prop).GetValue(entity, null);
                if (propEntry != null && propEntry is IEntity<T> nestedEntity)
                {
                    await nestedEntity.GetSingleNavigations(context);
                    await nestedEntity.GetCollectionNavigations(context);
                }
            }
        }

        public static async Task GetCollectionNavigations<T>(this IEntity<T> entity, DbContext context)
        {
            var collections = entity.GetCollectionNavigationsAsStrings();

            foreach (var prop in collections)
            {
                await context.Entry(entity).Collection(prop).LoadAsync();
                var propEntries = entity.GetType().GetProperty(prop).GetValue(entity, null) as IEnumerable<IEntity<T>>;
                if (propEntries != null)
                {
                    foreach (var propEntry in propEntries)
                    {
                        await propEntry.GetSingleNavigations(context);
                        await propEntry.GetCollectionNavigations(context);
                    }
                }
            }
        }

        public static List<string> GetCollectionNavigationsAsStrings<T>(this IEntity<T> entity)
        {
            var properties = entity.GetType().GetProperties();
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

        public static List<string> GetSingleNavigationsAsStrings<T>(this IEntity<T> entity)
        {
            var properties = entity.GetType().GetProperties();
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
    }
}