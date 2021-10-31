# dapr-partner-workshop

## Steps
The workshop is build around five steps.

0. Deployment of required Azure infrastructure, databases for step one and two, along with AKS for 3 and 4.
1. Building the monolithic Web API solution and splitting its databases.
2. Splitting solution into two projects, containeraizing them and adding DAPR runtime.
3. Create AKS manifest, setting up DAPR in Azure Kubernetes cluster and deploying solution to Cloud.
4. Adding DAPR pubsub component and RabbitMQ container. Changing solution code to work with a pubsub.
Analyzing issues and solving them with logging.

## Prerequisites

Good mood :).

1. Visual Studio or Visual Studio Code with .NET Framework 3.1.
2. Docker Desktop to run the containerized application locally.
https://www.docker.com/products/docker-desktop
3. DAPR CLI installed on a local machine.
https://docs.dapr.io/getting-started/install-dapr-cli/
4. Kompose tool for Kubernetes manifest generation (optional).
https://kompose.io/getting-started/ 
5. AZ CLI tools installation(for cloud deployment)
https://aka.ms/installazurecliwindows
6. Azure subscription, if you want to deploy applications to Kubernetes(AKS).
https://azure.microsoft.com/en-us/free/
7. Kubectl installation https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/#install-kubectl-binary-with-curl-on-windows
8. Good mood :)

## Step 0. Azure infrastructure
Script below should be run via Azure Portal bash console. 
You will receive database connection strings with setx command as output of this script.
Please add a correct name of your subscription to the first row of the script. 

