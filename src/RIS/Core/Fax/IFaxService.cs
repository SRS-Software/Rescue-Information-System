#region

using System;
using RIS.Core.Fax;

#endregion

namespace RIS.Core
{
    public interface IFaxService : IService
    {
        #region Functions

        string TestOcr(string path);

        #endregion //Functions

        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<EinsatzCreatedEventArgs> EinsatzCreated;

        #endregion //Events

        #region Properties

        #endregion //Properties
    }
}