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

			var excludeDisplayNames = eventData.ExcludeDisplayNameCsv.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			var no2FaMembers = members
				.Where(member => member.IsBot == false)
				.Where(member => member.IsAppUser == false)
				.Where(member => member.Deleted == false)
				.Where(member => member.Has2Fa == false)
				.Where(member => !excludeDisplayNames.Contains(member.Profile.DisplayName))
				.ToList();

			string postJson;
			if (no2FaMembers.Any())
			{
				var attachments = new List<dynamic>();
				foreach (var no2FaMember in no2FaMembers)
					attachments.Add(new
					{
						fields = new[]
						{
							new
							{
								title = "DisplayName",
								value = !string.IsNullOrEmpty(no2FaMember.Profile.DisplayName) ? no2FaMember.Profile.DisplayName : "未設定",
								@short = true
							},
							new
							{
								title = "Email",
								value = !string.IsNullOrEmpty(no2FaMember.Profile.Email) ? no2FaMember.Profile.Email : "未設定",
								@short = true
							}
						},
						mrkdwn_in = new[] {"fields"}
					});
				using (var httpClient = new HttpClient())
				{
					postJson = JsonConvert.SerializeObject(new
					{
						text = "2FA 無効ユーザ一覧",
						attachments
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
				postJson = JsonConvert.SerializeObject(new
				{
					text = "2FA が無効のユーザはいません！セキュアデス！"
				});
				context.Logger.LogLine("2FA が無効のユーザがいないため、 Slack へのポストは行いません。");
			}

			context.Logger.LogLine("PrEmergency finished!");

			return postJson;
		}
	}
}