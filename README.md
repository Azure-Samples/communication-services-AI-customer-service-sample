# Customer service sample application with Azure Communication Services and Azure OpenAI

In this code sample, you will learn how to combine Azure Communication Services with Azure OpenAI Service and Azure AI Services (previously known as Cognitive Services) to create an intelligent customer service solution. You will see how to create a natural language communication experience that can answer questions, initiate calls, and provide summaries using your company’s knowledge base and customer conversation data. You will use the Retrieval Augmented Generation pattern, which leverages Azure Cognitive Search to retrieve relevant information from your data sources and feed it to the GPT-35-turbo model, one of the large language models available in Azure OpenAI Service.

The code sample includes a web app that simulates a scenario where a customer interacts with a chatbot, a voice bot and an escalation path to a human technician from a fictitious company called Contoso Energy, which specializes in green energy and solar power. You will be able to run the app end to end by following the deployment instructions. We are excited for you to try it out and customize it to fit your own scenarios and data.

![Architecture diagram](docs/architecture-diagram.png)

## Features
- A chat bot that can converse with customer in natural language using Contoso Energy’s knowledge base.
- A retrieval augmented generation (RAG) pattern that leverages Azure Cognitive Search to retrieve relevant information from data sources and feed it to Azure Open AI Service.
- A sample web app for the customer to interact with the chat bot.
- An escalation from chat conversation to a PSTN call initiated by the bot on customer’s request.
- A job router that assigns the job to the most suitable technician based on skills, availability, and location.
- Azure Communication Call Automation to convert voice to text and vice versa.
- An AI copilot that generates summaries, emails, and answers using the Azure OpenAI Service and Azure AI Service.

![AIFeatures](docs/AI_Enabled_Customer_Service.png)

## Cost Estimation
Pricing varies per region and usage, so it isn't possible to predict exact costs for your usage. However, you can try the Azure pricing calculator for the resources below.

