# Customer service sample application with Azure Communication Services and Azure OpenAI

In this code sample, you will learn how to combine Azure Communication Services with Azure OpenAI Service and Azure AI Services (previously known as Cognitive Services) to create an intelligent customer service solution. You will see how to create a natural language communication experience that can answer questions, initiate calls, and provide summaries using your company’s knowledge base and customer conversation data. You will use the Retrieval Augmented Generation pattern, which leverages Azure Cognitive Search to retrieve relevant information from your data sources and feed it to the GPT-35-turbo model, one of the large language models available in Azure OpenAI Service.

The code sample includes a sample web app that simulates a scenario where a customer interacts with a chatbot, a voice bot and an escalation path to human technician from a fictitious company called Contoso Energy, which specializes in green energy and solar power. You will be able to run the app end to end and customize it to fit your own scenario and data.

![Architecture diagram](docs/architecture-diagram.png)

## Features
- A chat bot that can converse with customer in natural language using Contoso Energy’s knowledge base.
- A retrieval augmented generation (RAG) pattern that leverages Azure Cognitive Search to retrieve relevant information from data sources and feed it to Azure Open AI Service.
- A sample web app for the customer to interact with the chat bot.
- An escalation from chat conversation to a PSTN call initiated by the bot on customer’s request.
- A job router that assigns the job to the most suitable technician based on skills, availability, and location.
- Azure Communication Call Automation to convert voice to text and vice versa.
- An AI copilot that generates summaries, emails, and answers using the Azure OpenAI Service and Azure AI Service.

## Running the sample

### Pre-requisites
- An Azure account with an active subscription. For details, see [Create an account for free](https://azure.microsoft.com/free/)  

- For local run: Install Azure Dev Tunnels CLI. For details, see [Create and host dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) 

- .NET 7 https://dotnet.microsoft.com/download
- Powershell 7+ (pwsh) - For Windows users only.
   * Important: Ensure you can run pwsh.exe from a PowerShell command. If this fails, you will likely need to upgrade PowerShell.
- Azure CLI


> **Note**<br> 
> In order to deploy and run this example, you'll need an **Azure subscription with access enabled for the Azure OpenAI service**. You can request access [here](https://aka.ms/oaiapply). You can also visit [here](https://azure.microsoft.com/free/cognitive-search/) to get some free Azure credits to get you started. 

## Deploying to Azure Cloud 

### Starting from scratch
Execute the following steps, if you don't have any pre-existing Azure services and want to start from a fresh deployment.

> **Note**<br>
> This application uses the `gpt-35-turbo` model. When choosing which region to deploy to, make sure they're available in that region (i.e. EastUS). For more information, see the [Azure OpenAI Service documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/concepts/models#gpt-3-models-1). 

1. Clone the project to a new folder
2. Open new powershell terminal and run `.\start.ps1` for Windows or `start.sh` (for Linux and Mac)
    <br />Enter the desired subscription, location and a short descriptive environment name.
3. This command will:
    1. Create the required Azure resources necessary to run the sample 
    2. Automatically include any of the PDF files in `data/` folder to Azure Search index as knowledgebase
    3. Build and deploy backend and frontend webapps necessary for hosting the sample
    4. Register the backend application to receive notifications from Azure Communication Service events
4. After the application has been successfully deployed you will see an URL for frontend printed to the console. Click that URL to interact with the application in your browser.

Sample output:
```
You can view the resources created under the resource group <your rg name> in Azure Portal: https:/portal.azure.com/#@/...

Backend endpoint: https://app-app... 
Frontend endpoint: https://wapp-app-web-...
```

> **Note**<br >
> It may take a few minutes for the application to be fully deployed.

### Additional steps
Before using the email and phone calling capabilities of the sample, you'll need to configure the above resources with following additional updates:

1. Add a Calling and SMS enabled telephone number to your Communication resource. [Get a phone number](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/telephony/get-phone-number?tabs=windows&pivots=platform-azp).

2. Set up the email service as per the document [TODO]. Make note of the DoNotReply address generated for you.

3. Update the backend App Service application settings
    1. Open the web app resource created for backend application and navigate to the Environment variables blade.
    2. Update values for `AcsPhoneNumber` and `EmailSender` with the phone number and sender email address obtained in previous steps.
    3. Update the value for `EmailRecipient` with your email address where you would like to receive emails sent out by the sample applications.
    4. Remember to save settings.

  
## Setup Instructions – Local environment  

#### 1. Setup and host your Azure DevTunnel
[Azure DevTunnels](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/overview) is an Azure service that enables you to share local web services hosted on the internet. Use the commands below to connect your local development environment to the public internet. This creates a tunnel with a persistent endpoint URL and which allows anonymous access. We will then use this endpoint to notify your application of chat and job router events from the Azure Communication Service.
```bash
devtunnel create --allow-anonymous
devtunnel port create -p 7108 --protocol https
devtunnel host
```
Make a note of the devtunnel URI. You will need it at later steps.

#### 2. Run the `start.ps1` or `start.sh` to provision your resources, if not done before.

#### 3. Update dev tunnel uri in `backend\app\appsettings.json` 

##  Running the backend application locally
1. Ensure your Azure Dev tunnel URI is active and points to the correct port of your localhost application

2. Run `dotnet run` to build and run the sample application

3. Register an EventGrid Webhook for the ChatMessageReceivedInThread event that points to your DevTunnel URI and route “/api/chat/incoming/events”. Read more about Azure Event Grid webhooks [here](https://learn.microsoft.com/en-us/azure/event-grid/event-schema-communication-services).

4. Navigate to https://locahost:7108/swagger to familiarize yourself with available API routes on the backend application


## Running the frontend application locally
1. Navigate to `app/frontend`
2. Install dependencies

    ```bash
    npm install
    ```

3. Start the frontend app

    ```bash
    npm run start
    ```

    This will open a client server on port 3000 that serves the website files. By default it will connect to the localhost backend server running on port 7108