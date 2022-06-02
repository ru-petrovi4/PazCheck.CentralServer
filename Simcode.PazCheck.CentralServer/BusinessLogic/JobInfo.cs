using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
	/// <summary>
	///     Represents info about long-running operation
	/// </summary>
    public class JobInfo : IJobProgress
	{
		#region construction and destruction

		public JobInfo(JobsManager jobsManager, CancellationToken cancellationToken)
        {
            _jobsManager = jobsManager;
			CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		}

		#endregion

		#region public functions		

		public string JobId = @"";

		public double ProgressPercent;

		public string ProgressLabel = @"";

		public string ProgressDetail = @"";

		public bool Failed;

		/// <summary>
		///    Successful finished DateTimeUtc
		/// </summary>
		public DateTime? FinishedDateTimeUtc;

		public CancellationTokenSource CancellationTokenSource { get; } = new();

		public CancellationToken CancellationToken => CancellationTokenSource.Token;

		public Task ReportAsync(double progressPercent, string? progressLabel, string? progressDetail, bool failed)
        {
			ProgressPercent = progressPercent;
			ProgressLabel = progressLabel ?? @"";
			ProgressDetail = progressDetail ?? @"";
			Failed = failed;
			
			if (!Failed || progressPercent > 100.0 - Double.Epsilon * 100)
			{
				FinishedDateTimeUtc = DateTime.UtcNow;				
			}

			return Task.CompletedTask;
        }

		#endregion

		#region private fields

		private readonly JobsManager _jobsManager;

		#endregion
	}
}
