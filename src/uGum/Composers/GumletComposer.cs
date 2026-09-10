using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using uGum.Configuration;
using uGum.NotificationHandlers;
using uGum.Services;

namespace uGum.Composers;

public class GumletComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // load up the settings.
        var options = builder
            .Services.AddOptions<GumletSettings>()
            .Bind(builder.Config.GetSection(Constants.AppSettingsPath));

        options.ValidateDataAnnotations();

        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IGumletService, GumletService>();

        builder.Services.Configure<SwaggerGenOptions>(opt =>
        {
            opt.SwaggerDoc(
                Constants.Swagger.ApiName,
                new OpenApiInfo
                {
                    Title = Constants.Swagger.Title,
                    Version = Constants.Swagger.Version,
                }
            );
            opt.OperationFilter<GumletBackofficeOperationSecurityFilter>();
        });

        // media
        builder.AddNotificationAsyncHandler<MediaSavingNotification, MediaNotifications>();
        builder.AddNotificationAsyncHandler<MediaDeletedNotification, MediaNotifications>();

        // content
        builder.AddNotificationHandler<ContentCopyingNotification, ContentNotifications>();
        builder.AddNotificationAsyncHandler<
            ContentDeletedBlueprintNotification,
            ContentNotifications
        >();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentNotifications>();
        builder.AddNotificationHandler<ContentSavedBlueprintNotification, ContentNotifications>();
        builder.AddNotificationAsyncHandler<ContentSavingNotification, ContentNotifications>();

        // member
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, MemberNotifications>();
        builder.AddNotificationAsyncHandler<MemberSavingNotification, MemberNotifications>();

        builder.Services.AddSingleton<IOperationIdHandler, CustomOperationHandler>();
    }

    public class GumletBackofficeOperationSecurityFilter
        : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => Constants.Swagger.ApiName;
    }

    // This is used to generate nice operation IDs in our swagger json file
    // So that the gnerated TypeScript client has nice method names and not too verbose
    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
    public class CustomOperationHandler : OperationIdHandler
    {
        public CustomOperationHandler(IOptions<ApiVersioningOptions> apiVersioningOptions)
            : base(apiVersioningOptions) { }

        protected override bool CanHandle(
            ApiDescription apiDescription,
            ControllerActionDescriptor controllerActionDescriptor
        )
        {
            return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(
                "uGum.Controllers",
                comparisonType: StringComparison.InvariantCultureIgnoreCase
            )
                is true;
        }

        public override string Handle(ApiDescription apiDescription) =>
            $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}
