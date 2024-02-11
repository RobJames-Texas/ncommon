using NCommon.Data;
using System;
using System.Web.Mvc;

namespace NCommon.Mvc
{
    public class UnitOfWorkAttribute : ActionFilterAttribute
    {
        public static readonly string ContextUnitOfWorkKey = "UnitOfWorkAttribute_Request_UnitOfWork";

        public FilterScope Scope { get; set; } = FilterScope.Action;

        public TransactionMode TransactionMode { get; set; } = TransactionMode.Default;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Start(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Commits the transaction if the filter scope is action and no errors have occured.
            if (filterContext.Exception != null)
            {
                //Rollback...
                CurrentUnitOfWork(filterContext).Dispose();
                return;
            }

            if (Scope != FilterScope.Action) 
                return;

            try
            {
                CurrentUnitOfWork(filterContext).Commit();
            }
            finally
            {
                CleanUp(filterContext);
            }
            
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (Scope != FilterScope.Result)
                return;

            if (filterContext.Exception != null)
            {
                //Rollback
                CurrentUnitOfWork(filterContext).Dispose();
                return;
            }

            //Commits the unit of work if the filter scope is Result and no errors have occured.
            try
            {
                CurrentUnitOfWork(filterContext).Commit();
            }
            finally
            {
                CleanUp(filterContext);
            }
            
        }

        public void Start(ControllerContext filterContext)
        {
            var unitOfWork = new UnitOfWorkScope(TransactionMode);
            filterContext.HttpContext.Items[ContextUnitOfWorkKey] = unitOfWork;
        }

        public void CleanUp(ControllerContext filterContext)
        {
            var unitOfWork = filterContext.HttpContext.Items[ContextUnitOfWorkKey] as IUnitOfWorkScope;
            if (unitOfWork != null)
            {
                unitOfWork.Dispose();
                filterContext.HttpContext.Items.Remove(ContextUnitOfWorkKey);
            }
        }

        public IUnitOfWorkScope CurrentUnitOfWork(ControllerContext filterContext)
        {
            var currentUnitOfWork = filterContext.HttpContext.Items[ContextUnitOfWorkKey] as IUnitOfWorkScope;
            if (currentUnitOfWork == null)
            {
                throw new InvalidOperationException("No unit of work scope was found for the current action." +
                                                    "This might indicate a possible bug in NCommon UnitOfWorkAttribute action filter.");
            }
            return currentUnitOfWork;
        }

        /// <summary>
        /// Defines the scope of the unit of work when executing in the context of an Action. Default is
        /// <see cref="Action"/>
        /// </summary>
        public enum FilterScope
        {
            /// <summary>
            /// Specifies that the unit of work scope will be comitted when the action finishes executing.
            /// </summary>
            Action,
            /// <summary>
            /// Specifies that the unit of work scope will be comitted when the view finishes rendering.
            /// </summary>
            Result
        }
    }
}