# uGum

uGum connects Umbraco media with [Gumlet Video](https://www.gumlet.com/video/). When an editor saves a video in Umbraco, uGum uploads it to Gumlet and stores the resulting asset ID and HLS playback URL on the Umbraco item.

This gives editors a familiar Umbraco workflow while Gumlet handles video processing and delivery.

## What it does

- Adds a **Gumlet Sync** property editor for Umbraco.
- Uploads video files to Gumlet when an item is saved.
- Keeps the Gumlet asset reference and playback URL on the Umbraco item.
- Removes the corresponding Gumlet asset when the Umbraco item or video is deleted.
- Shows the Gumlet asset ID, playback URL, and processing status in the backoffice.
- Works with media, content, and member items.

## Requirements

- Umbraco CMS 18 or newer.
- A Gumlet Video workspace.
- A Gumlet API key with permission to create, read, and delete video assets.
- The Gumlet video source ID used for ingestion.

## Installation

Install the package in your Umbraco project:

```sh
dotnet add package Umbraco.Community.uGum
```

The package registers the property editor and notification handlers automatically.

## Configuration

Add the Gumlet settings to `appsettings.json`:

```json
{
  "Umbraco": {
    "Gumlet": {
      "ApiKey": "YOUR_GUMLET_API_KEY",
      "SourceId": "YOUR_GUMLET_SOURCE_ID",
      "Format": "hls",
      "Resolution": [
        "240p",
        "360p",
        "480p",
        "720p",
        "1080p"
      ],
      "KeepOriginal": false
    }
  }
}
```

Keep the API key server-side and out of source control. The key can also be supplied through the `Umbraco__Gumlet__ApiKey` environment variable. `Format` defaults to `hls`, `Resolution` controls the renditions Gumlet creates, and `KeepOriginal` controls whether Gumlet keeps the original upload.

## Set up the sync property

1. Open the media, content, or member type that contains the video upload property.
2. Add a property using the **Gumlet Sync** editor.
3. Enter the alias of the upload property in **Upload Property Alias**.
4. Save the type, then upload or save a video item.

The upload property and Gumlet Sync property must be on the same type.

![Gumlet Sync property editor configuration](https://raw.githubusercontent.com/skttl/umbraco-ugum/main/docs/umbraco_data_type.png)

After the item is saved, uGum uploads the video and displays the Gumlet asset information in the backoffice.

![Umbraco media item synchronized with Gumlet](https://raw.githubusercontent.com/skttl/umbraco-ugum/main/docs/umbraco_media.png)

## Use the playback URL

The property exposes a `GumletValue` with `Src`, `GumletAssetId`, and `PlaybackUrl`. Use `PlaybackUrl` with an HLS-capable video player:

```cshtml
@if (Model.Video?.GumletVideo is { PlaybackUrl: not null } video)
{
    <video controls src="@video.PlaybackUrl"></video>
}
```

For browsers without native HLS support, use a player such as [hls.js](https://github.com/video-dev/hls.js).

For Gumlet API key setup and source IDs, see the [full documentation](https://github.com/skttl/umbraco-ugum/blob/main/.github/README.md).
