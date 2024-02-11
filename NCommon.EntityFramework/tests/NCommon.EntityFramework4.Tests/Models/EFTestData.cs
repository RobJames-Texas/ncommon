using System;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace NCommon.EntityFramework4.Tests.Models
{
    public class EFTestData
    {
        readonly ObjectContext _context;

        public EFTestData(ObjectContext context)
        {
            _context = context;
            _context.ContextOptions.LazyLoadingEnabled = true;
        }

        public ObjectContext Context
        {
            get { return _context; }
        }

        public void Refresh(object entity)
        {
            _context.Refresh(RefreshMode.StoreWins, entity);
        }

        public T Get<T>(Func<T, bool> predicate) where T : class
        {
            return _context.CreateObjectSet<T>().Where(predicate).FirstOrDefault();
        }

        public void Batch(Action<EFTestDataActions> action)
        {
            var dataActions = new EFTestDataActions(this);
            action(dataActions);
            _context.SaveChanges();
        }
    }
}