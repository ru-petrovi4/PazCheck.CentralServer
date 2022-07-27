using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using Simcode.PazCheck.CentralServer.Common;
using Ssz.Utils.Addons;
using Ssz.DataAccessGrpc.Client;
using System.Text;

namespace Simcode.PazCheck.CentralServer.BusinessLogic.Safety
{
    public class SafetyBackgroundService : BackgroundService
    {
        #region construction and destruction

        public SafetyBackgroundService(IServiceProvider serviceProvider, AddonsManager addonsManager, ILogger<SafetyBackgroundService> logger, IConfiguration configuration, LocalDataAccessProvider localDataAccessProvider)
        {
            _serviceProvider = serviceProvider;
            _addonsManager = addonsManager;
            _serviceScope = serviceProvider.CreateScope();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<PazCheckDbContext>();
            _logger = logger;
            _configuration = configuration;                       
            _localDataAccessProvider = localDataAccessProvider;            
        }

        #endregion

        #region public functions

        public ThreadSafeDispatcher ThreadSafeDispatcher { get; } = new();        

        public CsvDb CsvDb { get; private set; } = null!;

        #endregion

        #region protected functions

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Safety Hosted Service is running.{Environment.NewLine}");

            _eventMessagesProcessingAddons = _addonsManager.Addons.OfType<EventMessagesProcessingAddonBase>().OrderBy(a => a.IsDummy).ToArray();

            CsvDb = ActivatorUtilities.CreateInstance<CsvDb>(
                _serviceProvider, ServerConfigurationHelper.GetProgramDataDirectoryInfo(_configuration), ThreadSafeDispatcher);

            _localDataAccessProvider.Initialize(null,
                true,
                true,
                @"",
                @"Simcode.PazCheck.CentralServer",
                Environment.MachineName,
                @"",
                new CaseInsensitiveDictionary<string?>(),
                ThreadSafeDispatcher);
            _localDataAccessProvider.EventMessagesCallback += DataAccessProviderOnEventMessagesCallback;
            _dataAccessProvidersCollection.Add(_localDataAccessProvider);
            var dataAccessProviderAddonsCollection = _addonsManager.Addons.OfType<DataAccessProviderAddonBase>().OrderBy(a => a.IsDummy).ToArray();            
            foreach (var dataAccessProviderAddon in dataAccessProviderAddonsCollection)
            {                
                var dataAccessProvider = dataAccessProviderAddon.GetDataAccessProvider(ThreadSafeDispatcher);
                if (dataAccessProvider is null)                
                    continue;
               dataAccessProvider.EventMessagesCallback += DataAccessProviderOnEventMessagesCallback;
                _dataAccessProvidersCollection.Add(dataAccessProvider);
                var eventSourceModel = new EventSourceModel();
                eventSourceModel.Initialize(dataAccessProvider);
                dataAccessProvider.Obj = eventSourceModel;
                _eventSourceModelsCollection.Add(eventSourceModel);
            }

            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

            await Task.Delay(5000);
            Unit? unit = _dbContext.Units.FirstOrDefault(u => u.Title == LoadFixtures.DefaultUnitTitle);
            if (unit == null)
            {
                _logger.LogError("Invalid UnitId.");
                return;
            }
            foreach (var eventSourceModel in _eventSourceModelsCollection)
            {
                FillInEventSourceModelAndDb(unit, eventSourceModel);                
            }

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(1000);
                if (cancellationToken.IsCancellationRequested) break;

                await ThreadSafeDispatcher.InvokeActionsInQueueAsync(cancellationToken);

                DateTime nowUtc = DateTime.UtcNow;

                //if (modelTimeValueSubscription.ValueStatusTimestamp.ValueStatusCode == ValueStatusCode.Good)
                //{
                //    int modelTimeSeconds = modelTimeValueSubscription.ValueStatusTimestamp.Value.ValueAsInt32(false);
                //    if (modelTimeSeconds > 0)
                //        device.DoWork((UInt64)modelTimeSeconds * 1000, nowUtc, cancellationToken);
                //}
                //

