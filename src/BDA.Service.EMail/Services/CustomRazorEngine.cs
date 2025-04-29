// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace BDA.Service.EMail.Services
{
    /// <summary>
    ///     Razor Engine
    /// </summary>
    public class CustomRazorEngine : ICustomRazorEngine
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="razorViewEngine">View Engine</param>
        /// <param name="tempDataProvider">Data Provider</param>
        /// <param name="serviceProvider">Service Provider</param>
        public CustomRazorEngine(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider
        )
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        ///     View holen
        /// </summary>
        /// <param name="viewName">View Name</param>
        /// <returns></returns>
        private IView FindView(string viewName)
        {
            var viewResult = _razorViewEngine.GetView(null, viewName, true);
            if (viewResult.Success)
            {
                return viewResult.View;
            }

            throw new Exception($"[{nameof(CustomRazorEngine)}]({nameof(FindView)}): Invalid View Path");
        }

        /// <summary>
        ///     Context holen
        /// </summary>
        /// <returns></returns>
        private ActionContext GetContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        #region Interface Implementations

        /// <summary>
        ///     RazorView in HTML wandeln
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> RazorViewToHtmlAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = GetContext();
            var view = FindView(viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary()
                    )
                    {
                        Model = model
                    },
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    output,
                    new HtmlHelperOptions()
                );
                await view.RenderAsync(viewContext).ConfigureAwait(true);
                return output.ToString();
            }
        }

        #endregion
    }
}