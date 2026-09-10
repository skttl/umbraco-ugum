using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using uGum.Services;

namespace uGum.NotificationHandlers;

public class ContentNotifications
    : NotificationHandlerBase,
        INotificationHandler<ContentCopyingNotification>,
        INotificationAsyncHandler<ContentDeletedBlueprintNotification>,
        INotificationAsyncHandler<ContentDeletedNotification>,
        INotificationHandler<ContentSavedBlueprintNotification>,
        INotificationAsyncHandler<ContentSavingNotification>
{
    private readonly IContentService _contentService;

    public ContentNotifications(
        ILogger<ContentNotifications> logger,
        IContentService contentService,
        IDataTypeService dataTypeService,
        IGumletService gumletService,
        MediaFileManager mediaFileManager
    )
        : base(logger, dataTypeService, gumletService, mediaFileManager)
    {
        _contentService = contentService;
    }

    public void Handle(ContentCopyingNotification notification) =>
        ResetGumletValuesWithoutDeleting(notification.Copy);

    public async Task HandleAsync(
        ContentDeletedBlueprintNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedBlueprints.Select(TryDeleteSyncedUploadFilesFromGumlet));

    public async Task HandleAsync(
        ContentDeletedNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedEntities.Select(TryDeleteSyncedUploadFilesFromGumlet));

    public void Handle(ContentSavedBlueprintNotification notification)
    {
        if (ResetGumletValuesWithoutDeleting(notification.SavedBlueprint))
        {
            _contentService.SaveBlueprint(
                notification.SavedBlueprint,
                notification.CreatedFromContent
            );
        }
    }

    public async Task HandleAsync(
        ContentSavingNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.SavedEntities.Select(TrySyncUploadFilesToGumlet));
}
