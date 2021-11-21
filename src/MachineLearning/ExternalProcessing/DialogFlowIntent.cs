namespace PigeonAPI.MachineLearning.ExternalProcessing;

using OpenAI_API;
using PigeonAPI.Exceptions;
using PigeonAPI.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Google.Cloud.Dialogflow.V2;

/// <summary>
/// Class to handle Dialogflow related functionality
/// </summary>
public static class DialogFlowIntent
{
    private static readonly Task<IntentsClient> client = Task.Run(async () => {
        var client = await IntentsClient.CreateAsync();

        return client;
    });
}