```bash
subscriptionID=$(az account list --query "[?contains(name,'Microsoft')].[id]" -o tsv)
echo "Test subscription ID is = " $subscriptionID
az account set --subscription $subscriptionID
az account show

location=northeurope
postfix=$RANDOM

#----------------------------------------------------------------------------------
# Database infrastructure
#----------------------------------------------------------------------------------

export dbResourceGroup=ms-action-dapr-data$postfix
export dbServername=ms-action-dapr$postfix
export dbPoolname=dbpool
export dbAdminlogin=FancyUser3
export dbAdminpassword=Sup3rStr0ng52$postfix
export dbPaperName=paperorders
export dbDeliveryName=deliveries

az group create --name $dbResourceGroup --location $location

az sql server create --resource-group $dbResourceGroup --name $dbServername --location $location \
--admin-user $dbAdminlogin --admin-password $dbAdminpassword
	
az sql elastic-pool create --resource-group $dbResourceGroup --server $dbServername --name $dbPoolname \
--edition Standard --dtu 50 --zone-redundant false --db-dtu-max 50

az sql db create --resource-group $dbResourceGroup --server $dbServername --elastic-pool $dbPoolname \
--name $dbPaperName --catalog-collation SQL_Latin1_General_CP1_CI_AS
	
az sql db create --resource-group $dbResourceGroup --server $dbServername --elastic-pool $dbPoolname \
--name $dbDeliveryName --catalog-collation SQL_Latin1_General_CP1_CI_AS	

sqlClientType=ado.net

SqlPaperString=$(az sql db show-connection-string --name $dbPaperName --server $dbServername --client $sqlClientType --output tsv)
SqlPaperString=${SqlPaperString/Password=<password>;}
SqlPaperString=${SqlPaperString/<username>/$dbAdminlogin}

SqlDeliveryString=$(az sql db show-connection-string --name $dbDeliveryName --server $dbServername --client $sqlClientType --output tsv)
SqlDeliveryString=${SqlDeliveryString/Password=<password>;}
SqlDeliveryString=${SqlDeliveryString/<username>/$dbAdminlogin}

SqlPaperPassword=$dbAdminpassword

#----------------------------------------------------------------------------------
# AKS infrastructure
#----------------------------------------------------------------------------------

location=northeurope
groupName=ms-action-dapr-cluster$postfix
clusterName=msaction-cluster$postfix
registryName=msactionregistry$postfix
accountSku=Standard_LRS
accountName=msactionstorage$postfix
queueName=msactionqueue
queueResultsName=msactionqueueresults

az group create --name $groupName --location $location

az acr create --resource-group $groupName --name $registryName --sku Standard
az acr identity assign --identities [system] --name $registryName

az aks create --resource-group $groupName --name $clusterName --node-count 3 --generate-ssh-keys --network-plugin azure
az aks update --resource-group $groupName --name $clusterName --attach-acr $registryName

#----------------------------------------------------------------------------------
# Service bus queue and application insights
#----------------------------------------------------------------------------------

groupName=ms-action-dapr-extras$postfix
location=northeurope
az group create --name $groupName --location $location
namespaceName=msActionDapr$postfix
queueName=createdelivery

az servicebus namespace create --resource-group $groupName --name $namespaceName --location $location
az servicebus queue create --resource-group $groupName --name $queueName --namespace-name $namespaceName
serviceBusString=$(az servicebus namespace authorization-rule keys list --resource-group $groupName --namespace-name $namespaceName --name RootManageSharedAccessKey --query primaryConnectionString --output tsv)

insightsName=msactiondaprlogs$postfix
az monitor app-insights component create --resource-group $groupName --app $insightsName --location $location --kind web --application-type web --retention-time 120

instrumentationKey=$(az monitor app-insights component show --resource-group $groupName --app $insightsName --query  "instrumentationKey" --output tsv)

#----------------------------------------------------------------------------------
# Azure function app with storage account
#----------------------------------------------------------------------------------

accountSku=Standard_LRS
accountName=msactionstorage$postfix

az storage account create --name $accountName --location $location --kind StorageV2 \
--resource-group $groupName --sku $accountSku --access-tier Hot  --https-only true

accountKey=$(az storage account keys list --resource-group $groupName --account-name $accountName --query "[0].value" | tr -d '"')

accountConnString="DefaultEndpointsProtocol=https;AccountName=$accountName;AccountKey=$accountKey;EndpointSuffix=core.windows.net"

applicationName=msactiondaprfunc$postfix
echo "applicationName  = " $applicationName

az functionapp create --resource-group $groupName \
--name $applicationName --storage-account $accountName \
--consumption-plan-location $location --functions-version 3

az functionapp update --resource-group $groupName --name $applicationName --set dailyMemoryTimeQuota=400000
az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "MSDEPLOY_RENAME_LOCKED_FILES=1"
az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings ASPNETCORE_ENVIRONMENT=Production
az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "StorageConnectionString=$accountConnString"

keyvaultName=msActionDapr$postfix
principalName=vaultadmin
principalCertName=vaultadmincert

az keyvault create --resource-group $groupName --name $keyvaultName --location $location
az keyvault secret set --name SqlPaperPassword --vault-name $keyvaultName --value $SqlPaperPassword

az ad sp create-for-rbac --name $principalName --create-cert --cert $principalCertName --keyvault $keyvaultName --skip-assignment --years 3

# get appId from output of this step and use commented code below to grant access.

# az ad sp show --id 88511b82-8ced-4ba3-bd9b-0599f479e870
# get objectId from command output above and set it to command below 

# az keyvault set-policy --name $keyvaultName --object-id b3535a27-26f0-4c59-a50a-bd13886e4185 --secret-permissions get
#----------------------------------------------------------------------------------
# SQL connection strings
#----------------------------------------------------------------------------------

printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlPaperString:\nsetx SqlPaperString \"$SqlPaperString\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlDeliveryString:\nsetx SqlDeliveryString \"$SqlDeliveryString\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlPaperPassword:\nsetx SqlPaperPassword \"$SqlPaperPassword\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlDeliveryPassword:\nsetx SqlDeliveryPassword \"$SqlPaperPassword\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable AzureWebJobsStorage:\nsetx AzureWebJobsStorage \"$accountConnString\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable ServiceBusString:\nsetx ServiceBusString \"$serviceBusString\"\n\n"

echo "Update open-telemetry-collector-appinsights.yaml in Step 4 End => <INSTRUMENTATION-KEY> value with:  " $instrumentationKey
```

Important thing is to setup network connectivity between deployed kubernetes cluster and deployed database.

