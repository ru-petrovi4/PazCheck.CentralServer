using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4;
using System.Threading;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using System.Reflection;
using Ssz.Utils.Logging;
using Microsoft.AspNetCore.Http;

namespace Simcode.PazCheck.CentralServer.MicroServices
{
    /// <summary>
    /// Default refresh token store.
    /// </summary>
    public class RefreshTokenStore : DefaultGrantStore<RefreshToken>, IRefreshTokenStore, IDisposable
    {
        #region construction and destruction
        
        public RefreshTokenStore(
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            IHttpContextAccessor httpContextAccessor,
            IInformationSecurityEventsLogger informationSecurityEventsLogger,
            ILogger<RefreshTokenStore> logger)
            : base(IdentityServerConstants.PersistedGrantTypes.RefreshToken, store, serializer, handleGenerationService, logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _informationSecurityEventsLogger = informationSecurityEventsLogger;

            //_timer = new Timer(OnTimerCallback, null, 5000, 5000);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) return;

            //if (disposing)
            //{
            //    _timer.Dispose();
            //}

            _disposed = true;
        }

        ~RefreshTokenStore()
        {
            Dispose(false);
        }

        public bool Disposed => _disposed;

        #endregion        

        #region public functions

        /// <summary>
        /// Stores the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        public async Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken)
        {
            return await CreateItemAsync(refreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.SessionId, refreshToken.Description, refreshToken.CreationTime, refreshToken.Lifetime);
        }

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        public Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken)
        {
            return StoreItemAsync(handle, refreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.SessionId, refreshToken.Description, refreshToken.CreationTime, refreshToken.CreationTime.AddSeconds(refreshToken.Lifetime), refreshToken.ConsumedTime);
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
        {
            return GetItemAsync(refreshTokenHandle);
        }

        /// <summary>
        /// Removes the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
        {
            return RemoveItemAsync(refreshTokenHandle);
        }

        /// <summary>
        /// Removes the refresh tokens.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            return RemoveAllAsync(subjectId, clientId);
        }

        #endregion

        //#region private functions

        //private void OnTimerCallback(object? state)
        //{
        //    if (Disposed) return;

        //    try
        //    {                
        //    }
        //    catch
        //    {
        //    }
        //}        

        //#endregion

        #region private fields

        private volatile bool _disposed;          
        //private Timer _timer;
        private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion
    }
}