                await DoWorkAsync(nowUtc, cancellationToken);
            }

            foreach (var dataAccessProvider in _dataAccessProvidersCollection)
            {
                dataAccessProvider.Close();                
            }

            foreach (var eventSourceModel in _eventSourceModelsCollection)
            {                
                eventSourceModel.Clear();
            }

            foreach (var dataAccessClientAddon in dataAccessProviderAddonsCollection)
            {
                dataAccessClientAddon.Close();
            }
        }

        #endregion

        #region private functions        

        private Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            //if (nowUtc < _lastWorkDateTimeUtc + TimeSpan.FromSeconds(1)) return Task.CompletedTask;
            //_lastWorkDateTimeUtc = nowUtc;

            foreach (var eventSourceModel in _eventSourceModelsCollection)
            {
                eventSourceModel.OnAlarmsListChanged();
            }            

            _dbContext.SaveChanges();

            return Task.CompletedTask;
        }

        private void FillInEventSourceModelAndDb(Unit unit, EventSourceModel eventSourceModel)
        {
            unit.Sections.Clear();
            
            var values = CsvDb.GetValues("SafetyTags.csv");
            foreach (var kvp in values)
            {
                var line = kvp.Value;
                if (line.Count < 3) continue;
                string tag = line[0] ?? "";
                string type = line[1] ?? "";
                double k = new Any(line[2]).ValueAsDouble(false);
                if (k == 0.0) k = 1.0;
                string area = line.Count >= 4 ? line[3] ?? "" : "";
                if (!String.Equals(type, "tag", StringComparison.InvariantCultureIgnoreCase)) continue;

                var fullAreaList = new List<string>();
                while (area != "")
                {
                    fullAreaList.Add(area);
                    area = CsvDb.GetValue("SafetyTags.csv", area, 3) ?? "";
                }
                string fullArea = "";
                if (fullAreaList.Count > 0)
                {
                    fullArea = String.Join('/', fullAreaList.AsEnumerable().Reverse().ToArray());
                }

                var tagSection = new Section
                {
                    Title = tag,
                    K = k
                };
                unit.Sections.Add(tagSection);

                EventSourceObject eventSourceObject = eventSourceModel.GetOrCreateEventSourceObject(tag, fullArea);
                eventSourceObject.Obj = tagSection;
                eventSourceObject.Subscriptions.Add(tagSection, new EventSourceModelSubscriptionInfo(EventSourceModel.AlarmMaxCategoryId_SubscriptionType, EventSourceModelSubscriptionScope.Active));                

                if (line.Count >= 5)
                {
                    string prop = line[4] ?? "";
                    if (prop == "") continue;
                    string ll = "";
                    if (line.Count >= 6) ll = line[5] ?? "";
                    string l = "";
                    if (line.Count >= 7) l = line[6] ?? "";
                    string h = "";
                    if (line.Count >= 8) h = line[7] ?? "";
                    string hh = "";
                    if (line.Count >= 9) hh = line[8] ?? "";
                    string vh = "";
                    if (line.Count >= 10) vh = line[9] ?? "";
                    string vhh = "";
                    if (line.Count >= 11) vhh = line[10] ?? "";

                    AddValueSubscriptions(tag, prop, ll, l, h, hh, vh, vhh, eventSourceModel);
                }
            }

            foreach (var kvp in eventSourceModel.EventSourceAreas)
            {
                EventSourceArea eventSourceArea = kvp.Value;

                var area = eventSourceArea.Area.Split('/').Last();
                double k = new Any(CsvDb.GetValue("SafetyTags.csv", area, 2)).ValueAsDouble(false);
                if (k == 0.0) k = 1.0;
                var areaSection = new Section
                {
                    Title = area,
                    K = k
                };
                unit.Sections.Add(areaSection);

                eventSourceArea.Obj = areaSection;
                eventSourceArea.Subscriptions.Add(areaSection, new EventSourceModelSubscriptionInfo(EventSourceModel.AlarmMaxCategoryId_SubscriptionType, EventSourceModelSubscriptionScope.Active));                
            }

            foreach (var kvp in eventSourceModel.EventSourceObjects)
            {
                EventSourceObject eventSourceObject = kvp.Value;
                Section tagSection = eventSourceObject.Obj as Section ?? throw new InvalidOperationException();

                var areas = new Dictionary<int, EventSourceArea>();
                foreach (var kvp2 in eventSourceObject.EventSourceAreas)
                {
                    if (kvp2.Key == @"")
                    {
                        areas[0] = kvp2.Value;
                    }
                    else
                    {
                        var areaParts = kvp2.Key.Split('/');
                        areas[areaParts.Length] = kvp2.Value;
                    }
                }
                foreach (var kvp2 in areas.OrderByDescending(kvp3 => kvp3.Key))
                {
                    Section areaSection = kvp2.Value.Obj as Section ?? throw new InvalidOperationException();
                    if (areaSection.Children.Contains(tagSection)) break;
                    areaSection.Children.Add(tagSection);
                    tagSection.Parent = areaSection;
                    tagSection.Level = kvp2.Key + 1;

                    tagSection = areaSection;
                }
            }

            _dbContext.SaveChanges();
        }

        private void AddValueSubscriptions(string tag, string prop, string ll, string l, string h, string hh, string vh, string vhh, EventSourceModel eventSourceModel)
        {
            var dataAccessProvider = eventSourceModel.DataAccessProvider;
            if (ll != "")
            {
                double v = new Any(ll).ValueAsDouble(false);
                _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                {
                    if (ValueStatusCode.IsGood(newVal.ValueStatusCode))
                    {
                        bool active = newVal.Value.ValueAsDouble(false) <= v;
                        SendEventMessage(tag, AlarmConditionType.LowLow, active, 2, eventSourceModel);
                    }
                }));
            }
            if (l != "")
            {
                double v = new Any(l).ValueAsDouble(false);
                _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                {
                    if (ValueStatusCode.IsGood(newVal.ValueStatusCode))
                    {
                        bool active = newVal.Value.ValueAsDouble(false) <= v;
                        SendEventMessage(tag, AlarmConditionType.Low, active, 1, eventSourceModel);
                    }
                }));
            }
            if (h != "")
            {
                double v = new Any(h).ValueAsDouble(false);
                _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                {
                    if (ValueStatusCode.IsGood(newVal.ValueStatusCode))
                    {
                        bool active = newVal.Value.ValueAsDouble(false) >= v;
                        SendEventMessage(tag, AlarmConditionType.High, active, 1, eventSourceModel);
                    }
                }));
            }
            if (hh != "")
            {
                double v = new Any(hh).ValueAsDouble(false);
                _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                {
                    if (ValueStatusCode.IsGood(newVal.ValueStatusCode))
                    {
                        bool active = newVal.Value.ValueAsDouble(false) >= v;
                        SendEventMessage(tag, AlarmConditionType.HighHigh, active, 2, eventSourceModel);
                    }
                }));
            }
            if (vh != "")
            {
                double v = new Any(vh).ValueAsDouble(false);
                if (v > 0)
                {
                    _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                    {
                        if (ValueStatusCode.IsGood(newVal.ValueStatusCode) &&
                            ValueStatusCode.IsGood(oldVal.ValueStatusCode) &&
                            newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) >= v)
                            SendEventMessage(tag, AlarmConditionType.PositiveRate, true, 1, eventSourceModel);
                    }));
                }
                else if (v < 0)
                {
                    _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                    {
                        if (ValueStatusCode.IsGood(newVal.ValueStatusCode) &&
                            ValueStatusCode.IsGood(oldVal.ValueStatusCode) &&
                            newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) < v)
                            SendEventMessage(tag, AlarmConditionType.NegativeRate, true, 1, eventSourceModel);
                    }));
                }
            }
            if (vhh != "")
            {
                double v = new Any(vhh).ValueAsDouble(false);
                if (v > 0)
                {
                    _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                    {
                        if (ValueStatusCode.IsGood(newVal.ValueStatusCode) &&
                            ValueStatusCode.IsGood(oldVal.ValueStatusCode) &&
                            newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) >= v)
                            SendEventMessage(tag, AlarmConditionType.PositiveRate, true, 2, eventSourceModel);
                    }));
                }
                else if (v < 0)
                {
                    _valueSubscriptionsCollection.Add(new ValueSubscription(dataAccessProvider, tag + prop, (oldVal, newVal) =>
                    {
                        if (ValueStatusCode.IsGood(newVal.ValueStatusCode) &&
                            ValueStatusCode.IsGood(oldVal.ValueStatusCode) &&
                            newVal.Value.ValueAsDouble(false) - oldVal.Value.ValueAsDouble(false) < v)
                            SendEventMessage(tag, AlarmConditionType.NegativeRate, true, 2, eventSourceModel);
                    }));
                }
            }
        }

        private readonly List<ValueSubscription> _valueSubscriptionsCollection = new();

        private void SendEventMessage(string tag, AlarmConditionType alarmConditionType, bool active, uint alarmCategoryId, EventSourceModel eventSourceModel)
        {
            EventSourceObject eventSourceObject = eventSourceModel.GetOrCreateEventSourceObject(tag, "");

            eventSourceModel.ProcessEventSourceObject(eventSourceObject, alarmConditionType, alarmCategoryId,
                        active, true, DateTime.UtcNow, out bool alarmConditionChanged, out bool unackedChanged);
        }

        private async void DataAccessProviderOnEventMessagesCallback(IDataAccessProvider dataAccessProvider, EventMessagesCollection eventMessagesCollection)
        {
            if (eventMessagesCollection.EventMessages.Count == 0)
                return;

            string? sourceSystemName = eventMessagesCollection.CommonFields?.TryGetValue(@"SourceSystemName");
            if (String.IsNullOrEmpty(sourceSystemName))
                return;

            string? unitId = eventMessagesCollection.CommonFields?.TryGetValue(@"UnitId");
            if (String.IsNullOrEmpty(unitId))
                return;

            using var dbContext = new PazCheckDbContext();
            Unit unit;
            try
            {
                unit = dbContext.Units.Single(u => u.Id == unitId);
            }
            catch
            {
                _logger.LogError("Invali unitId: {0}", unitId);
                return;
            }

            //var eventMessagesArray = eventMessagesCollection.EventMessages.Where(em => em != null).OrderBy(em => em.OccurrenceTimeUtc).ToArray();

            //var eventMessagesProcessingAddon = _eventMessagesProcessingAddons?.FirstOrDefault(a => a.CanAddToEventSourceModelEventMessageFrom(sourceSystemName));
            //if (eventMessagesProcessingAddon is not null)
            //{
            //    EventSourceModel? eventSourceModel = dataAccessProvider.Obj as EventSourceModel;
            //    if (eventSourceModel is not null)
            //    {                    
            //        foreach (EventMessage eventMessage in eventMessagesArray)
            //        {
            //            await eventMessagesProcessingAddon.AddToEventSourceModelAsync(eventSourceModel, eventMessage);
            //        }
            //    }
            //}

            var eventMessagesProcessingAddon = _eventMessagesProcessingAddons?.FirstOrDefault(a => a.CanProcessEventMessageFrom(sourceSystemName));
            if (eventMessagesProcessingAddon is not null)
            { 
                UnitEventsInterval? unitEventsInterval = await eventMessagesProcessingAddon.ProcessEventMessagesAsync(sourceSystemName, eventMessagesCollection.EventMessages, @"Autoimported", CancellationToken.None, null);
                if (unitEventsInterval is not null)
                {
                    string? sourceAddonId = eventMessagesCollection.CommonFields?.TryGetValue(@"SourceAddonId");
                    if (!String.IsNullOrEmpty(sourceAddonId))
                    {
                        try
                        {
                            var t = dataAccessProvider.PassthroughAsync(sourceAddonId, @"SetAddonOptions", Encoding.UTF8.GetBytes(
                                NameValueCollectionHelper.GetNameValueCollectionString(new CaseInsensitiveDictionary<string?>()
                                {
                                    { @"%(MaxProcessedTimeUtc)", new Any(unitEventsInterval.EndTimeUtc).ValueAsString(false, @"O") }
                                })
                            ));
                        }
                        catch
                        {
                        }
                    }
                    unit.UnitEventsIntervals.Add(unitEventsInterval);
                    await dbContext.SaveChangesAsync();
                }                
            }               
        }

        #endregion        

        #region private fields
        
        private readonly IServiceProvider _serviceProvider;
        private readonly AddonsManager _addonsManager;
        private readonly IServiceScope _serviceScope;
        private readonly PazCheckDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly LocalDataAccessProvider _localDataAccessProvider;

        private EventMessagesProcessingAddonBase[]? _eventMessagesProcessingAddons;

        private readonly List<IDataAccessProvider> _dataAccessProvidersCollection = new();

        private readonly List<EventSourceModel> _eventSourceModelsCollection = new();

        #endregion
    }
}


