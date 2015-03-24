using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Countersoft.Gemini.Extensibility.Events;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Commons.Dto;
using Countersoft.Gemini.Contracts;
using Countersoft.Gemini.Extensibility;

namespace Webhooks
{
    [AppGuid(AppConstants.AppId)]
    [AppType(AppTypeEnum.Event)]
    [AppName("Webhooks")]
    [AppKey("Webhooks")]
    [AppDescription("Post data to a webhook")]
    public class IssueEventListener : AbstractIssueListener
    {
        private List<WebhooksData> GetData(GeminiContext context, bool create, bool update)
        {
            var data = context.GlobalConfigurationWidgetStore.Get<List<WebhooksData>>(AppConstants.AppId);
            if (data == null || data.Value == null)
            {
                data = new Countersoft.Gemini.Commons.Entity.GlobalConfigurationWidgetData<List<WebhooksData>>();
                data.Value = new List<WebhooksData>();
            }

            if (create && update)
            {
                data.Value.RemoveAll(a => !a.Create && !a.Update);
            }
            else if (create)
            {
                data.Value.RemoveAll(a => !a.Create);
            }
            else if (update)
            {
                data.Value.RemoveAll(a => !a.Update);
            }
            else
            {
                data.Value.RemoveAll(a => a.Create || a.Update);
            }

            return data.Value;
        }

        public override void AfterCreateFull(IssueDtoEventArgs args)
        {
            CallWebhook(GetData(args.Context, true, false), args.Issue);
        }

        public override void AfterUpdateFull(IssueDtoEventArgs args)
        {
            CallWebhook(GetData(args.Context, false, true), args.Issue);
        }

        private string GetIssueJson(IssueDto data)
        {
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter());
            serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            serializer.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;

            using (System.IO.TextWriter tw = new System.IO.StringWriter())
            {
                serializer.Serialize(tw, data);
                return tw.ToString();
            }
        }

        private void CallWebhook(List<WebhooksData> urls, IssueDto data)
        {
            WebClient client = new WebClient();
            var json = GetIssueJson(data);

            foreach (var url in urls)
            {
                try
                {
                    client.UploadString(url.Url, "POST", json);
                }
                catch (Exception ex)
                {
                    LogService.Logger.LogError(ex, string.Concat("Webhooks app - ", url.Url));
                }
            }
        }
    }
}