There are two steps to do it via Azure Portal.

Kubernetes connectivity
* Navigate into resource group MC_msaction-cluster_msaction-cluster_northeurope and open
* Open Virtual network there
* Open Service endpoints and click add
* Select Microsoft.SQL from dropdown and select aks-vnet in the next dropdown.
* Add additional integration with Microsoft.ServiceBus

SQL Server connectivity.
* Navigate to the resource group - ms-action-dapr-data
* Open Sql Server ms-action-dapr
* Click Show Firewall
* On top click add client IP address, so you can access sql server from your work machine
* Click  Add existing virtual network + Create new virtual network
* Add aks-vnet with a proper name(check name via AKS cluster group)
* Most important step - click Save in the portal UI

## Step 1. Monolithic Web API initial split.

We will split Entity Framework context into two parts that can use the same(or different databases).

Gradual split in code is a key to successful migration and can be done as a part of normal development process.

Start folder contains monolith solution 

End folder contains monolith with EF context split in two for the different schema in the same database.




## Step 2. Split in two projects, docker compose and DAPR initialization.
We adding containerization via Visual Studio tooling and manually adding DAPR sidecar configuration for each server.

Start folder contains solution with two projects.
Along with Dockefile generated by visual studio and updates for code.
Including the new reference for localhost container url - host.docker.internal and environment variable file. 
Don't forget to add env file with a secrets content along with changes to docker-compose.yaml in the root folder.

Solution will work with two containers, so there is a need to put the correct container port for Delivery service.

!! Be aware, if you have docker build exceptions in Visual studio with errors related to the File system, there is a need to configure docker desktop. Open Docker desktop => configuration => Resources => File sharing => Add your project folder or entire drive, C:\ for example. Dont forget to remove drive setting later on.

End folder contains solution with DAPR, service invocation via HTTP and docker compose files with sidecar. 

Lets start with right clicking on each solution and adding orchestration with Container orchestration via Docker compose. Visual studio will generate docker compose files for you. 

## Step 3. Application deployment to Azure Kubernetes service.
We will create AKS manifests for our services and add DAPR sections.
Deploy dapr to AKS cluster and add containers to the private repository.

Start folder contains solution with local env variables added to docker compose. At this point we will enable database communication with our AKS cluster and setup connection from local machine to private container registry and kubernetes cluster.

End folder contains solution with Kubernetes manifests ready for deployment, secrets included right into manifests to simplify flow.

You will need an Azure subscription ID.

Lets start with CMD.
```cmd
az login
az account set --subscription 95cd9078f8c
az account show
az acr login --name msactionregistry
az aks get-credentials --resource-group msaction-cluster --name msaction-cluster --overwrite-existing
kubectl config use-context msaction-cluster
kubectl get all
```

And initialize DAPR.
```cmd
dapr init -k 
```

and validate it with 
```cmd
dapr status -k 
```
Then we will need to build our solution in release mode and observe results with command. You can start docker desktop application for GUI container handling.

```cmd
docker images
```
Lets tag our newly built container with azure container registry name and version.
```cmd
docker tag tpaperorders:latest msactionregistry.azurecr.io/tpaperorders:v1
docker tag tpaperdelivery:latest msactionregistry.azurecr.io/tpaperdelivery:v1
```

Check results with
```cmd
docker images
```

And push images to container registry
```cmd
docker push msactionregistry.azurecr.io/tpaperorders:v1
docker push msactionregistry.azurecr.io/tpaperdelivery:v1
```

There is need to change version of container in YAML manifest files inside Step 3 End directory, and change this files each time you preparing a new version of container.
```yaml
    spec:
      containers:
        - name: tpaperorders
          image: msactionregistry.azurecr.io/tpaperorders:v1
          imagePullPolicy: Always
          ports:
            - containerPort: 80
              protocol: TCP          
```

Now we need to pray the "demo gods" for our deployment and run commands below
```cmd
kubectl apply -f tpaperorders-deploy.yaml
kubectl apply -f tpaperdelivery-deploy.yaml
```

And then check results with 

```cmd
kubectl get all
```

