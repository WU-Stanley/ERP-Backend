# wu-erp

# Wigww University Identity and Acccess Management system
# ERP-Backend
# ERP-Backend
# ERP-Backend
# ICT Microsoft account onboarding

When a recruitment offer is accepted or HR marks an application as hired, the ERP creates an ICT onboarding work item. Users in the `ITAdmin`, Systems Administrator, or Web Asset Manager roles receive the onboarding permissions and notification.

Microsoft account provisioning uses the existing `MicrosoftIdentity` tenant, client ID, and client secret. The Entra application must have the Microsoft Graph application permission needed to create users, with tenant admin consent.

The address policy can be configured with environment variables. Defaults are shown below:

```text
IctOnboarding__MicrosoftAccount__Domain=wigweuniversity.edu.ng
IctOnboarding__MicrosoftAccount__EmailFormat={first}.{last}
IctOnboarding__MicrosoftAccount__MaximumUniqueAttempts=100
```

Supported format tokens are `{first}`, `{last}`, `{firstinitial}`, and `{lastinitial}`. If the generated address already belongs to any Microsoft, ERP user, employee, or previously provisioned hire, the ERP appends a number beginning with `2`.
