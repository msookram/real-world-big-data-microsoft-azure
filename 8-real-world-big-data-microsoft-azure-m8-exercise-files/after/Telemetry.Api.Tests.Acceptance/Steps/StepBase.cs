using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TechTalk.SpecFlow;

namespace Telemetry.Api.Tests.Acceptance.Steps
{
    public abstract class StepBase
    {
        protected string DeviceId
        {
            get { return GetContextValue("DeviceId"); }
            set { ScenarioContext.Current["DeviceId"] = value; }
        }

        protected string RequestContentFilename
        {
            get { return GetContextValue("RequestContentFilename"); }
            set { ScenarioContext.Current["RequestContentFilename"] = value; }
        }

        protected HttpResponseMessage ApiResponse
        {
            get { return GetContextValue<HttpResponseMessage>("HttpResponseMessage"); }
            set { ScenarioContext.Current["HttpResponseMessage"] = value; }
        }

        protected bool IsRequestCompressed
        {
            get { return GetContextValue<bool>("IsRequestCompressed"); }
            set { ScenarioContext.Current["IsRequestCompressed"] = value; }
        }

        protected int? ModifiedContentId
        {
            get { return GetContextValue<int?>("ModifiedContentId"); }
            set { ScenarioContext.Current["ModifiedContentId"] = value; }
        }

        private string GetContextValue(string key)
        {
            var value = GetContextValue<string>(key);
            return value ?? string.Empty;
        }

        private T GetContextValue<T>(string key)
        {
            if (ScenarioContext.Current != null && ScenarioContext.Current.ContainsKey(key))
            {
                return ScenarioContext.Current.Get<T>(key);
            }
            return default(T);
        }

        protected static void AssertObjectContainsTheExpectedFieldsAndValues(object obj, Table table)
        {
            var properties = new List<KeyValuePair<string, JToken>>();
            var rawProperties = (ICollection<KeyValuePair<string, JToken>>)obj;
            //flatten nested objects:
            var prefix = string.Empty;
            foreach (var property in rawProperties)
            {
                AddFlattenedProperty(property, prefix, properties);
            }
            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                var value = row["Value"];
                Assert.AreEqual(1, properties.Count(x => x.Key == field && x.Value.ToString() == value), "Expected field: {0} with value: {1}", field, value);
            }
        }

        protected string GetApiResponse(string relativeUrl, params object[] args)
        {
            ApiResponse = null;
            var requestUrl = ConfigurationManager.AppSettings["Api.BaseUrl"] + string.Format(relativeUrl, args);
            using (var client = new HttpClient())
            {
                AddDeviceHeaders(client);
                var getTask = client.GetAsync(requestUrl);
                getTask.Wait();
                ApiResponse = getTask.Result;
                var readTask = getTask.Result.Content.ReadAsStringAsync();
                readTask.Wait();
                return readTask.Result;
            }
        }

        private Stream GetRequestContent()
        {
            var requestContentPath = Path.Combine(Environment.CurrentDirectory, @"Resources\Requests", RequestContentFilename);
            return File.OpenRead(requestContentPath);
        }

        protected string GetRequestContentString()
        {
            var content = string.Empty;
            using (var contentStream = GetRequestContent())
            {
                using (var reader = new StreamReader(contentStream))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }

        protected void PostApiRequest(string relativeUrl, params object[] args)
        {
            ApiResponse = null;
            using (var content = GetRequestContent())
            {
                var requestUrl = ConfigurationManager.AppSettings["Api.BaseUrl"] + string.Format(relativeUrl, args);
                using (var client = new HttpClient())
                {
                    AddDeviceHeaders(client);
                    var requestContent = new StreamContent(content);
                    var contentType = IsRequestCompressed ? "application/gzip" : "application/json";
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    var postTask = client.PostAsync(requestUrl, requestContent);
                    postTask.Wait();
                    ApiResponse = postTask.Result;
                }
            }
        }

        private void AddDeviceHeaders(HttpClient client)
        {
            if (!string.IsNullOrEmpty(DeviceId))
            {
                client.DefaultRequestHeaders.Add("x-device-id", DeviceId);
            }
        }

        private static void AddFlattenedProperty(KeyValuePair<string, JToken> property, string prefix, List<KeyValuePair<string, JToken>> properties)
        {
            if (property.Value.Type == JTokenType.Object)
            {
                var nestedProperties = (ICollection<KeyValuePair<string, JToken>>)property.Value;
                prefix = prefix + property.Key + ".";
                foreach (var nestedProperty in nestedProperties)
                {
                    AddFlattenedProperty(nestedProperty, prefix, properties);
                }
            }
            else
            {
                properties.Add(new KeyValuePair<string, JToken>(prefix + property.Key, property.Value));
            }
        }
    }
}
