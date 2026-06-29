using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
	/// <summary>
	///     Represents info about long-running operation
	/// </summary>
    public class JobInfo : IJobProgress
	{
		#region construction and destruction

		public JobInfo(string jobId, string jobTitle, string user)
        {			
            JobId = jobId;
			JobTitle = jobTitle;
			User = user;			
		}

		#endregion

		#region public functions

		public string JobId { get; }

		/// <summary>
		///     Заголовок задачи
		/// </summary>		
		public string JobTitle { get; }

		/// <summary>
		///     Пользователь, запустивший задачу
		/// </summary>
		public string User { get; }

        /// <summary>
        ///     Процент выполнения задачи 0 - 100
        /// </summary>		
        public uint ProgressPercent { get; private set; }

        /// <summary>
        ///     Лейбл о статусе выполнения или сообщение об ошибке
        /// </summary>		
        public string ProgressLabel { get; private set; } = @"";

        /// <summary>
        ///     Детали о статусе исполнение или детали об ошибке
        /// </summary>		
        public string ProgressDetails { get; private set; } = @"";

        /// <summary>
		///     Cancelled - BadRequestCancelledByClient = 0x802C0000
        ///     See consts in <see cref="StatusCodes"/>.        
        /// </summary>
        public uint StatusCode { get; private set; }

        /// <summary>
        ///     CancellationTokenSource for job cancelling.
        /// </summary>
        public CancellationTokenSource Job_CancellationTokenSource { get; } = new();

        /// <summary>
        ///     ContinuationSemaphoreSlim for job continue from pause or cancel.
        /// </summary>
        public SemaphoreSlim Job_ContinuationSemaphoreSlim { get; } = new SemaphoreSlim(0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progressPercent"></param>
        /// <param name="progressLabel"></param>
        /// <param name="progressDetails"></param>
        /// <param name="statusCode">See consts in StatusCodes</param>
        /// <returns></returns>
        public async Task SetJobProgressAsync(uint? progressPercent, string? progressLabel, string? progressDetails, uint statusCode)
        {
			await _syncRoot.WaitAsync();
			try
			{
				if (progressPercent is not null)
					ProgressPercent = (uint)progressPercent;
                if (progressLabel is not null)
                    ProgressLabel = progressLabel;
                if (progressDetails is not null)
                    ProgressDetails = progressDetails;
                StatusCode = statusCode;

				if (!StatusCodes.IsGood(StatusCode)) // If failed or cancelled
				{
					if (ProgressPercent == 0)
						ProgressPercent = 5; // Что бы была видна красная линия.

                    _endTimeUtc = DateTime.UtcNow;                    
                }
                else if (ProgressPercent == 100) // If finished
                {
                    _endTimeUtc = DateTime.UtcNow;
                }
            }
			finally
			{
				_syncRoot.Release();
			}			
        }

        public async Task<IJobProgress> GetChildJobProgressAsync(uint minProgressPercent, uint maxProgressPercent, bool parentFailedIfFailed)
        {
            await SetJobProgressAsync(minProgressPercent, null, null, StatusCodes.Good);
            return new ChildJobProgress(this, 
				minProgressPercent, 
				maxProgressPercent,
                parentFailedIfFailed);
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
					User = this.User,
					ProgressPercent = this.ProgressPercent,
					ProgressLabel = this.ProgressLabel,
					ProgressDetail = this.ProgressDetails,
					JobStatusCode = this.StatusCode,
					BeginTimeUtc = this._beginTimeUtc,
					EndTimeUtc = this._endTimeUtc,
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
		///     Время начала задачи.
		/// </summary>		
		private DateTime _beginTimeUtc = DateTime.UtcNow;

		/// <summary>
		///     Время окончания задачи, успешно или с ошибкой.
		/// </summary>		
		private DateTime? _endTimeUtc;

		#endregion		
    }
}
