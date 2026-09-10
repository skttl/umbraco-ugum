using Microsoft.Extensions.DependencyInjection;
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

        builder.AddBackOfficeOpenApiDocument(
            Constants.Swagger.ApiName,
            document =>
                document
                    .WithTitle(Constants.Swagger.Title)
                    .WithUiTitle(Constants.Swagger.Title)
                    .WithBackOfficeAuthentication()
        );

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

    }
}
