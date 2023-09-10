// Copyright (c) Microsoft. All rights reserved.

global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using Microsoft.AspNetCore.Mvc;

global using Azure;
global using Azure.AI.OpenAI;
global using Azure.Communication;
global using Azure.Communication.Chat;
global using Azure.Communication.CallAutomation;
global using Azure.Communication.Identity;
global using Azure.Messaging.EventGrid;
global using Azure.Search.Documents;
global using Azure.Search.Documents.Models;
global using Azure.Messaging;
global using Azure.Messaging.EventGrid.SystemEvents;

global using CustomerSupportServiceSample.Extensions;
global using CustomerSupportServiceSample.Helpers;
global using CustomerSupportServiceSample.Interfaces;
global using CustomerSupportServiceSample.Models;
global using CustomerSupportServiceSample.Services;