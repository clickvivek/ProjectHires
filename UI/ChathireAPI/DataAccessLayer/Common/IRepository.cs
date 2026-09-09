//using DataAccessLayer.Audit;
//using DataAccessLayer.CustomModels;
using BusinessEntityAndDTO.Common;
using DataAccessLayer.Models;
//using EntityAndDTO.AuditEntity;
//using EntityAndDTO.Common;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DataAccessLayer.Common
{
    public interface IRepository
    {
        Task<E> Get<E, P>(P id) where E : BaseModel<P> where P : struct;
        Task<List<E>> GetAll<E>() where E : class;

        Task<E> Post<E, P>(E entity, bool saveChanges = true) where E : BaseModel<P> where P : struct;
        Task Put<E, P>(P id, E user, bool saveChanges = true) where E : BaseModel<P> where P : struct;
        Task Delete<E, P>(P id, bool saveChanges = true) where E : BaseModel<P> where P : struct;
        Task<bool> Exists<E, P>(P id) where E : BaseModel<P> where P : struct;
    }

    public interface IRepository<T, I> : IRepository
    {
        Task<T> Get(I id);
        Task<List<T>> GetAll();
        Task Put(I id, T user, bool saveChanges = true);
        Task<T> Post(T user, bool saveChanges = true);
        Task Delete(I id, bool saveChanges = true);
        Task<bool> Exists(I id);
        Task<List<T>> AddRange(List<T> lists, UserContext userContext);

        //Task RemoveRange(I id, List<T> lists, UserContext userContext);
    }
    //public class BaseRepository
    //{
    //    protected IAuditRepository auditRepository;
    //    public BaseRepository(IRepositoryFactory _repositoryFactory, String ConnectionString) 
    //    {
    //        auditRepository = _repositoryFactory.Get<IAuditRepository>();
    //    }

    //    private void PopulateAuditValuesContext(EntityAndDTO.AuditEntity.Audit audit, UserContext context)
    //    {
    //        audit.AuditDate = DateTime.UtcNow;
    //        audit.SessionGuid = context.SessionGuid;
    //        audit.UserId = context.UserId;
    //    }

    //    private Audit<E> GetAuditValues<E>(E value, String MethodName, String MethodFullName, String Message = "Success", EnumAuditGroup? auditGroup = null, EnumAuditKeyColumn? KeyColumn = null, string KeyColumnValue = null, String AddlData = null)
    //    {
    //        EntityAndDTO.AuditEntity.Audit<E> audit = null;
    //        if (auditRepository != null)
    //        {
    //            audit = new Audit<E>();
    //            EnumAuditGroup auditGroupFromClass;
    //            audit.AuditGroup = auditGroup == null ? (Enum.TryParse<EnumAuditGroup>(typeof(E).Name, true, out auditGroupFromClass) ? auditGroupFromClass : EnumAuditGroup.None) : auditGroup.Value;
    //            audit.IsSuccess = true;
    //            audit.MethodName = MethodName;
    //            audit.MethodFullName = MethodFullName;
    //            audit.Message = Message;
    //            audit.AuditDate = DateTime.UtcNow;
    //            audit.AdddlData = AddlData;
    //            audit.AdddlDataObject = value;
    //            audit.KeyColumnValue = KeyColumnValue == null ?
    //                 value == null ? "" : value.GetType().GetProperty("Id").GetValue(value).ToString() 
    //                : KeyColumnValue;
    //            audit.KeyColumn = KeyColumn == null ? EnumAuditKeyColumn.Id : KeyColumn.Value;
    //        }
    //        return audit;
    //    }

    //    protected async Task ExecuteAsync(Func<Task> function, UserContext context, EntityAndDTO.AuditEntity.Audit audit = null)
    //    {
    //        try
    //        {
    //            await function();
    //            await AuditEntry(audit, context, true);
    //        }
    //        catch (ArgumentException)
    //        {
    //            await AuditEntry(audit, context, true);
    //            throw;
    //        }
    //        catch (System.Exception ex)
    //        {
    //            audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //            await AuditEntry(audit, context, false, ex.Message);
    //            throw;
    //        }
    //    }

    //    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> function, UserContext context, Audit<T> audit = null)
    //    {

    //        try
    //        {
    //            var result =  await function();
    //            audit.AdddlDataObject = result;
    //            await AuditEntry(audit, context, true);

    //            return result;
    //        }
    //        catch (ArgumentException ex)
    //        {
    //            await AuditEntry(audit, context, true);
    //            throw;
    //        }
    //        catch (System.Exception ex)
    //        {
    //            audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //            await AuditEntry(audit, context, false, ex.Message);
    //            throw;
    //        }
    //    }

    //    protected async Task<E> ExecuteAsync<I,E>(Func<I,Task<E>> function, UserContext context,I value, Audit<I> audit = null)
    //    {
    //        try
    //        {
    //            var result = await function(value);
    //            await AuditEntry(audit, context, true);
    //            return result;
    //        }
    //        catch (ArgumentException)
    //        {
    //            await AuditEntry(audit, context, true);
    //            throw;
    //        }
    //        catch (System.Exception ex)
    //        {
    //            audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //            await AuditEntry(audit, context, false, ex.Message);
    //            throw;
    //        }
    //    }

    //    protected async Task ExecuteAsync<I>(Func<I, Task> function, UserContext context, I value, Audit<I> audit = null)
    //    {
    //        try
    //        {
    //            await function(value);
    //            await AuditEntry(audit, context, true);
    //        }
    //        catch (ArgumentException)
    //        {
    //            await AuditEntry(audit, context, true);
    //            throw;
    //        }
    //        catch (System.Exception ex)
    //        {
    //            audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //            await AuditEntry(audit, context, false, ex.Message);
    //            throw;
    //        }
    //    }

    //    //protected async Task<E> ExecuteAsync<E>(Func<Task<E>> function, UserContext context, EntityAndDTO.AuditEntity.Audit audit = null)
    //    //{
    //    //    try
    //    //    {
    //    //        var result = await function();
    //    //        await AuditEntry(audit, context, true);
    //    //        return result;
    //    //    }
    //    //    catch (ArgumentException ex)
    //    //    {
    //    //        await AuditEntry(audit, context, true);
    //    //        throw ex;
    //    //    }
    //    //    catch (System.Exception ex)
    //    //    {
    //    //        audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //    //        await AuditEntry(audit, context, false, ex.Message);
    //    //        throw ex;
    //    //    }
    //    //}
    //    protected async Task ExecuteAsync(Func<Task> function, UserContext context, String MethodName, String MethodFullName, String Message = "Success", EnumAuditGroup? auditGroup = null, EnumAuditKeyColumn? keyColumn = null, string keyColumnValue = null)
    //    {
    //        EntityAndDTO.AuditEntity.Audit<String> audit = null;

    //        audit = GetAuditValues<String>(MethodName, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);

    //        await ExecuteAsync(function, context, audit);

    //    }
    //    protected async Task<E> ExecuteAsync<E>(Func<Task<E>> function, UserContext context, E value, String MethodName, String MethodFullName, String Message = "Success", EnumAuditGroup? auditGroup = null, EnumAuditKeyColumn? keyColumn = null, string keyColumnValue = null)
    //    {
    //        EntityAndDTO.AuditEntity.Audit<E> audit = null;

    //        audit = GetAuditValues<E>(default(E), MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);

    //        return await ExecuteAsync<E>(function, context, audit);
    //        //EntityAndDTO.AuditEntity.Audit<E> audit = null;
    //        //try
    //        //{
    //        //    audit = GetAuditValues<E>(value, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);
    //        //    await function();
    //        //    await AuditEntry(audit, context, true);
    //        //}
    //        //catch (ArgumentException ex)
    //        //{
    //        //    await AuditEntry(audit, context, true);
    //        //    throw ex;
    //        //}
    //        //catch (System.Exception ex)
    //        //{
    //        //    audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //        //    await AuditEntry(audit, context, false, ex.Message);
    //        //    throw ex;
    //        //}
    //    }

    //    //protected async Task<E> ExecuteAsync<E>(Func<Task<E>> function, UserContext context, E value, String MethodName, String MethodFullName, String Message = "Success", EnumAuditGroup? auditGroup = null, EnumAuditKeyColumn? keyColumn = null, string keyColumnValue = null)
    //    //{
    //    //    return await ExecuteAsync<E, E>(function, context, value, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);
    //    //    #region commentToRemove
    //    //    //EntityAndDTO.AuditEntity.Audit<E> audit = null;
    //    //    //try
    //    //    //{
    //    //    //    audit = GetAuditValues<E>(value, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);
    //    //    //    var result = await function();
    //    //    //    await AuditEntry(audit, context, true);
    //    //    //    return result;
    //    //    //}
    //    //    //catch (ArgumentException ex)
    //    //    //{
    //    //    //    await AuditEntry(audit, context, true);
    //    //    //    throw ex;
    //    //    //}
    //    //    //catch (System.Exception ex)
    //    //    //{
    //    //    //    audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //    //    //    await AuditEntry(audit, context, false, ex.Message);
    //    //    //    throw ex;
    //    //    //}
    //    //    #endregion commentToRemove
    //    //}


    //    protected async Task<E> ExecuteAsync<I, E>(Func<I,Task<E>> function, UserContext context, I value, String MethodName, String MethodFullName, String Message, EnumAuditGroup? auditGroup = null, EnumAuditKeyColumn? keyColumn = null, string keyColumnValue = null)
    //    {
    //        EntityAndDTO.AuditEntity.Audit<I> audit = null;

    //        audit = GetAuditValues<I>(value, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);

    //        return await ExecuteAsync<I,E>(function, context, value, audit);
    //        //EntityAndDTO.AuditEntity.Audit<T> audit = null;
    //        //try
    //        //{
    //        //    audit = GetAuditValues<T>(value, MethodName, MethodFullName, Message, auditGroup, keyColumn, keyColumnValue);
    //        //    var result = await function();
    //        //    await AuditEntry(audit, context, true);
    //        //    return result;
    //        //}
    //        //catch (ArgumentException ex)
    //        //{
    //        //    await AuditEntry(audit, context, false);
    //        //    throw ex;
    //        //}
    //        //catch (System.Exception ex)
    //        //{
    //        //    audit.AdddlData = (audit.AdddlData == null) ? ex.StackTrace : audit.AdddlData + ex.StackTrace;
    //        //    await AuditEntry(audit, context, false, ex.Message);
    //        //    throw ex;
    //        //}
    //    }

    //    protected async Task AuditEntry<T>(Audit<T> audit, UserContext context, bool IsSuccess, string Message = null)
    //    {
    //        if (audit != null && auditRepository != null)
    //        {
    //            PopulateAuditValuesContext(audit, context);
    //            if (Message != null) audit.Message = Message;
    //            audit.IsSuccess = IsSuccess;
    //            await auditRepository.AuditAsync(audit);
    //        }
    //    }
    //    private async Task AuditEntry(EntityAndDTO.AuditEntity.Audit audit, UserContext context, bool IsSuccess, string Message = null)
    //    {
    //        if (audit != null && auditRepository != null)
    //        {
    //            PopulateAuditValuesContext(audit, context);
    //            if (Message != null) audit.Message = Message;
    //            audit.IsSuccess = IsSuccess;
    //            await auditRepository.AuditAsync(audit);
    //        }
    //    }

    //}
}
