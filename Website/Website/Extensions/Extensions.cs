using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Website
{
    public static class APIExtensions
    {
        public static string ToPrettyInterval(this DateTime time)
        {
            var now = DateTime.Now;
            var interval = now - time;
            var years = interval.Days / 365; // close enough
            var months = interval.Days / 30; // fuck a september, april, june, november, and february
            if (years > 0)
                return $"{years} year{(years > 1 ? "s" :"")} ago";
            if (months > 0)
                return $"{months} month{(months > 1 ? "s" : "")} ago";
            if (interval.Days > 0)
                return $"{interval.Days} day{(interval.Days > 1 ? "s" : "")} ago";
            if (interval.Hours > 0)
                return $"{interval.Hours} hour{(interval.Hours > 1 ? "s" : "")} ago";
            if (interval.Minutes > 0)
                return $"{interval.Minutes} minute{(interval.Minutes > 1 ? "s" : "")} ago";
            return interval.Seconds > 15
                ? "less than a minute ago"
                : "just now";
        }

        public static string GetDisplayName(this Enum val)
        {
            var member = val.GetType().GetMember(val.ToString())[0];
            var nameAttr = member.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault();
            return (nameAttr as DescriptionAttribute)?.Description ?? val.ToString();
        }

        public static IHttpActionResult WithReason(this IHttpActionResult result, string reason)
        {
            return new WithReasonHttpActionResult(result, reason);
        }

        private class WithReasonHttpActionResult : IHttpActionResult
        {
            private readonly IHttpActionResult _result;
            private readonly string _reason;
            public WithReasonHttpActionResult(IHttpActionResult result, string reason)
            {
                _result = result;
                _reason = reason;
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var res = await _result.ExecuteAsync(cancellationToken);
                res.ReasonPhrase = _reason;
                return res;
            }
        }
    }
}