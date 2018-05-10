namespace Slack2FAChecker
{
	public class Program
	{
		/// <summary>
		///     Entry point for local debug
		/// </summary>
		public static void Main(string[] args)
		{
			// use custom LambdaContext to access local context logger
			new Function().FunctionHandler(new EventData
				{
					SlackApiToken = "",
					ExcludeDisplayNameCsv = "",
					SlackWebhookUrl = ""
			}, new DebugLambdaContext())
				.Wait();
		}
	}
}