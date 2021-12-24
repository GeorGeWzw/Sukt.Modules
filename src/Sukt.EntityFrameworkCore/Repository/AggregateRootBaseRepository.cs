﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sukt.Module.Core.Domian;
using Sukt.Module.Core.Enums;
using Sukt.Module.Core.Exceptions;
using Sukt.Module.Core.Extensions;
using Sukt.Module.Core.OperationResult;
using Sukt.Module.Core.Repositories;
using Sukt.Module.Core.ResultMessageConst;
using Sukt.Module.Core.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Sukt.EntityFrameworkCore
{
    public class AggregateRootBaseRepository<TEntity, Tkey> : IAggregateRootRepository<TEntity, Tkey>
        where TEntity : class, IAggregateRootWithIdentity<Tkey> where Tkey : IEquatable<Tkey>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AggregateRootBaseRepository(IServiceProvider serviceProvider)
        {
            UnitOfWork = (serviceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork);//获取工作单元实例
            _dbContext = UnitOfWork.GetDbContext();
            _dbSet = _dbContext.Set<TEntity>();
            _logger = serviceProvider.GetLogger<AggregateRootBaseRepository<TEntity, Tkey>>();
            _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        }
        /// <summary>
        /// 表对象
        /// </summary>
        private readonly DbSet<TEntity> _dbSet = null;

        /// <summary>
        /// 上下文
        /// </summary>
        private readonly DbContext _dbContext = null;

        /// <summary>
        ///
        /// </summary>
        private readonly ILogger _logger = null;
        /// <summary>
        /// 工作单元
        /// </summary>
        public IUnitOfWork UnitOfWork { get; }
        #region Query

        /// <summary>
        /// 获取 不跟踪数据更改（NoTracking）的查询数据源
        /// </summary>
        public virtual IQueryable<TEntity> NoTrackEntities => _dbSet.AsNoTracking();

        /// <summary>
        /// 获取 跟踪数据更改（Tracking）的查询数据源
        /// </summary>
        public virtual IQueryable<TEntity> TrackEntities => _dbSet;

        /// <summary>
        /// 根据ID得到实体
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public virtual TEntity GetById(Tkey primaryKey) => _dbSet.Find(primaryKey);

        /// <summary>
        /// 异步根据ID得到实体
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByIdAsync(Tkey primaryKey) => await _dbSet.FindAsync(primaryKey);

        /// <summary>
        /// 将查询的实体转换为DTO输出
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public virtual TDto GetByIdToDto<TDto>(Tkey primaryKey) where TDto : class, new() => this.GetById(primaryKey).MapTo<TDto>();

        /// <summary>
        /// 异步将查询的实体转换为DTO输出
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public virtual async Task<TDto> GetByIdToDtoAsync<TDto>(Tkey primaryKey) where TDto : class, new() => (await this.GetByIdAsync(primaryKey)).MapTo<TDto>();

        #endregion Query

        #region Insert

        /// <summary>
        /// 同步批量添加实体
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public virtual OperationResponse Insert(params TEntity[] entitys)
        {
            entitys.NotNull(nameof(entitys));
            //entitys = CheckInsert(entitys);
            _dbSet.AddRange(entitys);
            int count = _dbContext.SaveChanges();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }

        /// <summary>
        /// 异步添加单条实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> InsertAsync(TEntity entity)
        {
            entity.NotNull(nameof(entity));
            //entity = CheckInsert(entity);
            await _dbSet.AddAsync(entity);
            int count = await _dbContext.SaveChangesAsync();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }
        /// <summary>
        /// 异步添加单条实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="checkFunc"></param>
        /// <param name="insertFunc"></param>
        /// <param name="completeFunc"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> InsertAsync(TEntity entity, Func<TEntity, Task> checkFunc = null, Func<TEntity, TEntity, Task<TEntity>> insertFunc = null, Func<TEntity, TEntity> completeFunc = null)
        {
            entity.NotNull(nameof(entity));
            try
            {
                if (checkFunc.IsNotNull())
                {
                    await checkFunc(entity);
                }
                if (!insertFunc.IsNull())
                {
                    entity = await insertFunc(entity, entity);
                }
                //entity = entity.CheckInsert<TEntity, Tkey>(_httpContextAccessor);//CheckInsert(entity);
                await _dbSet.AddAsync(entity);

                if (completeFunc.IsNotNull())
                {
                    entity = completeFunc(entity);
                }
                int count = await _dbContext.SaveChangesAsync();
                return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
            }
            catch (SuktAppException e)
            {
                return new OperationResponse(e.Message, OperationEnumType.Error);
            }
            catch (Exception ex)
            {
                return new OperationResponse(ex.Message, OperationEnumType.Error);
            }
            //entity.NotNull(nameof(entity));
            //entity = CheckInsert(entity);
            //await _dbSet.AddAsync(entity);
            //int count = await _dbContext.SaveChangesAsync();
            //return new OperationResponse(count > 0 ? ResultMessage.InsertSuccess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }
        /// <summary>
        /// 批量异步添加实体
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> InsertAsync(TEntity[] entitys)
        {
            entitys.NotNull(nameof(entitys));
            //entitys = CheckInsert(entitys);
            await _dbSet.AddRangeAsync(entitys);
            int count = await _dbContext.SaveChangesAsync();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }
        #endregion Insert

        #region Update

        /// <summary>
        /// 同步逐条更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual OperationResponse Update(TEntity entity)
        {
            entity.NotNull(nameof(entity));
            //entity = CheckUpdate(entity);
            _dbSet.Update(entity);
            int count = _dbContext.SaveChanges();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }

        /// <summary>
        /// 异步逐条更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> UpdateAsync(TEntity entity)
        {
            entity.NotNull(nameof(entity));
            //entity = CheckUpdate(entity);
            _dbSet.Update(entity);
            int count = await _dbContext.SaveChangesAsync();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }

        /// <summary>
        /// 异步批量更新
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> UpdateAsync(TEntity[] entitys)
        {
            entitys.NotNull(nameof(entitys));
            //entitys = CheckUpdate(entitys);
            _dbSet.UpdateRange(entitys);
            int count = await _dbContext.SaveChangesAsync();
            return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }
        /// <summary>
        /// 异步更新单条实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="checkFunc"></param>
        /// <returns></returns>
        public virtual async Task<OperationResponse> UpdateAsync(TEntity entity, Func<TEntity, Task> checkFunc = null)
        {
            entity.NotNull(nameof(entity));
            try
            {
                if (checkFunc.IsNotNull())
                {
                    await checkFunc(entity);
                }
                //entity = entity.CheckInsert<TEntity, Tkey>(_httpContextAccessor);//CheckInsert(entity);
                _dbSet.Update(entity);
                int count = await _dbContext.SaveChangesAsync();
                return new OperationResponse(count > 0 ? ResultMessage.SaveSusscess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
            }
            catch (SuktAppException e)
            {
                return new OperationResponse(e.Message, OperationEnumType.Error);
            }
            catch (Exception ex)
            {
                return new OperationResponse(ex.Message, OperationEnumType.Error);
            }
            //entity.NotNull(nameof(entity));
            //entity = CheckInsert(entity);
            //await _dbSet.AddAsync(entity);
            //int count = await _dbContext.SaveChangesAsync();
            //return new OperationResponse(count > 0 ? ResultMessage.InsertSuccess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }
        #endregion Update

        #region Delete

        public virtual int Delete(params TEntity[] entitys)
        {
            foreach (var entity in entitys)
            {
                CheckDelete(entity);
            }
            return _dbContext.SaveChanges();
        }

        public virtual async Task<OperationResponse> DeleteAsync(Tkey primaryKey)
        {
            TEntity entity = await this.GetByIdAsync(primaryKey);
            if (entity.IsNull())
            {
                return new OperationResponse($"该{primaryKey}键的数据不存在", OperationEnumType.QueryNull);
            }
            int count = await this.DeleteAsync(entity);
            return new OperationResponse(count > 0 ? ResultMessage.DeleteSuccess : ResultMessage.NoChangeInOperation, count > 0 ? OperationEnumType.Success : OperationEnumType.NoChanged);
        }

        public virtual async Task<int> DeleteAsync(TEntity entity)
        {
            entity = await this.GetByIdAsync(entity.Id);
            if (entity.IsNull())
            {
                throw new SuktAppException($"该{entity.Id}键的数据不存在");
            }
            CheckDelete(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteBatchAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            predicate.NotNull(nameof(predicate));
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                List<MemberBinding> newMemberBindings = new List<MemberBinding>();
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "o"); //参数

                ConstantExpression constant = Expression.Constant(true);
                var propertyName = nameof(ISoftDelete.IsDeleted);
                var propertyInfo = typeof(TEntity).GetProperty(propertyName);
                var memberAssignment = Expression.Bind(propertyInfo, constant); //绑定属性
                newMemberBindings.Add(memberAssignment);

                //创建实体
                var newEntity = Expression.New(typeof(TEntity));
                var memberInit = Expression.MemberInit(newEntity, newMemberBindings.ToArray()); //成员初始化
                Expression<Func<TEntity, TEntity>> updateExpression = Expression.Lambda<Func<TEntity, TEntity>> //生成要更新的Expression
                (
                   memberInit,
                   new ParameterExpression[] { parameterExpression }
                );

                return await NoTrackEntities.Where(predicate).UpdateAsync(updateExpression, cancellationToken);
            }
            return await NoTrackEntities.Where(predicate).DeleteAsync(cancellationToken);
        }

        #endregion Delete

        #region 帮助方法

        /// <summary>
        /// 检查删除
        /// </summary>
        /// <param name="entitys">实体集合</param>
        /// <returns></returns>
        private void CheckDelete(IEnumerable<TEntity> entitys)
        {
            foreach (var entity in entitys)
            {
                this.CheckDelete(entity);
            }
        }

        /// <summary>
        /// 检查删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        private void CheckDelete(TEntity entity)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                ISoftDelete softDeletabl = (ISoftDelete)entity;
                softDeletabl.IsDeleted = true;
                var entity1 = (TEntity)softDeletabl;

                this._dbContext.Update(entity1);
            }
            else
            {
                this._dbContext.Remove(entity);
            }
        }

        ///// <summary>
        ///// 检查软删除接口
        ///// </summary>
        ///// <param name="entity">要检查的实体</param>
        ///// <returns>返回检查好的实体</returns>
        //private TEntity CheckISoftDelete(TEntity entity)
        //{
        //    if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        //    {
        //        ISoftDelete softDeletableEntity = (ISoftDelete)entity;
        //        softDeletableEntity.IsDeleted = true;
        //        var entity1 = (TEntity)softDeletableEntity;
        //        return entity1;
        //    }
        //    return entity;
        //}

      //  /// <summary>
      //  /// 检查创建
      //  /// </summary>
      //  /// <param name="entitys">实体集合</param>
      //  /// <returns></returns>

      //  private TEntity[] CheckInsert(TEntity[] entitys)
      //  {
      //      for (int i = 0; i < entitys.Length; i++)
      //      {
      //          var entity = entitys[i];
      //          entitys[i] = CheckInsert(entity);
      //      }
      //      return entitys;
      //  }

      //  /// <summary>
      //  /// 检查创建时间
      //  /// </summary>
      //  /// <param name="entity">实体</param>
      //  /// <returns></returns>
      //  private TEntity CheckInsert(TEntity entity)
      //  {
      //      var creationAudited = entity.GetType().GetInterface(/*$"ICreationAudited`1"*/typeof(ICreated<>).Name);
      //      if (creationAudited == null)
      //      {
      //          return entity;
      //      }

      //      var typeArguments = creationAudited?.GenericTypeArguments[0];
      //      var fullName = typeArguments?.FullName;
      //      if (fullName == typeof(Guid).FullName)
      //      {
      //          entity = CheckICreationAudited<Guid>(entity);
      //      }

      //      return entity;
      //  }

      //  private TEntity CheckICreationAudited<TUserKey>(TEntity entity)
      //     where TUserKey : struct, IEquatable<TUserKey>
      //  {
      //      if (!entity.GetType().IsBaseOn(typeof(ICreated<>)))
      //      {
      //          return entity;
      //      }

      //      ICreated<TUserKey> entity1 = (ICreated<TUserKey>)entity;
      //      entity1.CreatedId = _httpContextAccessor.HttpContext.User.Identity.GetUesrId<TUserKey>();
      //      entity1.CreatedAt = DateTime.Now;
      //      return (TEntity)entity1;
      //  }

      //  /// <summary>
      //  /// 检查最后修改时间
      //  /// </summary>
      //  /// <param name="entitys"></param>
      //  /// <returns></returns>
      //  private TEntity[] CheckUpdate(TEntity[] entitys)
      //  {
      //      for (int i = 0; i < entitys.Length; i++)
      //      {
      //          var entity = entitys[i];
      //          entitys[i] = CheckUpdate(entity);
      //      }
      //      return entitys;
      //  }

      //  /// <summary>
      //  /// 检查最后修改时间
      //  /// </summary>
      //  /// <param name="entity">实体</param>
      //  /// <returns></returns>
      //  private TEntity CheckUpdate(TEntity entity)
      //  {
      //      var creationAudited = entity.GetType().GetInterface(/*$"ICreationAudited`1"*/typeof(IModifyAudited<>).Name);
      //      if (creationAudited == null)
      //      {
      //          return entity;
      //      }

      //      var typeArguments = creationAudited?.GenericTypeArguments[0];
      //      var fullName = typeArguments?.FullName;
      //      if (fullName == typeof(Guid).FullName)
      //      {
      //          entity = CheckIModificationAudited<Guid>(entity);
      //      }

      //      return entity;
      //  }

      //  /// <summary>
      //  /// 检查最后修改时间
      //  /// </summary>
      //  /// <typeparam name="TUserKey"></typeparam>
      //  /// <param name="entity"></param>
      //  /// <returns></returns>
      //  public TEntity CheckIModificationAudited<TUserKey>(TEntity entity)
      //where TUserKey : struct, IEquatable<TUserKey>
      //  {
      //      if (!entity.GetType().IsBaseOn(typeof(IModifyAudited<>)))
      //      {
      //          return entity;
      //      }

      //      IModifyAudited<TUserKey> entity1 = (IModifyAudited<TUserKey>)entity;
      //      //entity1.LastModifyId = _suktUser.Id a;
      //      entity1.LastModifyId = _httpContextAccessor.HttpContext.User?.Identity.GetUesrId<TUserKey>();
      //      entity1.LastModifedAt = DateTime.Now;
      //      return (TEntity)entity1;
      //  }

        #endregion 帮助方法
    }
}