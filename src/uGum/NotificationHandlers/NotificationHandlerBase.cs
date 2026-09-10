using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uGum.Models;
using uGum.Services;

namespace uGum.NotificationHandlers;

public abstract class NotificationHandlerBase
{
    private readonly ILogger<NotificationHandlerBase> _logger;
    private readonly IDataTypeService _dataTypeService;
    private readonly IGumletService _gumletService;
    private readonly MediaFileManager _mediaFileManager;

    public NotificationHandlerBase(
        ILogger<NotificationHandlerBase> logger,
        IDataTypeService dataTypeService,
        IGumletService gumletService,
        MediaFileManager mediaFileManager)
    {
        _logger = logger;
        _dataTypeService = dataTypeService;
        _gumletService = gumletService;
        _mediaFileManager = mediaFileManager;
    }

    public async Task<bool> TryDeleteSyncedUploadFilesFromGumlet(IContentBase node)
    {
        try
        {
            return await DeleteSyncedUploadFilesFromGumlet(node);
        }
        catch
        {
            _logger.LogError(
                "An error occurred while deleting synced upload files from Gumlet for content with ID {ContentId}",
                node.Id
            );
            return false;
        }
    }

    public async Task<bool> TrySyncUploadFilesToGumlet(IContentBase node)
    {
        try
        {
            return await SyncUploadFilesToGumlet(node);
        }
        catch
        {
            _logger.LogError(
                "An error occurred while syncing upload files to Gumlet for content with ID {ContentId}",
                node.Id
            );
            return false;
        }
    }

    /// <summary>
    /// Syncs upload files to Gumlet when the content has uGum sync properties.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>boolean indicating if any changes were made</returns>
    public async Task<bool> SyncUploadFilesToGumlet(IContentBase node)
    {
        var isUpdated = false;

        var gumletSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var gumletSyncProperty in gumletSyncProperties)
        {
            var dataType = await _dataTypeService.GetAsync(
                gumletSyncProperty.PropertyType.DataTypeKey
            );

            if (
                dataType is null
                || dataType.ConfigurationData.TryGetValue(
                    Constants.UploadPropertyAlias,
                    out var value
                )
                    is false
                || value is not string uploadPropertyAlias
                || string.IsNullOrWhiteSpace(uploadPropertyAlias)
            )
            {
                continue;
            }

            var existingStringValue = node.GetValue<string>(gumletSyncProperty.Alias);

            var existingValue =
                existingStringValue.IsNullOrWhiteSpace() is false
                && existingStringValue.StartsWith("{")
                    ? JsonSerializer.Deserialize<GumletValue>(existingStringValue)
                    : null;

            var canContinue =
                node.IsPropertyDirty(uploadPropertyAlias)
                || existingValue?.Src != node.GetValue<string>(uploadPropertyAlias);

            if (canContinue == false)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(existingValue?.GumletAssetId) is false)
            {
                await _gumletService.DeleteAsset(existingValue.GumletAssetId);
                node.SetValue(gumletSyncProperty.Alias, null);
                isUpdated = true;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var fileStream = _mediaFileManager.GetFile(node, out var _, uploadPropertyAlias);
                fileStream.CopyTo(ms);
                var byteArray = ms.ToArray();

                if (byteArray is not null && byteArray.Length > 0)
                {
                    var asset = await _gumletService.CreateAsset(
                        byteArray,
                        node.Name,
                        node.CreatorId.ToString(),
                        node.GetUdi().ToString().EnsureEndsWith($"/{uploadPropertyAlias}")
                    );

                    node.SetValue(
                        gumletSyncProperty.Alias,
                        JsonSerializer.Serialize(
                            new GumletValue()
                            {
                                GumletAssetId = asset.AssetId,
                                PlaybackUrl = asset.Output?.PlaybackUrl,
                                Src = node.GetValue<string>(uploadPropertyAlias),
                            }
                        )
                    );

                    isUpdated = true;
                }
            }
        }

        return isUpdated;
    }

    /// <summary>
    /// Deletes the files from Gumlet if the content has any uGum sync properties.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>boolean indicating if any changes were made</returns>
    public async Task<bool> DeleteSyncedUploadFilesFromGumlet(IContentBase node)
    {
        var isUpdated = false;

        var gumletSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var gumletSyncProperty in gumletSyncProperties)
        {
            var dataType = await _dataTypeService.GetAsync(
                gumletSyncProperty.PropertyType.DataTypeKey
            );

            var existingStringValue = node.GetValue<string>(gumletSyncProperty.Alias);

            var existingValue =
                existingStringValue.IsNullOrWhiteSpace() is false
                && existingStringValue.StartsWith("{")
                    ? JsonSerializer.Deserialize<GumletValue>(existingStringValue)
                    : null;

            if (existingValue is null || existingValue.GumletAssetId.IsNullOrWhiteSpace())
            {
                continue;
            }

            await _gumletService.DeleteAsset(existingValue.GumletAssetId);
            node.SetValue(gumletSyncProperty.Alias, null);
            isUpdated = true;
        }

        return isUpdated;
    }

    public static bool ResetGumletValuesWithoutDeleting(IContentBase node)
    {
        var isUpdated = false;

        var gumletSyncProperties = node.Properties.Where(x =>
            x.PropertyType.PropertyEditorAlias == Constants.PropertyEditorSchema
        );

        foreach (var gumletSyncProperty in gumletSyncProperties)
        {
            node.SetValue(gumletSyncProperty.Alias, null);
            isUpdated = isUpdated || node.IsPropertyDirty(gumletSyncProperty.Alias);
        }

        return isUpdated;
    }
}