//List<AlarmInfoViewModelBase> newAlarmInfoViewModels = new List<AlarmInfoViewModelBase>();
//foreach (EventMessage eventMessage in newEventMessages.Where(em => em != null).OrderBy(em => em.OccurrenceTime))
//{
//    var alarmInfoViewModels = await DeltaSimHelper.ProcessEventMessage(_eventSourceModel, eventMessage);
//    if (alarmInfoViewModels != null)
//    {
//        newAlarmInfoViewModels.AddRange(alarmInfoViewModels);
//    }
//}
//if (newAlarmInfoViewModels.Count > 0)
//{
//    //AlarmsListViewModel.OnAlarmNotification(newAlarmInfoViewModels);

//    _eventSourceModel.OnAlarmsListChanged();
//}


//var rootEventSourceArea = _eventSourceModel.EventSourceAreas.TryGetValue("");
//if (rootEventSourceArea != null)
//{
//    Section rootAreaSection = rootEventSourceArea.Obj as Section ?? throw new InvalidOperationException();
//    ComputeWidthAndHeightForChilds(rootAreaSection);
//}


//private void ComputeWidthAndHeightForChilds(Section section)
//{
//    var count = section.Children.Count;
//    if (count == 0) return;
//    //double sumK = section.Children.Sum(s => s.K);

