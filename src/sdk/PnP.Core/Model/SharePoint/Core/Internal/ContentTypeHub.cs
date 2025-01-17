﻿using PnP.Core.Services;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Content Type Hub class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = "_api/Web")]
    internal sealed class ContentTypeHub : BaseDataModel<IContentTypeHub>, IContentTypeHub
    {
        #region Construction
        public ContentTypeHub()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                var request = api.ApiCall.Request.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);

                return api;
            };
        }
        #endregion

        #region Properties

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        internal string SiteId { get; set; }

        #endregion

        #region Methods

        public async Task<string> GetSiteIdAsync()
        {
            if (SiteId != null)
            {
                return SiteId;
            }

            var apiCall = BuildSiteIdApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new MicrosoftGraphServiceException(PnPCoreResources.Exception_RetrievingContentTypeHubSiteId);
            }

            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            if (json.TryGetProperty("id", out JsonElement id))
            {
                SiteId = id.GetString();
            }

            return SiteId;
        }

        private ApiCall BuildSiteIdApiCall()
        {
            return new ApiCall($"sites/{PnPContext.Uri.Host}:/sites/contenttypehub?$select=id", ApiType.Graph);
        }

        public string GetSiteId()
        {
            return GetSiteIdAsync().GetAwaiter().GetResult();
        }

        #endregion
    }
}
