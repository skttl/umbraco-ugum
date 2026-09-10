import { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor'

const entryPointManifest = {
  name: 'uGum Entry Point',
  type: 'backofficeEntryPoint',
  alias: 'uGum.EntryPoint',
  js: () => import('./entrypoint.js'),
}

const editorManifest: ManifestPropertyEditorUi = {
  type: 'propertyEditorUi',
  alias: 'uGum.PropertyEditorUi.GumletSync',
  name: 'Gumlet Sync',
  element: () => import('./property-editor-ui-gumlet-sync.element.js'),
  meta: {
    label: 'Gumlet Sync',
    icon: 'icon-video',
    group: 'media',
    propertyEditorSchemaAlias: 'uGum.Sync',

    settings: {
      properties: [
        {
          alias: "uploadPropertyAlias",
          label: "Upload Property Alias",
          description: "Set the alias of the upload property editor to sync with",
          propertyEditorUiAlias: "Umb.PropertyEditorUi.TextBox",
        },
      ],
    },
  },
};

const schemaManifest: ManifestPropertyEditorSchema = {
  type: 'propertyEditorSchema',
  name: 'Gumlet Sync',
  alias: 'uGum.Sync',
  meta: {
    defaultPropertyEditorUiAlias: 'uGum.PropertyEditorUi.GumletSync'
  },
};

export const manifests = [
  entryPointManifest,
  editorManifest,
  schemaManifest,
];
