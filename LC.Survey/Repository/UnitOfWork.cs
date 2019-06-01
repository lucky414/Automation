using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Repository
{
   public class UnitOfWork: IUnitOfWork
    {
        private readonly DbContext _dbContext;

        public UnitOfWork(DbContext context)
        {
            _dbContext = context;
        }

        public void SaveChanges()
        {
            ((IObjectContextAdapter)_dbContext).ObjectContext.SaveChanges();
        }

        #region Implementation of IDisposable

        private bool _disposed;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes off the managed and unmanaged resources used.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_disposed)
                return;

            _disposed = true;
        }

        #endregion
    }
}
