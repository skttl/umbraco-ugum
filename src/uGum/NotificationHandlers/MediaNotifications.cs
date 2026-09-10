using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using uGum.Services;

namespace uGum.NotificationHandlers;

public class MediaNotifications
    : NotificationHandlerBase,
        INotificationAsyncHandler<MediaSavingNotification>,
        INotificationAsyncHandler<MediaDeletedNotification>
{
    public MediaNotifications(
        ILogger<MediaNotifications> logger,
        IDataTypeService dataTypeService,
        IGumletService gumletService,
        MediaFileManager mediaFileManager
    )
        : base(logger, dataTypeService, gumletService, mediaFileManager) { }

    public async Task HandleAsync(
        MediaSavingNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.SavedEntities.Select(TrySyncUploadFilesToGumlet));

    public async Task HandleAsync(
        MediaDeletedNotification notification,
        CancellationToken cancellationToken
    ) => await Task.WhenAll(notification.DeletedEntities.Select(TryDeleteSyncedUploadFilesFromGumlet));
}
