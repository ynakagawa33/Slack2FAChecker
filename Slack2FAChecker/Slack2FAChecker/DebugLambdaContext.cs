using System;
using Amazon.Lambda.Core;

namespace Slack2FAChecker
{
	/// <summary>
	///     Implementation for Local Debug
	/// </summary>
	public class DebugLambdaContext : ILambdaContext
	{
		public string AwsRequestId { get; }
		public IClientContext ClientContext { get; }
		public string FunctionName { get; } = typeof(DebugLambdaContext).Namespace;
		public string FunctionVersion { get; }
		public ICognitoIdentity Identity { get; }
		public string InvokedFunctionArn { get; }
		public ILambdaLogger Logger { get; } = new DebugLambdaLogger();
		public string LogGroupName { get; }
		public string LogStreamName { get; }
		public int MemoryLimitInMB { get; }
		public TimeSpan RemainingTime { get; }
	}
}