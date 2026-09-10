import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { GumletValue } from './types/gumletvalue';
import { AssetStatus, Ugum } from './api';

@customElement('ugum-property-editor-ui-gumlet-sync')
export class UGumPropertyEditorUIGumletSyncElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  @property() public set value(newValue: GumletValue | undefined | null) { this._value = newValue; this.#fetchStatus(); }
  public get value(): GumletValue | undefined | null { return this._value; }
  @state() private _value: GumletValue | undefined | null = null;
  @state() private _status: AssetStatus | null = null;
  @state() private _statusLoading = false;

  #fetchStatus = async () => {
    if (!this.value?.GumletAssetId) { this._status = null; return; }
    this._statusLoading = true;
    const { data } = await Ugum.getStatus({ query: { assetId: this.value.GumletAssetId } });
    this._statusLoading = false;
    this._status = data as unknown as AssetStatus;
    if (this._status === AssetStatus.UNKNOWN || this._status === AssetStatus.PREPARING) setTimeout(() => this.#fetchStatus(), 5000);
  }

  #assetStatusColor = (status: AssetStatus | null) => {
    switch (status) {
      case AssetStatus.PREPARING: return 'warning';
      case AssetStatus.ERRORED: return 'danger';
      case AssetStatus.READY: return 'positive';
      default: return 'default';
    }
  };

  #assetStatusLabel = (status: AssetStatus | null) => {
    switch (status) {
      case AssetStatus.PREPARING: return 'Preparing';
      case AssetStatus.ERRORED: return 'Errored';
      case AssetStatus.READY: return 'Ready';
      case AssetStatus.NOT_FOUND: return 'Not Found';
      default: return 'Unknown';
    }
  };

  override render() {
    return this.value
      ? html`<uui-ref-node name=${this.value.GumletAssetId ?? 'No asset id'} detail=${this.value.PlaybackUrl ?? 'No playback URL'} readonly>
          <uui-icon slot="icon" name="icon-video"></uui-icon>
          ${this._statusLoading ? html`<uui-loader size="s" slot="tag"></uui-loader>` : html`<uui-tag size="s" slot="tag" color=${this.#assetStatusColor(this._status)} @click=${this.#fetchStatus}>${this.#assetStatusLabel(this._status)}</uui-tag>`}
        </uui-ref-node>`
      : html`<uui-tag look="placeholder">No video uploaded to Gumlet</uui-tag>`;
  }
}

export default UGumPropertyEditorUIGumletSyncElement;
declare global { interface HTMLElementTagNameMap { 'ugum-property-editor-ui-gumlet-sync': UGumPropertyEditorUIGumletSyncElement; } }
