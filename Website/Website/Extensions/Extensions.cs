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