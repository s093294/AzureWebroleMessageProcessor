﻿
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using SInnovations.Azure.MessageProcessor.ApplicationInsights;
using SInnovations.Azure.MessageProcessor.ApplicationInsights.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Azure.MessageProcessor.Core.Notifications
{
    public static class MessageCompletedNotificationExtensions
    {
        public static async Task TrackMessageCompletedAsync(this MessageCompletedNotification notice, bool classAttributeRequired = true, bool inheritLookup = true)
        {

            if (!classAttributeRequired || Attribute.IsDefined(notice.Message.GetType(), typeof(ApplicationInsightsAttribute), inheritLookup))
            {
                TelemetryClient rtClient = new TelemetryClient();
                rtClient.TrackEvent(await notice.CreateEventTelemetryAsync());
            }
        }
        public static async Task<EventTelemetry> CreateEventTelemetryAsync(this MessageCompletedNotification notice, string name = null)
        {
            var t = new EventTelemetry(name ?? (Attribute.IsDefined(notice.Message.GetType(), typeof(ApplicationInsightsAttribute), true) ? 
                ((ApplicationInsightsAttribute)notice.Message.GetType().GetCustomAttributes(typeof(ApplicationInsightsAttribute), true)[0]).EventTelemetryName  ?? "MessageComplated"
                : "MessageCompleted"));
            
            t.Properties.Add("MessageId", notice.Message.MessageId);
            t.Properties.Add("MessageType", notice.Message.GetType().Name);
            t.Metrics.Add("Elapsed", notice.Elapsed.TotalMilliseconds);



            var props = notice.Message.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(ApplicationInsightsAttribute)));

            foreach (var prop in props)
            {
                ApplicationInsightsAttribute attr = (ApplicationInsightsAttribute)prop.GetCustomAttributes(typeof(ApplicationInsightsAttribute), true)[0];
                if (attr.EventTelemetryMetadataProviderType == null && !string.IsNullOrEmpty(attr.EventTelemetryMetadataProviderTypeName))
                {
                    attr.EventTelemetryMetadataProviderType = Type.GetType(attr.EventTelemetryMetadataProviderTypeName);
                }


                if (attr.EventTelemetryMetadataProviderType != null)
                {
                    var provider = notice.Resolver.GetHandler(attr.EventTelemetryMetadataProviderType) as IEventTelemetryMetadataProvider;
                    await provider.AddMetadataAsync(t, attr.PropertyName ?? prop.Name, prop.GetValue(notice.Message));
                }
                else
                {
                    t.Properties.Add(attr.PropertyName ?? prop.Name, prop.GetValue(notice.Message).ToString());
                }
            }




            return t;
        }
    }
}
