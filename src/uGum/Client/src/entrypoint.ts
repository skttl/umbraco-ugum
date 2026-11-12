import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { client } from "./api/client.gen";

export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {
  _host.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
    authContext?.configureClient(client);
  });
}
