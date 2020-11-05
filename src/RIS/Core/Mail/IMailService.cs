#region

using System;
using RIS.Core.Mail;

#endregion

namespace RIS.Core
{
    public interface IMailService : IService
    {
        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<MailReceivedEventArgs> MailReceived;

        #endregion //Events

        #region Properties

        #endregion //Properties

        #region Functions

        #endregion //Functions
    }
}