- Azure OpenAI: Standard tier, ChatGPT and Ada models. Pricing per 1K tokens used, and at least 1K tokens are used per question. [Pricing](https://aka.ms/Mech-OpenAIpricing)
- Azure Cognitive Search: Standard tier, 1 replica, free level of semantic search. Pricing per hour. [Pricing](https://aka.ms/Mech-CogSearchpricing)
- Form Recognizer: SO (Standard) tier using pre-built layout. Pricing per document page, sample documents have 261 pages total. [Pricing](https://aka.ms/Mech-formpricing)
- Azure Blob Storage: Standard tier with ZRS (Zone-redundant storage). Pricing per storage and read operations. [Pricing](https://aka.ms/mech-blobpricing)
- Azure Communication Services : [Pricing](https://aka.ms/Mech-acspricing)
    - Email - Pay as you go tier
    - Calling, Recording and Video
    - Phone Number
    - Azure Communication Services Job Router: $0,01 per job created. First 100 jobs are free.

To reduce costs, you can switch to free SKUs for Azure Cognitive Search. There are some limits to consider; for example, you can have up to 1 free Cognitive Search resource per subscription, and the free Form Recognizer resource only analyzes the first 2 pages of each document. You can also reduce costs associated with the Form Recognizer by reducing the number of documents in the data folder, or by removing the post provision hook in azure.yaml that runs the prepdocs.py script. 

Azure Communication Services offer free [trial phone numbers](https://aka.ms/Mech-trialnumbers) for calling. The trial period is for 30 days. Note that the trial numbers does not support SMS.

## Pre-requisites
> **Note**<br> 
>  If you need Azure credits, you can visit [here](https://aka.ms/Mech-CogsSearch1) to get free credits to get you started. 
- An Azure account with an active subscription. For details, see [Create an account for free](https://aka.ms/Mech-Azureaccount) 
- For local run: Install Azure Dev Tunnels CLI by running the code below. For details, see [Create and host dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) 
    ```
    winget install Microsoft.Devtunnel
    ```
- [.NET 7](https://dotnet.microsoft.com/download)
- Powershell 7+ (pwsh) - For Windows users only. To install, navigate to "Installing the MSI package" section of [this guide](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3).
   * Important: Ensure you can run pwsh.exe from a PowerShell command. If this fails, you will likely need to upgrade PowerShell.
- Azure CLI. To install, navigate to "Installation" section of [this guide](https://learn.microsoft.com/en-us/cli/azure/).
- In order to deploy and run this example, you'll need an **Azure subscription with access enabled for the Azure OpenAI service**. Request access [here](https://aka.ms/Mech-OpenAI). 
    * Important: Please note that although the survey says it can take up to 10 days, it usually takes no more than a few hours. Please do not proceed further before receiving the access approval email.


## Running the sample

From here, you have two options to implement ACS AI customer service sample. 
1. Deploying to Azure Cloud  
2. Deploying to Local Environment. 
* Important: If you want to try both methods, you can not duplicate deployments under the same subscription. You can either 1. use two different subscriptions for each method OR 2. delete the deployment completely after following one method and re-deploy to follow the second method. If you have trouble re-deploying, go to FAQ section.

## Option 1: Deploying to Azure Cloud 

> **Note**<br>
> This application uses the `gpt-35-turbo` model. When choosing which region to deploy to, make sure they're available in that region (i.e. EastUS). For more information, see the [Azure OpenAI Service documentation](https://aka.ms/Mech-OpenAIdocs). 

###  Deploy the sample to Azure subscription

1. Clone the project to a new folder.
2. Open a new powershell terminal and run `.\start.ps1` for Windows or `start.sh` (for Linux and Mac) 
    * Important: The directory to run this code should be the downloaded repository folder
3. Enter the name of the subscription that received AI access approval, service location and a new descriptive environment name (this will be the name of the new resource group).
    * Important: You can put locations like "eastus," "eastus2"
    - This command will:
        - Create the required Azure resources necessary to run the sample under the new resource group. 
        - Automatically include any of the PDF files in `data/` folder to Azure Search index as the knowledge base that is referred by the bot.
        - Build and deploy backend and frontend webapps necessary for hosting the sample webapp.
        - Register the backend application to receive notifications from Azure Communication Service events.
        - After the application is deployed, you will see a URL for frontend in the console output. Click that URL to interact with the application in your browser.

        Sample output:
        ```
        You can view the resources created under the resource group <your rg name> in Azure Portal: https:/portal.azure.com/#@/...

        Backend endpoint: https://app-app... 
        Frontend endpoint: https://wapp-app-web-...
        ```
> **Note**<br >
> It may take several minutes for the application to get fully deployed. Go grab a cup of coffee or listen to your favourite song. If the deployment fails, delete the deployment completely and redeploy. If the deployment fails, go to FAQ section.

###  Azure Portal Resource Setup
4. After deployment is completed, go to [Azure portal](https://portal.azure.com) and go to the resource group you created.
5. Selecte Communication Services Resource and follow [Connect Azure AI service to Azure communication service resource](https://aka.ms/Mech-connectACSWithAI)
6. Add a Calling and SMS enabled telephone number to your Communication resource. [Get a phone number](https://aka.ms/Mech-getPhone).
7. [Set up the email service](https://aka.ms/Mech-EmailService) to the resource group you created. 
8. [Create a managed Azure email domain](https://aka.ms/Mech-emaildomain) that will be used for sending emails. 
9. Update the backend App Service application settings
   - Open the resource that starts with "app-..." and navigate to the "Confirguration" blade.
   - Update values for `AcsPhoneNumber` and `EmailSender` with the phone number and sender email address obtained in previous steps.
   - Update the value for `EmailRecipient` with your personal email address where you would like to receive emails sent out by the sample applications.
   - Remember to save settings.
  
## Option 2: Deploying to Local environment  
> **Note**<br >
    > [Azure DevTunnels](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/overview) is an Azure service that enables you to share local web services hosted on the internet. Use the commands below to connect your local development environment to the public internet. This creates a tunnel with a persistent endpoint URL and which allows anonymous access. We will then use this endpoint to notify your application of chat and job router events from the Azure Communication Service.

###  DevTunnel URI Setup
1. Setup and host your Azure DevTunnel

    ```bash
    devtunnel create --allow-anonymous
    devtunnel port create -p 7108 --protocol https
    devtunnel host
    ```
    Make a note of the devtunnel URI. You will need it at later steps.

2. Run the `start.ps1` or `start.sh` to deploy the repository and create all resources to local environment.
    * Important: You will still see the deployment on your Azure portal.
        > **Note**<br >
        > It may take several minutes for the application to get fully deployed. Go grab a cup of coffee or listen to your favourite song.
3. Navigate to app\backend\appsettings.json and change the "HostUrl" variable with the devtunnel URI from the previous step. 
    * Important: If other variables are empty, run the code below
        ```bash
        ./scripts/update-local-appsettings.ps1
        ```

###  Running the backend application locally
4. Ensure your Azure Dev tunnel URI is active and points to the correct port of your localhost application.
5. Open a new terminal and run `dotnet run` to build and run the sample application.
6. Go to Communication Service Resource and follow [this direction](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/sms/handle-sms-events) to create EventGrid
7. For step 5 of the instruction above, choose 'ChatMessageReceivedInThread', 'RouterWorkerOfferIssued', 'RouterWorkerOfferAccepted,' and 'CallEnded for Event Types.'
8. For step 8 of the instruction above, enter your devtunnel URL and add “/api/events” at the end. Read more about Azure Event Grid webhooks [here](https://learn.microsoft.com/en-us/azure/event-grid/event-schema-communication-services).
9. Navigate to https://locahost:7108/swagger to familiarize yourself with available API routes on the backend application.
    * Important: You can go to your devtunnel URL + "swagger" to check if you land on the same page. You should land on the same page.

### Running the frontend application locally
10. Open a new terminal (third terminal) and navigate to `app/frontend`
11. Install dependencies

    ```bash
    npm install
    ```
12. Start the frontend app
    ```bash
    npm run start
    ```
    This will open a client server on port 3000 that serves the website files. By default it will connect to the localhost backend server running on port 7108

### Application Flow
13. Click on "Customer Service" button to start conversing with the bot via chat. If you don't see the button, go to FAQ section.

    ![customer_start_view](docs/customer_start_view.png)

- Voip Call - The application will send a voip call join link to the phone number you share in the chat with the bot. However, if you do not have a verified Azure Communication Services phone number, your Telco may block the SMS as it includes a url. As a workaround, browse <url>/?callerType=Customer to get the join link. Replace url with the frontend url or the localhost url depending on if you are running locally or the deployed application. 

- Agent View - The agent view includes chat history with the customer and AI assistant panel that can generate summary, action items and more. It gets active only after an agent gets involved in the flow. Browse <url>/?callerType=Agent to launch the agent view. 

    ![agent_view](docs/agent_view.png)

## FAQ
- How can I delete failed deployment?
    1. Delete the resource group from the resource group page.
    2. Go to subsciprtion that you created resource group for
    3. Navigate to "Deployments" panel on the left
    4. Under "Deployment details," select all deployments you attempted and click delete
    5. You completely deleted the deployments. You can re-deploy now.
- I don't see customer support button. What do I do?
    1. Go to app\frontend\src\appsettings
    2. Replace "baseUrl" with "https://localhost:7108"
    3. Refresh the "http://localhost:3000/" and you will see the button on bottom right.

## Resources
- [Azure Communication Services Blog](https://aka.ms/Mech-TechBlog) on AI infused customer service usecase
- [Microsoft Mechanics Youtube video showcasing this sample app](https://aka.ms/Mech-ytvideo)
- [Azure Communication Services](https://aka.ms/Mech-acsdocs)
- [Azure Cognitive Search](https://aka.ms/Mech-CogSearch)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/cognitive-services/openai/overview)

> **Note**<br >
>The [Voice recognition with speech-to-text](https://aka.ms/Mech-SpeechToText) feature used in the demo is in public preview. This preview version is provided without a service-level agreement, and we don't recommend it for production workloads. Certain features might not be supported or might have constrained capabilities. For more information, see Supplemental Terms of Use for Microsoft Azure Previews.

>The PDF documents used in this demo contain information generated using a language model (Azure OpenAI Service). The information contained in these documents is only for demonstration purposes and does not reflect the opinions or beliefs of Microsoft. Microsoft makes no representations or warranties of any kind, express or implied, about the completeness, accuracy, reliability, suitability, or availability with respect to the information contained in this document. All rights reserved to Microsoft.