You can use set of commands below for quick container/publish re-deployments.
Just change version in kubernetes manifest and commands below.
```cmd
docker tag tpaperorders:latest msactionregistry.azurecr.io/tpaperorders:v2
docker images
docker push msactionregistry.azurecr.io/tpaperorders:v2
kubectl apply -f tpaperorders-deploy.yaml
kubectl get all

docker tag tpaperdelivery:latest msactionregistry.azurecr.io/tpaperdelivery:v2
docker images
docker push msactionregistry.azurecr.io/tpaperdelivery:v2
kubectl apply -f tpaperdelivery-deploy.yaml
kubectl get all
```

We cam observe our deployment with get all command and checking of external public endpoints(public load balancer endpoints).

```
20.67.14.15/api/order/create/1
20.67.15.202/api/delivery/get
```

In case of the problems we need to investigate logs via command prompt.

```
kubectl logs tpaperdelivery-8c4bdc475-j89kx daprd
kubectl logs tpaperdelivery-8c4bdc475-j89kx tpaperdelivery
```




## Step 4. Introduction to the DAPR pubsub.
We will deploy DAPR pubsub component to Azure. Make changes to our code and take a look into the pod logs to see whats happening.

Start folder contains all needed files for this step.

We need to deploy pubsub component and RabbitMQ broker with
```cmd
kubectl apply -f rabbitmq.yaml
kubectl apply -f pubsub-rabbitmq.yaml
```

Then we updating C# code and DAPR service manifest files to container v2 and building solution in Visual Studio.

```cmd
docker tag tpaperorders:latest msactionregistry.azurecr.io/tpaperorders:v2
docker tag tpaperdelivery:latest msactionregistry.azurecr.io/tpaperdelivery:v2

docker push msactionregistry.azurecr.io/tpaperorders:v2
docker push msactionregistry.azurecr.io/tpaperdelivery:v2
```

And then deployment via service manifests.
```cmd
kubectl apply -f rabbitmq.yaml
kubectl apply -f pubsub-rabbitmq.yaml
```

And testing results with slightly updated endpoints
```
20.67.14.15/api/order/create/1
20.67.15.202/api/deliveries/get
```

We will need following commands to get logs from AKS cluster.
You should get correct pod names from get all command and change log command accordingly.

```cmd
kubectl get all

kubectl logs tpaperdelivery-599b8cd4b7-8nxzz daprd
kubectl logs tpaperdelivery-599b8cd4b7-8nxzz tpaperdelivery
```
In the folder END we have additional file for Application insight integration.

Check out the file open-telemetry-collector-appinsights.yaml and replace the <INSTRUMENTATION-KEY> placeholder with your Application Insights Instrumentation Key.
Apply the configuration with 

```
kubectl apply -f open-telemetry-collector-appinsights.yaml
```
Open collector-config.yaml file and check its content
Apply the configuration with 
```	
kubectl apply -f collector-config.yaml
```
Update services manifestst with following code and update container version to the new version.
	
```
        dapr.io/log-level: debug
        dapr.io/config: "appconfig"
```
	
Rebuild solution in visual studio and deploy new container versions.

```
docker tag tpaperorders:latest msactionregistry.azurecr.io/tpaperorders:v4
docker tag tpaperdelivery:latest msactionregistry.azurecr.io/tpaperdelivery:v4

docker push msactionregistry.azurecr.io/tpaperorders:v4
docker push msactionregistry.azurecr.io/tpaperdelivery:v4

kubectl apply -f tpaperorders-deploy.yaml
kubectl apply -f tpaperdelivery-deploy.yaml
```

	
	
	
## Step 5. Secrets via Azure KeyVault and Azure functions component.

* We created an Azure Key Vault with our infrastructure beforehand. 
But steps below included just in case.

```bash
az keyvault create --resource-group $groupName --name $keyvaultName --location $location
```

* Create a service principal

Create a service principal with a new certificate and store the 3-year certificate inside [your keyvault]'s certificate vault.

> **Note** you can skip this step if you want to use an existing service principal for keyvault instead of creating new one

