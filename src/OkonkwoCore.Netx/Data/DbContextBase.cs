using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OkonkwoCore.Netx.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OkonkwoCore.Netx.Data
{
    public abstract class DbContextBase : DbContext
    {
        Type _seedEntityTypeConfigurationType;

        protected DbContextBase(DbContextOptions options, Type seedEntityTypeConfigurationType)
           : base(options)
        {
            _seedEntityTypeConfigurationType = seedEntityTypeConfigurationType;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ignore properties associated with these types and interfaces
            modelBuilder.Ignore<PropertyChangedEventHandler>();
            modelBuilder.Ignore<ExtensionDataObject>();
            modelBuilder.Ignore<IIdentifiableEntity>();

            modelBuilder.ApplyConfigurationsFromAssembly(_seedEntityTypeConfigurationType.Assembly);

            // ALWAYS 
            base.OnModelCreating(modelBuilder);
        }

        protected virtual string GetUser()
        {
            return null;
        }

        protected virtual void BeforeSaveChanges()
        {
            _saveChangesTime = DateTime.UtcNow;

            #region set DateCreated and DateModified

            void setCreatedOrModifiedDate(EntityEntry entry)
            {
                if (entry.Entity is IChangesTrackedEntity changedOrAddedItem)
                {
                    if (entry.State == EntityState.Added)
                    {
                        changedOrAddedItem.DateCreated = _saveChangesTime.Value;
                        changedOrAddedItem.CreatedBy = GetUser();
                    }
                    else
                    {
                        changedOrAddedItem.DateCreated = (DateTime)entry.OriginalValues["DateCreated"];
                        changedOrAddedItem.CreatedBy = (string)entry.OriginalValues["CreatedBy"];

                        changedOrAddedItem.DateModified = _saveChangesTime.Value;
                        changedOrAddedItem.ModifiedBy = GetUser();
                    }
                }
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
                setCreatedOrModifiedDate(entry);
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
                setCreatedOrModifiedDate(entry);

            #endregion
        }

        protected DateTime? _saveChangesTime { get; set; }

        public override int SaveChanges()
        {
            int saveResult = default(int);

            try
            {
                BeforeSaveChanges();

                saveResult = base.SaveChanges();

                AfterSaveChanges();
            }
            catch (Exception ex)
            {
                SaveChangesCatch(ex);
            }
            finally
            {
                SaveChangesFinally();
            }

            return saveResult;
        }

        protected virtual void AfterSaveChanges()
        {
        }

        protected virtual void SaveChangesCatch(Exception ex)
        {
            if (ex is ValidationException validationException)
            {
                var errors = validationException.Data;
                var result = new StringBuilder();
                var allErrors = new List<ValidationResult>();

                //foreach (var error in errors)
                //    foreach (var validationError in error.ValidationErrors)
                //    {
                //        result.Append($"\r\n  Entity of type {error.Entry.Entity.GetType().ToString()} has validation error \"{validationError.ErrorMessage}\" for property {validationError.PropertyName}.\r\n");
                //        if (error.Entity is IIdentifiableEntity domainEntity)
                //        {
                //            result.Append((domainEntity.Id == default)
                //               ? "  This entity was created in this session.\r\n"
                //               : $"  The id of the entity is {domainEntity.Id}.\r\n");
                //        }
                //        allErrors.Add(new ValidationResult(validationError.ErrorMessage, new[] { validationError.PropertyName }));
                //    }

                //var fault = new EntityValidationFault(result.ToString(), allErrors);
                //throw new FaultException<EntityValidationFault>(fault, fault.Message);
            }
            else if (ex is DbUpdateException dbUpdateException)
            {
                var sqlException = dbUpdateException?.InnerException as SqlException;

                if (sqlException != null)
                {
                    // create enums for these
                    // sqlException.Number == SqlServerViolationOfUniqueIndex

                    var result = new StringBuilder("The following errors during the context.SaveChanges() operation: \r\n");

                    foreach (SqlError error in sqlException.Errors)
                    {
                        string errString = error.Message;
                        result.Append("\r\n" + error.Message);
                    }

                    // replace this with CoreException?
                    throw new Exception(result.ToString());
                }
            }
        }

        protected virtual void SaveChangesFinally()
        {
            // done .. clear this
            _saveChangesTime = null;
        }
    }
}
