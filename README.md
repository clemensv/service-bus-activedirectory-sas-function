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

