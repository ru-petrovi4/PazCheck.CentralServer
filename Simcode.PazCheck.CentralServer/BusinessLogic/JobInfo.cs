using Grpc.Core;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
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

		public JobInfo(string jobId, string jobTitle, CancellationToken cancellationToken)
        {
			JobId = jobId;
			JobTitle = jobTitle;			
			CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		}

		#endregion

		#region public functions

		public string JobId { get; }

		/// <summary>
		///     Заголовок задачи
		/// </summary>		
		public string JobTitle { get; }

		public CancellationTokenSource CancellationTokenSource { get; } = new();

		public async Task ReportAsync(double progressPercent, string? progressLabel, string? progressDetail, uint statusCode)
        {
			await _syncRoot.WaitAsync();
			try
			{
				_progressPercent = progressPercent;
				_progressLabel = progressLabel ?? @"";
				_progressDetail = progressDetail ?? @"";
				_statusCode = statusCode;

				if (statusCode != 0 || progressPercent > 99.9) // If failed or finished
				{
					_finishedTimeUtc = DateTime.UtcNow;
				}
			}
			finally
			{
				_syncRoot.Release();
			}			
        }

		public async Task<Job> CreateJobAsync()
        {
			await _syncRoot.WaitAsync();
			try
            {
				return new Job
				{
					Id = JobId,
					JobTitle = this.JobTitle,
					ProgressPercent = this._progressPercent,
					ProgressLabel = this._progressLabel,
					ProgressDetail = this._progressDetail,
					StatusCode = this._statusCode,
					FinishedTimeUtc = this._finishedTimeUtc,
				};
			}
            finally 
			{
				_syncRoot.Release();
			}
		}

		#endregion

		#region private fields
		
		private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

		/// <summary>
		///     Процент выполнения задачи 0 - 100
		/// </summary>		
		private double _progressPercent;

		/// <summary>
		///     Лейбл о статусе выполнения или сообщение об ошибке
		/// </summary>		
		private string _progressLabel = @"";

		/// <summary>
		///     Дептали о статусе исполнение или детали об ошибке
		/// </summary>		
		private string _progressDetail = @"";

		/// <summary>
		///     OK = 0, Cancelled = 1, Error >= 2.
		///     For details, see Grpc.Core.StatusCode        
		/// </summary>
		private uint _statusCode;

		/// <summary>
		///     Время окончания задачи, успешно или с ошибкой.
		/// </summary>		
		private DateTime? _finishedTimeUtc;

		#endregion
	}
}
