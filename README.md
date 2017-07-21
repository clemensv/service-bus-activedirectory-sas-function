# Service Bus SAS Token Function for Azure Active Directory 

This project implements an Azure Function acting as a Security 
Token Service (STS) that will issue Azure Service Bus, Azure Relay, 
and Azure Event compatible Shared Access Signature (SAS) Tokens 
to an application that has been registered with Active Directory 
and is in possession of an application key.

For Service Bus applications that have so far relied on the Azure
Active Directory Access Control Service (ACS), this Function is the 
foundation for a migration path.

The GetToken function implementation itself has no hard dependency 
on Active Directory and can be adapted to other identity providers. 

# Application Deployment

The project includes an Azure Resource Manager template that will
deploy a Service Bus namespace, and configure an Azure Function app
alongside of it that acts as its SAS token STS. The ARM template will
automatically configure the Service Bus namespace with four SAS rules 
for the rights "Send", "Listen", "SendListen", and "Manage". The "Manage"
rule also carries send and listen rights. The rule names along with the 
generated keys are added into the configuration of the Functions app under
the "ServiceBusSend", "ServiceBusListen", "ServiceBusSendListen", 
and "ServiceBusManage" settings keys. The Service Bus namespace is put 
under the "ServiceBusNamespace" app setting key.

## Deploying the template

Deploying the template first requires creating a resource group and 
associating that group with an Azure region. With Azure Powershell, 
this may look like this:

```Powershell
New-AzureRmResourceGroup -Name clemensv102 -Location westeurope
```

Once the resource group has been created, you can deploy the template,
specifying the Service Bus namespace name to be created:

```Powershell
New-AzureRmResourceGroupDeployment -ResourceGroupName clemensv102 -TemplateFile azuredeploy.json -serviceBusNamespaceName clemensv102
```

## Deploying the Function 

Once the template deployment is complete, a new Function app has been 
created that is prefixed with the name of the Service Bus namespace. In 
the above example, the name will be "clemens102sts".

The Azure Function code is the publishing output of the Microsoft.ServiceBus.ActiveDirectorySasTokenFunction project 
and publlished into the Azure Functions app like any other precompiled C# function.

It's recommended to first follow the Active Directory steps discussed below before
deploying the code.

For code deployment guidance see the [Azure Functions documentation]().

#Configuring Azure Active Directory 

Because Azure Active Directory is not integrated with Azure Resource 
Management templates, the automated deployment needs to be completed
with a few manual steps in the portal, which also include setting up
a first service account.

## Enabling Active Directory for the Function app

After the Function app is set up and the code is deployed, the Function
needs to be enabled for Active Directory.

![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)
![img01](images\img01.png)