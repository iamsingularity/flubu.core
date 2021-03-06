﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using FlubuCore.Context;
using Microsoft.Web.Administration;

namespace FlubuCore.Tasks.Iis
{
    public class ControlAppPoolTask : TaskBase<int, IControlAppPoolTask>, IControlAppPoolTask
    {
        private readonly string _applicationPoolName;

        private readonly ControlApplicationPoolAction _action;

        private bool _failIfNotExist;
        private string _description;

        public ControlAppPoolTask(string applicationPoolName, ControlApplicationPoolAction action)
        {
            _applicationPoolName = applicationPoolName;
            _action = action;
        }

        protected override string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                {
                    return $"{_action.ToString()}s application pool {_applicationPoolName}.";
                }

                return _description;
            }

            set { _description = value; }
        }

        public IControlAppPoolTask FailIfNotExist()
        {
            _failIfNotExist = true;
            return this;
        }

        protected override int DoExecute(ITaskContextInternal context)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
                const string message = "Application pool '{0}' has been {1}ed.";
                foreach (ApplicationPool applicationPool in applicationPoolCollection)
                {
                    if (applicationPool.Name == _applicationPoolName)
                    {
                        string logMessage;
                        switch (_action)
                        {
                            case ControlApplicationPoolAction.Start:
                                {
                                    RunWithRetries(x => applicationPool.Start(), 3);
                                    logMessage = string.Format(CultureInfo.InvariantCulture, message, _applicationPoolName, _action);
                                    break;
                                }

                            case ControlApplicationPoolAction.Stop:
                                {
                                    RunWithRetries(
                                        x => applicationPool.Stop(),
                                        3,
                                        -2147023834 /*app pool already stopped*/);
                                    logMessage = string.Format(CultureInfo.InvariantCulture, message, _applicationPoolName, "stopp");
                                    break;
                                }

                            case ControlApplicationPoolAction.Recycle:
                                {
                                    RunWithRetries(x => applicationPool.Recycle(), 3);
                                    logMessage = string.Format(CultureInfo.InvariantCulture, message, _applicationPoolName, _action);
                                    break;
                                }

                            default:
                                throw new NotSupportedException();
                        }

                        serverManager.CommitChanges();

                        DoLogInfo(logMessage);
                        return 0;
                    }
                }

                string appPoolDoesNotExistMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Application pool '{0}' does not exist.",
                    _applicationPoolName);

                if (_failIfNotExist)
                    throw new TaskExecutionException(appPoolDoesNotExistMessage, 1);

                DoLogInfo(message);
                return 0;
            }
        }

        private static void RunWithRetries(
            Action<int> action,
            int retries,
            params long[] ignoredErrorCodes)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    action(0);
                    break;
                }
                catch (COMException ex)
                {
                    #if !NETSTANDARD1_6
                    for (int j = 0; j < ignoredErrorCodes.Length; j++)
                    {
                        if (ignoredErrorCodes[j] == ex.ErrorCode)
                        {
                            return;
                        }
                    }
                    #endif
                    if (i == retries - 1)
                        throw;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
