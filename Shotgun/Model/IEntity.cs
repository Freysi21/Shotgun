using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Shotgun.Models
{
    //Base interface for all models used on api too implement
    //Each model will implement this interface so the generic repository
    //implementation can make queries for id fields on our models


    public abstract class IEntity
    {
        public async Task GetSingleNavigations(DbContext context)
        {
            var singles = GetSingleNavigationsAsStrings();

            foreach (var prop in singles)
            {
                await context.Entry(this).Reference(prop).LoadAsync();
                var propEntry = this.GetType().GetProperty(prop).GetValue(this, null);
                if (propEntry != null && (propEntry as IEntity) != null)
                {
                    await (propEntry as IEntity).GetSingleNavigations(context);
                    await (propEntry as IEntity).GetCollectionNavigations(context);
                }
            }

        }
        public async Task GetCollectionNavigations(DbContext context)
        {
            var collections = GetCollectionNavigationsAsStrings();

            foreach (var prop in collections)
            {
                await context.Entry(this).Collection(prop).LoadAsync();
                var propList = GetType().GetProperty(prop).GetValue(this) as IEnumerable<IEntity>;

                if (propList != null && propList.Count() > 0)
                {
                    foreach (var item in propList)
                    {
                        await item.GetCollectionNavigations(context);
                        await item.GetCollectionNavigations(context);
                    }
                }
            }
        }

        public List<string> GetCollectionNavigationsAsStrings()
        {
            var properties = GetType().GetProperties();
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

        public List<string> GetSingleNavigationsAsStrings()
        {
            var properties = GetType().GetProperties();
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
    public abstract class IEntity<T> : IEntity
    {
        public abstract T Id { get; set; }
    }
}