```bash
az ad sp create-for-rbac --name $principalName --create-cert --cert $principalCertName --keyvault $keyvaultName --skip-assignment --years 3

{
  "appId": "88511b82-8ced-4ba3-bd9b-0599f479e870",
  "displayName": "vaultadmin",
  "name": "88511b82-8ced-4ba3-bd9b-0599f479e870",
  "password": null,
  "tenant": "53e93ede-ec5b-4d7a-8376-48e080d23e88"
}
```

**Save the both the appId and tenant from the output which will be used in the next step**

* Get the Object Id for [your_service_principal_name]

```bash
az ad sp show --id 88511b82-8ced-4ba3-bd9b-0599f479e870

{
    ...
  "objectId": "b3535a27-26f0-4c59-a50a-bd13886e4185",
  "objectType": "ServicePrincipal",
    ...
}
```

* Grant the service principal the GET permission to your Azure Key Vault

```bash
az keyvault set-policy --name $keyvaultName --object-id b3535a27-26f0-4c59-a50a-bd13886e4185 --secret-permissions get
```

Now, your service principal has access to your keyvault,  you are ready to configure the secret store component to use secrets stored in your keyvault to access other components securely.

* Download PFX cert from your Azure Keyvault via Portal
  Go to your keyvault on Portal and download [certificate_name] pfx cert from certificate vault
  
* Create a kubernetes secret using the following command

- **C:\3\msactiondapr-vaultadmincert-20210923.pfx** is the path of PFX cert file you downloaded before
- **vaultsecret** is secret name in Kubernetes secret store

```bash
kubectl create secret generic vaultsecret2 --from-file=vaultsecret2="C:\3\msactiondapr-vaultadmincert-20210923.pfx"
```

2. Create azurekeyvault-deploy.yaml component file

The component yaml refers to the Kubernetes secretstore using `auth` property and  `secretKeyRef` refers to the certificate stored in Kubernetes secret store.

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: azurekeyvault
spec:
  type: secretstores.azure.keyvault
  metadata:
  - name: vaultName
    value: msActionDapr
  - name: spnTenantId
    value: "53e93ede-ec5b-4d7a-8376-48e080d23e88"
  - name: spnClientId
    value: "88511b82-8ced-4ba3-bd9b-0599f479e870"
  - name: spnCertificate
    key: msactiondapr-vaultadmincert-20210923.pfx
```

3. Apply azurekeyvault.yaml component

```bash
kubectl apply -f azurekeyvault-deploy.yaml
```

4. We already stored SQL password in KeyVault, but for clarification.

```bash
az keyvault secret set --name SqlPaperPassword --vault-name $keyvaultName --value $SqlPaperPassword
```

Now we need to update manifest of delivery service

```yaml
    spec:
      containers:
        - name: tpaperdelivery
          image: msactionregistry.azurecr.io/tpaperdelivery:v1
          imagePullPolicy: Always
          ports:
            - containerPort: 80
              protocol: TCP
          env:
            - name: ASPNETCORE_URLS
              value: http://+:80
            - name: SqlDeliveryString
              value: [secret]
            - name: SqlPaperPassword
              secretKeyRef:
                name: SqlPaperPassword
                key: SqlPaperPassword
      auth:
          secretStore: azurekeyvault 
```

* Apply service component

  ```bash
  kubectl apply -f tpaperorders-deploy.yaml
  ```
Make sure that `secretstores.azure.keyvault` is loaded successfully in `daprd` sidecar log
  
  
## Useful commands and notes.  

You might need to delete all deployments
```cmd
kubectl get deployments

kubectl delete deployments tpaperdelivery
kubectl delete deployments tpaperorders

kubectl delete svc tpaperorders
kubectl delete svc tpaperdelivery
```

If you want to purge containers from Azure container registry
```cmd
az acr repository delete --name msactionregistry --repository tpaperdelivery
az acr repository delete --name msactionregistry --repository tpaperorders
```

To cleanup local docker images via cmd. It is recommended to do after each step.

```cmd
for /F %i in ('docker images -a -q') do docker rmi -f %i
```
