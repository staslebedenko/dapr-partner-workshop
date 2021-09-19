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
https://docs.dapr.io/getting-started/install-dapr-cli/docker
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

export dbResourceGroup=net-fwdays-dapr-data$postfix
export dbServername=net-fwdays-dapr$postfix
export dbPoolname=fwdays
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
groupName=netfwdays-cluster$postfix
clusterName=netfwdays-cluster$postfix
registryName=netfwdaysregistry$postfix
accountSku=Standard_LRS
accountName=netfwdaysstorage$postfix
queueName=netfwdaysqueue
queueResultsName=netfwdaysqueueresults

az group create --name $groupName --location $location

az storage account create --name $accountName --location $location --kind StorageV2 \
--resource-group $groupName --sku $accountSku --access-tier Hot  --https-only true

accountKey=$(az storage account keys list --resource-group $groupName --account-name $accountName --query "[0].value" | tr -d '"')

accountConnString="DefaultEndpointsProtocol=https;AccountName=$accountName;AccountKey=$accountKey;EndpointSuffix=core.windows.net"

az storage queue create --name $queueName --account-key $accountKey \
--account-name $accountName --connection-string $accountConnString

az storage queue create --name $queueResultsName --account-key $accountKey \
--account-name $accountName --connection-string $accountConnString

az acr create --resource-group $groupName --name $registryName --sku Standard
az acr identity assign --identities [system] --name $registryName

az aks create --resource-group $groupName --name $clusterName --node-count 3 --generate-ssh-keys --network-plugin azure
az aks update --resource-group $groupName --name $clusterName --attach-acr $registryName

echo "Update local.settings.json Values=>AzureWebJobsStorage value with:  " $accountConnString

#----------------------------------------------------------------------------------
# SQL connection strings
#----------------------------------------------------------------------------------

printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlPaperString:\nsetx SqlPaperString \"$SqlPaperString\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlDeliveryString:\nsetx SqlDeliveryString \"$SqlDeliveryString\"\n\n"
printf "\n\nRun string below in local cmd prompt to assign secret to environment variable SqlPaperPassword:\nsetx SqlPaperPassword \"$SqlPaperPassword\"\n\n"

```

## Step 1. Monolithic Web API initial split.

We will split Entity Framework context into two parts that can use the same(or different databases).

Start folder contains monolith solution.

End folder contains monolith with EF context split in two.

## Step 2. Split in two projects, docker compose and DAPR initialization.
We adding containerization via Visual Studio tooling and manually adding DAPR sidecar configuration for each server.

Start folder contains solution with two projects.

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
az acr login --name netfwdaysregistry
az aks get-credentials --resource-group netfwdays-cluster --name netfwdays-cluster --overwrite-existing
kubectl config use-context netfwdays-cluster
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
docker tag tpaperorders:latest netfwdaysregistry.azurecr.io/tpaperorders:v3
docker tag tpaperdelivery:latest netfwdaysregistry.azurecr.io/tpaperdelivery:v1
```

Check results with
```cmd
docker images
```

And push images to container registry
```cmd
docker push netfwdaysregistry.azurecr.io/tpaperorders:v1
docker push netfwdaysregistry.azurecr.io/tpaperdelivery:v1
```

There is need to change version of container in YAML manifest files inside Step 3 End directory, and change this files each time you preparing a new version of container.
```yaml
    spec:
      containers:
        - name: tpaperorders
          image: netfwdaysregistry.azurecr.io/tpaperorders:v1
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
docker tag tpaperorders:latest netfwdaysregistry.azurecr.io/tpaperorders:v6
docker images
docker push netfwdaysregistry.azurecr.io/tpaperorders:v6
kubectl apply -f tpaperorders-deploy.yaml
kubectl get all

docker tag tpaperdelivery:latest netfwdaysregistry.azurecr.io/tpaperdelivery:v14
docker images
docker push netfwdaysregistry.azurecr.io/tpaperdelivery:v14
kubectl apply -f tpaperdelivery-deploy.yaml
kubectl get all
```


## Step 4. Introduction to the DAPR pubsub.
We will deploy DAPR pubsub component to Azure. Make changes to our code and take a look into the pod logs to see whats happening.

Start folder contains all needed files for this step.

We need to deploy pubsub component and RabbitMQ broker with
```cmd
kubectl apply -f rabbitmq.yaml
kubectl apply -f pubsub-rabbitmq.yaml
```

We will need following commands to get logs from AKS cluster.
You should get correct pod names from get all command and change log command accordingly.

```cmd
kubectl get all

kubectl logs tpaperdelivery-599b8cd4b7-8nxzz daprd
kubectl logs tpaperdelivery-599b8cd4b7-8nxzz tpaperdelivery
```


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
az acr repository delete --name netfwdaysregistry --repository tpaperdelivery
az acr repository delete --name netfwdaysregistry --repository tpaperorders
```

