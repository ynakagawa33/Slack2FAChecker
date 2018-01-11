using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Slack2FAChecker
{
	public class Function
	{
		private static string Region => Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");
		private static string SlackApiEndpoint => "https://slack.com/api/";
		private static string SlackUserListMethod => "users.list";

		public async Task<dynamic> FunctionHandler(dynamic input, ILambdaContext context)
		{
			context.Logger.LogLine("PrEmergency started!");
			context.Logger.LogLine(Region);
			var rawJson = JsonConvert.SerializeObject(input);
			context.Logger.LogLine(rawJson);
			var eventData = (EventData) JsonConvert.DeserializeObject<EventData>(rawJson);

			if (string.IsNullOrEmpty(eventData.SlackApiToken))
			{
				context.Logger.LogLine(
					$"Input json detected not contain {nameof(eventData.SlackApiToken)} key. return immediately.");
				throw new ArgumentException(nameof(eventData.SlackApiToken));
			}
			if (string.IsNullOrEmpty(eventData.SlackWebhookUrl))
			{
				context.Logger.LogLine(
					$"Input json detected not contain {nameof(eventData.SlackWebhookUrl)} key. return immediately.");
				throw new ArgumentException(nameof(eventData.SlackWebhookUrl));
			}

			var members = new List<Member>();
			using (var httpClient = new HttpClient())
			{
				var nextCursor = string.Empty;
				var content = new Dictionary<string, string>
				{
					{"token", eventData.SlackApiToken},
					{"limit", "200"}
				};
				var url = SlackApiEndpoint + SlackUserListMethod;
				while (true)
				{
					if (!string.IsNullOrEmpty(nextCursor))
						content.Add("cursor", nextCursor);

					var formUrlEncodedContent = new FormUrlEncodedContent(content);
					var response = await httpClient.PostAsync(url, formUrlEncodedContent);
					var slackUserListResponseJson = await response.Content.ReadAsStringAsync();
					context.Logger.LogLine(slackUserListResponseJson);
					var slackUserListResponse = JsonConvert.DeserializeObject<UserListResponse>(slackUserListResponseJson);
					members.AddRange(slackUserListResponse.Members);

					if (string.IsNullOrEmpty(slackUserListResponse.ResponseMetadata.NextCursor))
						break;
					nextCursor = slackUserListResponse.ResponseMetadata.NextCursor;
				}
			}

			var excludeUserNames = eventData.ExcludeUserNameCsv.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			var no2FaUserNames = members
				.Where(member => member.Has2Fa == false && member.Deleted == false)
				.Select(x => x.Name)
				.Except(excludeUserNames)
				.ToList();

			string postMessage;
			if (no2FaUserNames.Any())
			{
				postMessage = "2FA �������[�U" + Environment.NewLine + string.Join(",", no2FaUserNames);
				using (var httpClient = new HttpClient())
				{
					var postJson = JsonConvert.SerializeObject(new
					{
						text = postMessage
					});

					using (var content = new StringContent(postJson, Encoding.UTF8, "application/json"))
					{
						var requestContentText = await content.ReadAsStringAsync();
						context.Logger.LogLine(requestContentText);
						var response = await httpClient.PostAsync(eventData.SlackWebhookUrl, content);
						var responseContentText = await response.Content.ReadAsStringAsync();
						context.Logger.LogLine($"{response.StatusCode}:{responseContentText}");
					}
				}
			}
			else
			{
				postMessage = "2FA �������̃��[�U�͂��܂���I�Z�L���A�f�X�I";
				context.Logger.LogLine("2FA �������̃��[�U�����Ȃ����߁A Slack �ւ̃|�X�g�͍s���܂���B");
			}

			context.Logger.LogLine("PrEmergency finished!");

			return postMessage;
		}
	}
}