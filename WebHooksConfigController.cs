using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Contracts;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Infrastructure.Apps;

namespace Webhooks
{
    [AppType(AppTypeEnum.Config),
    AppControlGuid("FF74C706-49BA-4D81-8F4D-A55C26D2CADF"),
    AppKey("Webhooks"),
    AppGuid(AppConstants.AppId),
    AppAuthor("Countersoft"),
    AppName("Webhooks"),
    AppDescription("Call web hooks when items are create and / or updated"),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class WebHooksConfigController : BaseAppController
    {
        public override WidgetResult Caption(Countersoft.Gemini.Commons.Dto.IssueDto issue = null)
        {
            WidgetResult result = new WidgetResult();
            result.Success = true;
            result.Markup.Html = "Webhooks";

            return result;
        }

        public override WidgetResult Show(Countersoft.Gemini.Commons.Dto.IssueDto issue = null)
        {
            return new WidgetResult();
        }

        public override WidgetResult Configuration()
        {
            var data = GetData();
            WidgetResult result = new WidgetResult();
            result.Markup = new WidgetMarkup("views/webhooks.cshtml", data.Value);
            result.Success = true;

            return result;
        }

        [AppUrl("getproperty")]
        public ActionResult GetProperty(string id, string property)
        {
            var data = GetData();
            var row = data.Value.Find(u => u.Url.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if (row == null) return JsonError();

            switch (property.ToLowerInvariant())
            {
                case "description":
                    return Content(row.Description);
                case "url":
                    return Content(row.Url);
                case "create":
                    return Content(row.Create.ToString());
                case "update":
                    return Content(row.Update.ToString());
            }

            GeminiContext.GlobalConfigurationWidgetStore.Save<List<WebhooksData>>(AppConstants.AppId, data.Value);

            return JsonSuccess();
        }

        [AppUrl("saveproperty")]
        public ActionResult SaveProperty(string id, string property, string value)
        {
            var data = GetData();
            var row = data.Value.Find(u => u.Url.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if (row == null) return JsonError();
            string returnData = value;
            switch (property.ToLowerInvariant())
            {
                case "description":
                    row.Description = value;
                    break;
                case "url":
                    row.Url = value;
                    break;
                case "create":
                    row.Create = value.ToBool();
                    returnData = row.Create ? "Yes" : string.Empty;
                    break;
                case "update":
                    row.Update = value.ToBool();
                    returnData = row.Update ? "Yes" : string.Empty;
                    break;
            }

            GeminiContext.GlobalConfigurationWidgetStore.Save<List<WebhooksData>>(AppConstants.AppId, data.Value);

            return Content(returnData);
        }

        [AppUrl("save")]
        public ActionResult Save(WebhooksData hook)
        {
            var data = GetData();
            if (data.Value.Find(u => u.Url.Equals(hook.Url, StringComparison.InvariantCultureIgnoreCase)) != null) return JsonError("Duplicate Entry");
            
            data.Value.Add(hook);
            GeminiContext.GlobalConfigurationWidgetStore.Save<List<WebhooksData>>(AppConstants.AppId, data.Value);

            return JsonSuccess();
        }

        [AppUrl("delete")]
        public ActionResult Delete(string url)
        {
            var data = GetData();
            var dataUrl = data.Value.Find(u => u.Url.Equals(url, StringComparison.InvariantCultureIgnoreCase));
            if (dataUrl == null)
            {
                return JsonError("Cannot find url");
            }

            data.Value.Remove(dataUrl);

            GeminiContext.GlobalConfigurationWidgetStore.Save<List<WebhooksData>>(AppConstants.AppId, data.Value);

            return JsonSuccess();
        }


        private GlobalConfigurationWidgetData<List<WebhooksData>> GetData()
        {
            var data = GeminiContext.GlobalConfigurationWidgetStore.Get<List<WebhooksData>>(AppConstants.AppId);
            if (data == null || data.Value == null)
            {
                data = new GlobalConfigurationWidgetData<List<WebhooksData>>();
                data.Value = new List<WebhooksData>();
            }

            return data;
        }

    }
}