//    //Solver solver = Solver.CreateSolver("GLOP");
//    //Variable w = solver.MakeNumVar(0.0, 1.0, "w");
//    //Variable h = solver.MakeNumVar(0.0, 1.0, "h");
//    //solver.Add(w - h >= 0.0);
//    //solver.Minimize(w - h);
//    //Solver.ResultStatus resultStatus = solver.Solve();
//    //if (resultStatus != Solver.ResultStatus.OPTIMAL)
//    //{
//    //    _logger.LogError("The problem does not have an optimal solution!");
//    //    return;
//    //}

//    //double widthPercent = w.SolutionValue() * 100;
//    //double heightPercent = h.SolutionValue() * 100;
//    //foreach (var subSection in section.Children)
//    //{
//    //    subSection.Width = widthPercent * subSection.K;
//    //    subSection.Height = heightPercent;
//    //}
//}


//public EventMessage GetEventMessage()
//{
//    var eventInfo = new EventMessage(new EventId { });
//    eventInfo.OccurrenceTime = OccurrenceTime.ToDateTime();
//    eventInfo.EventType = (EventType)EventType;
//    eventInfo.TextMessage = TextMessage;
//    eventInfo.CategoryId = CategoryId;
//    eventInfo.Priority = Priority;
//    eventInfo.OperatorName = OperatorName;
//    if (OptionalAlarmDataCase == OptionalAlarmDataOneofCase.AlarmMessageData)
//    {
//        eventInfo.AlarmMessageData = AlarmMessageData.ToAlarmMessageData();
//    }
//    if (ClientRequestedFields.Count > 0)
//    {
//        eventInfo.ClientRequestedFields = new Utils.CaseInsensitiveDictionary<string>(ClientRequestedFields);
//    }
//    return eventInfo;
//}