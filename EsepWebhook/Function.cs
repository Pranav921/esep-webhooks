
using Amazon.Lambda.Core;
using System.Text;
using Newtonsoft.Json;

// Assembly attribute to enable JSON serialization
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
        string issueUrl = (string)json.issue.html_url;

        string payload = JsonConvert.SerializeObject(
            new { text = $"Issue Created: {issueUrl}" }
        );

        using var client = new HttpClient();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            Environment.GetEnvironmentVariable("SLACK_URL")
        )
        {
            Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json"
            )
        };

        var response = client.Send(request);
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
