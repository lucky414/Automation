using LC.Survey.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LC.Survey.Repository
{
  public  class Repository: IRepository, IDisposable
    {
        //Private Variables
        private bool bDisposed;
        private DbContext context;
        private IUnitOfWork unitOfWork;

        #region Contructor Logic

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Repository<TEntity>"/> class.
        /// </summary>
        public Repository()
        {

        }

        /// <summary>
        ///     Initializes a new instance of the
        /// <see cref="Repository<TEntity>" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Repository(DbContext contextObj)
        {
            if (contextObj == null)
                throw new ArgumentNullException("context");
            this.context = contextObj;
        }

        public Repository(ObjectContext contextObj)
        {
            if (contextObj == null)
                throw new ArgumentNullException("context");
            context = new DbContext(contextObj, true);
        }

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Properties

        //DbContext Property
        protected DbContext DbContext
        {
            get
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                return context;
            }
        }

        //Unit of Work Property
        public IUnitOfWork UnitOfWork
        {
            get
            {
                if (unitOfWork == null)
                {
                    unitOfWork = new UnitOfWork(DbContext);
                }
                return unitOfWork;
            }
        }

        #endregion

        #region Data Display Methods

        //Helper Method tp create Query [IQuerable]

        public TEntity GetByKey<TEntity>(object keyValue) where TEntity : class
        {
            EntityKey key = GetEntityKey<TEntity>(keyValue);

            object originalItem;
            if (((IObjectContextAdapter)DbContext).
            ObjectContext.TryGetObjectByKey(key, out originalItem))
            {
                return (TEntity)originalItem;
            }

            return default(TEntity);
        }

        public IQueryable<TEntity> GetQuery<TEntity>() where TEntity : class
        {
            string entityName = GetEntityName<TEntity>();
            return ((IObjectContextAdapter)DbContext).
            ObjectContext.CreateQuery<TEntity>(entityName);
        }

        public IQueryable<TEntity> GetQuery<TEntity>
        (Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return GetQuery<TEntity>().Where(predicate);
        }

        //All Readonly Display or fetch data methods.
        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return GetQuery<TEntity>().AsEnumerable();
        }

        public IEnumerable<TEntity> Get<TEntity, TOrderBy>
        (Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex,
            int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return GetQuery<TEntity>()
                    .OrderBy(orderBy)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable();
            }
            return
                GetQuery<TEntity>()
                    .OrderByDescending(orderBy)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable();
        }

        public IEnumerable<TEntity> Get<TEntity,
        TOrderBy>(Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize,
            SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return GetQuery(criteria).
                    OrderBy(orderBy).
                    Skip((pageIndex - 1) * pageSize).
                    Take(pageSize)
                    .AsEnumerable();
            }
            return
                GetQuery(criteria)
                    .OrderByDescending(orderBy)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable();
        }

        public TEntity Single<TEntity>
        (Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return GetQuery<TEntity>().Single<TEntity>(criteria);
        }

        public TEntity First<TEntity>
        (Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return GetQuery<TEntity>().First(predicate);
        }

        public IEnumerable<TEntity> Find<TEntity>
        (Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return GetQuery<TEntity>().Where(criteria);
        }

        public TEntity FindOne<TEntity>
        (Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return GetQuery<TEntity>().Where(criteria).FirstOrDefault();
        }

        public int Count<TEntity>() where TEntity : class
        {
            return GetQuery<TEntity>().Count();
        }

        public int Count<TEntity>
        (Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return GetQuery<TEntity>().Count(criteria);
        }

        #endregion

        #region Data Transactional Methods

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            DbContext.Set<TEntity>().Add(entity);
        }

        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            DbContext.Set<TEntity>().Attach(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            string fqen = GetEntityName<TEntity>();

            object originalItem;
            EntityKey key =
            ((IObjectContextAdapter)DbContext).ObjectContext.CreateEntityKey(fqen, entity);
            if (((IObjectContextAdapter)DbContext).ObjectContext.TryGetObjectByKey
            (key, out originalItem))
            {
                ((IObjectContextAdapter)DbContext).ObjectContext.ApplyCurrentValues
                (key.EntitySetName, entity);
            }
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            DbContext.Set<TEntity>().Remove(entity);
        }

        public void Delete<TEntity>(Expression<Func<TEntity,
        bool>> criteria) where TEntity : class
        {
            IEnumerable<TEntity> records = Find(criteria);

            foreach (TEntity record in records)
            {
                Delete(record);
            }
        }

        #endregion

        #region Internal Processing Private Methods

        private EntityKey GetEntityKey<TEntity>(object keyValue) where TEntity : class
        {
            string entitySetName = GetEntityName<TEntity>();
            ObjectSet<TEntity> objectSet =
            ((IObjectContextAdapter)DbContext).ObjectContext.CreateObjectSet<TEntity>();
            string keyPropertyName = objectSet.EntitySet.ElementType.KeyMembers[0].ToString();
            var entityKey = new EntityKey
            (entitySetName, new[] { new EntityKeyMember(keyPropertyName, keyValue) });
            return entityKey;
        }

        private string GetEntityName<TEntity>() where TEntity : class
        {
            // Thanks to Kamyar Paykhan -
            // http://huyrua.wordpress.com/2011/04/13/
            // entity-framework-4-poco-repository-and-specification-pattern-upgraded-to-ef-4-1/
            // #comment-688
            string entitySetName = ((IObjectContextAdapter)DbContext).ObjectContext
                .MetadataWorkspace
                .GetEntityContainer(((IObjectContextAdapter)DbContext).
                    ObjectContext.DefaultContainerName,
                    DataSpace.CSpace)
                .BaseEntitySets.Where(bes => bes.ElementType.Name == typeof(TEntity).Name).First().Name;
            return string.Format("{0}.{1}",
            ((IObjectContextAdapter)DbContext).ObjectContext.DefaultContainerName,
                entitySetName);
        }

        private string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private bool IsValidTag(string tag, string tags)
        {
            string[] allowedTags = tags.Split(',');
            if (tag.IndexOf("javascript") >= 0) return false;
            if (tag.IndexOf("vbscript") >= 0) return false;
            if (tag.IndexOf("onclick") >= 0) return false;

            var endchars = new char[] { ' ', '>', '/', '\t' };

            int pos = tag.IndexOfAny(endchars, 1);
            if (pos > 0) tag = tag.Substring(0, pos);
            if (tag[0] == '/') tag = tag.Substring(1);

            foreach (string aTag in allowedTags)
            {
                if (tag == aTag) return true;
            }

            return false;
        }

        #endregion

        #region Disposing Methods

        protected void Dispose(bool bDisposing)
        {
            if (!bDisposed)
            {
                if (bDisposing)
                {
                    if (null != context)
                    {
                        context.Dispose();
                    }
                }
                bDisposed = true;
            }
        }

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
