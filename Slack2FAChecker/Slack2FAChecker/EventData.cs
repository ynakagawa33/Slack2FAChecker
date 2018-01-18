namespace Slack2FAChecker
{
	public class EventData
	{
		public bool DryRun { get; set; }

		public string SlackWebhookUrl { get; set; }
		public string SlackApiToken { get; set; }

		public string ExcludeDisplayNameCsv { get; set; }
	}
}