using Microsoft.AspNet.SignalR;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Hubs
{
    public abstract class RazorHub : Hub
    {
        protected string Render(string view, object model)
        {
            return Render(view, GetType().Name, model);
        }

        protected static string Render(string view, string hubName, object model)
        {
            var config = GetConfiguration(hubName);
            var service = RazorEngineService.Create(config);
            return service.RunCompile(view, null, model);
        }

        private static ICachingProvider _cache;
        static RazorHub()
        {
#if DEBUG
            _cache = new FileWatchingCachingProvider();
#else
            _cache = new DefaultCachingProvider();
#endif
        }


        private static TemplateServiceConfiguration GetConfiguration(string name)
        {
            return new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(MvcTemplateBase<>),
                TemplateManager = new ResolvePathTemplateManager(GetPaths(name)),
                CachingProvider = _cache
            };
        }

        private static IEnumerable<string> GetPaths(string name)
        {
            var hub = name.EndsWith("Hub")
                ? name.Substring(0, name.Length - 3)
                : name;
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "Shared");
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", hub);
        }
    }

    public class FileWatchingCachingProvider : ICachingProvider
    {
        private InvalidatingCachingProvider _inner;
        private IDictionary<ITemplateKey, DateTime> _asOf;

        public FileWatchingCachingProvider()
        {
            _inner = new InvalidatingCachingProvider();
            _asOf = new Dictionary<ITemplateKey, DateTime>();
        }

        public TypeLoader TypeLoader
        {
            get { return _inner.TypeLoader; }
        }

        public void CacheTemplate(ICompiledTemplate template, ITemplateKey key)
        {
            var fullPath = ((FullPathTemplateKey)key).FullPath;
            _asOf[key] = File.GetLastWriteTime(fullPath);
            _inner.CacheTemplate(template, key);
        }

        public bool TryRetrieveTemplate(ITemplateKey key, Type modelType, out ICompiledTemplate template)
        {
            if (!_inner.TryRetrieveTemplate(key, modelType, out template))
                return false;

            var fullPath = ((FullPathTemplateKey)key).FullPath;
            DateTime cachedAsOf;
            return _asOf.TryGetValue(key, out cachedAsOf)
                && cachedAsOf == File.GetLastWriteTime(fullPath);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }

    public class MvcTemplateBase<T> : HtmlTemplateBase<T>, IViewDataContainer
    {
        public HtmlHelper<T> Html { get; private set; }

        public UrlHelper Url { get; private set; }

        private ViewDataDictionary _viewData;
        public ViewDataDictionary ViewData
        {
            get
            {
                return _viewData
                    ?? (_viewData = new ViewDataDictionary(Model));
            }
            set { _viewData = value; }
        }

        public MvcTemplateBase()
        {
            Html = new HtmlHelper<T>(new ViewContext(), this);
            Url = new UrlHelper(HttpContext.Current.Request.RequestContext);
        }
    }

}