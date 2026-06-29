using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Ssz.IdentityServer;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Simcode.PazCheck.CentralServer.Presentation;

public class GrafanaProxyMiddleware
{
    #region construction and destruction

    public GrafanaProxyMiddleware(
        RequestDelegate nextMiddleware,
        IConfiguration configuration,
        ITokenValidator tokenValidator,
        ITokenResponseGenerator tokenResponseGenerator,            
        IRefreshTokenService refreshTokenService,
        IEnumerable<Client> clients,
        Cache cache)
    {
        _nextMiddleware = nextMiddleware;
        _tokenValidator = tokenValidator;
        _tokenResponseGenerator = tokenResponseGenerator;            
        _refreshTokenService = refreshTokenService;
        _clients = clients;
        _cache = cache;

        _grafanaUrl = ConfigurationHelper.GetValue(configuration, PazCheckConstants.ConfigurationKey_GrafanaUrl, @"");
        if (_grafanaUrl != @"")
        {
            int i = _grafanaUrl.IndexOf(@"//");
            if (i == -1)
                throw new Exception(@"Invalid config: " + PazCheckConstants.ConfigurationKey_GrafanaUrl);
            i = _grafanaUrl.IndexOf('/', i + 2);
            if (i == -1)
                throw new Exception(@"Invalid config: " + PazCheckConstants.ConfigurationKey_GrafanaUrl);
            _grafanaUrl_Base = _grafanaUrl.Substring(0, i);
            _grafanaUrl_Path = _grafanaUrl.Substring(i);
        }
        else
        {
            _grafanaUrl_Base = @"";
            _grafanaUrl_Path = @"";
        }
    }

    #endregion

    #region public functions

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        if (request.Path.Value is null || _grafanaUrl_Path == @"" || !request.Path.Value.StartsWith(_grafanaUrl_Path))
        {
            await _nextMiddleware(context);
            return;
        }

        bool setAuthCookies = false;

        var uri = new Uri(_grafanaUrl_Base + request.Path.Value + request.QueryString);
        var queryNameValueCollection = HttpUtility.ParseQueryString(uri.Query);
        var authTokenString = queryNameValueCollection["authToken"];
        var refreshTokenString = queryNameValueCollection["refreshToken"];
        queryNameValueCollection.Remove("authToken");
        queryNameValueCollection.Remove("refreshToken");
        if (String.IsNullOrEmpty(authTokenString))
        {
            context.Request.Cookies.TryGetValue("authToken", out authTokenString);
            context.Request.Cookies.TryGetValue("refreshToken", out refreshTokenString);
        }
        else
        {
            setAuthCookies = true;
        }
        if (String.IsNullOrEmpty(authTokenString))
        {
            Debug.WriteLine("authToken is null. NOT redirected to grafana -> " + request.Path.Value);
            return;
        }            

        //Debug.WriteLine("redirected to grafana -> " + request.Path.Value);
        string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);
        var targetUri = new Uri(queryNameValueCollection.Count > 0
            ? String.Format("{0}?{1}", pagePathWithoutQueryString, queryNameValueCollection)
            : pagePathWithoutQueryString);

        var grafanaRequest = new HttpRequestMessage();

        var requestMethod = context.Request.Method;

        if (!HttpMethods.IsGet(requestMethod) &&
          !HttpMethods.IsHead(requestMethod) &&
          !HttpMethods.IsDelete(requestMethod) &&
          !HttpMethods.IsTrace(requestMethod))
        {
            var streamContent = new StreamContent(context.Request.Body);
            grafanaRequest.Content = streamContent;
        }

        foreach (var header in context.Request.Headers)
        {
            grafanaRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        TokenValidationResult accessTokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(authTokenString);        
        if (accessTokenValidationResult.Error == OidcConstants.ProtectedResourceErrors.ExpiredToken)
        {
            Client client = _clients.First();
            TokenValidationResult refreshTokenValidationResult = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenString, client);
            if (!refreshTokenValidationResult.IsError)
            {
                var validatedTokenRequest = new ValidatedTokenRequest()
                {
                    GrantType = OidcConstants.GrantTypes.RefreshToken,
                    RefreshToken = refreshTokenValidationResult.RefreshToken,
                    RefreshTokenHandle = refreshTokenString
                };
                validatedTokenRequest.SetClient(client);
                var tokenResponse = await _tokenResponseGenerator.ProcessAsync(new TokenRequestValidationResult(validatedTokenRequest));

                authTokenString = tokenResponse.AccessToken;
                refreshTokenString = tokenResponse.RefreshToken;
                accessTokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(authTokenString);
                setAuthCookies = true;
            }
        }

        if (!accessTokenValidationResult.IsError)
        {
            if (_cache.CheckAccess(HttpContextHelper.GetRoles(accessTokenValidationResult.Claims), "Method.WidgetsAdmin"))
                grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-USER", @"admin");
            else if (_cache.CheckAccess(HttpContextHelper.GetRoles(accessTokenValidationResult.Claims), "Method.WidgetsEditor"))
                grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-USER", @"editor");
            else if (_cache.CheckAccess(HttpContextHelper.GetRoles(accessTokenValidationResult.Claims), "Method.WidgetsViewer"))
                grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-USER", @"viewer");
        }

        //grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-NAME", @"editor3 name");
        //grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-ROLE", @"Editor");
        //grafanaRequest.Headers.TryAddWithoutValidation(@"X-WEBAUTH-EMAIL", @"editor3@mail.ru");

        grafanaRequest.RequestUri = targetUri;
        grafanaRequest.Headers.Host = targetUri.Host;
        grafanaRequest.Method = GetMethod(context.Request.Method);

        try
        {
            using (var grafanaResponse = await _httpClient.SendAsync(grafanaRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)grafanaResponse.StatusCode;

                foreach (var header in grafanaResponse.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in grafanaResponse.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                context.Response.Headers.Remove("transfer-encoding");

                if (setAuthCookies)
                {
                    context.Response.Cookies.Append("authToken", authTokenString);
                    context.Response.Cookies.Append("refreshToken", refreshTokenString ?? @"");
                }

                await grafanaResponse.Content.CopyToAsync(context.Response.Body);
            }
        }
        catch
        {
        }
    }

    #endregion

    #region private functions            

    private static HttpMethod GetMethod(string method)
    {
        if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
        if (HttpMethods.IsGet(method)) return HttpMethod.Get;
        if (HttpMethods.IsHead(method)) return HttpMethod.Head;
        if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
        if (HttpMethods.IsPost(method)) return HttpMethod.Post;
        if (HttpMethods.IsPut(method)) return HttpMethod.Put;
        if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
        return new HttpMethod(method);
    }        

    #endregion

    #region private fields

    private static readonly HttpClient _httpClient = new HttpClient();        

    private readonly RequestDelegate _nextMiddleware;
    private readonly ITokenValidator _tokenValidator;
    private readonly ITokenResponseGenerator _tokenResponseGenerator;
    private readonly IRefreshTokenService _refreshTokenService;        
    private readonly IEnumerable<Client> _clients;
    private readonly Cache _cache;

    private readonly string _grafanaUrl;        
    private readonly string _grafanaUrl_Base;
    private readonly string _grafanaUrl_Path;

    #endregion
}


//await _tokenCreationService.CreateTokenAsync(refreshTokenValidationResult.RefreshToken.AccessToken);