using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace RCCArbiter
{
    // Data Contracts
    [DataContract(Namespace = "http://roblox.com/")]
    public class Job
    {
        [DataMember(Order = 0)]
        public string id { get; set; } = string.Empty;

        [DataMember(Order = 1)]
        public double expirationInSeconds { get; set; }

        [DataMember(Order = 2)]
        public int category { get; set; }

        [DataMember(Order = 3)]
        public double cores { get; set; }
    }

    [DataContract(Namespace = "http://roblox.com/")]
    public enum LuaType
    {
        [EnumMember]
        LUA_TNIL,
        [EnumMember]
        LUA_TBOOLEAN,
        [EnumMember]
        LUA_TNUMBER,
        [EnumMember]
        LUA_TSTRING,
        [EnumMember]
        LUA_TTABLE
    }

    [DataContract(Namespace = "http://roblox.com/")]
    public class LuaValue
    {
        [DataMember(Order = 0)]
        public LuaType type { get; set; }

        [DataMember(Order = 1, IsRequired = false)]
        public string? value { get; set; }

        [DataMember(Order = 2, IsRequired = false)]
        public LuaValue[]? table { get; set; }

        public LuaValue()
        {
            type = LuaType.LUA_TNIL;
        }
    }

    [DataContract(Namespace = "http://roblox.com/")]
    public class ScriptExecution
    {
        [DataMember(Order = 0, IsRequired = false)]
        public string? name { get; set; }

        [DataMember(Order = 1, IsRequired = false)]
        public string? script { get; set; }

        [DataMember(Order = 2, IsRequired = false)]
        public LuaValue[]? arguments { get; set; }
    }

    [DataContract(Namespace = "http://roblox.com/")]
    public class Status
    {
        [DataMember(Order = 0, IsRequired = false)]
        public string? version { get; set; }

        [DataMember(Order = 1)]
        public int environmentCount { get; set; }
    }

    // Service Contract
    [ServiceContract(Namespace = "http://roblox.com/", ConfigurationName = "RCCServiceSoap")]
    public interface IRCCServiceSoap
    {
        [OperationContract(Action = "http://roblox.com/HelloWorld", ReplyAction = "*")]
        string HelloWorld();

        [OperationContract(Action = "http://roblox.com/GetVersion", ReplyAction = "*")]
        string GetVersion();

        [OperationContract(Action = "http://roblox.com/GetStatus", ReplyAction = "*")]
        Status GetStatus();

        [OperationContract(Action = "http://roblox.com/OpenJob", ReplyAction = "*")]
        LuaValue[] OpenJob(Job job, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/OpenJobEx", ReplyAction = "*")]
        LuaValue[] OpenJobEx(Job job, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/BatchJob", ReplyAction = "*")]
        LuaValue[] BatchJob(Job job, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/BatchJobEx", ReplyAction = "*")]
        LuaValue[] BatchJobEx(Job job, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/Execute", ReplyAction = "*")]
        LuaValue[] Execute(string jobID, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/ExecuteEx", ReplyAction = "*")]
        LuaValue[] ExecuteEx(string jobID, ScriptExecution script);

        [OperationContract(Action = "http://roblox.com/RenewLease", ReplyAction = "*")]
        double RenewLease(string jobID, double expirationInSeconds);

        [OperationContract(Action = "http://roblox.com/CloseJob", ReplyAction = "*")]
        void CloseJob(string jobID);

        [OperationContract(Action = "http://roblox.com/GetExpiration", ReplyAction = "*")]
        double GetExpiration(string jobID);

        [OperationContract(Action = "http://roblox.com/GetAllJobs", ReplyAction = "*")]
        Job[] GetAllJobs();

        [OperationContract(Action = "http://roblox.com/GetAllJobsEx", ReplyAction = "*")]
        Job[] GetAllJobsEx();

        [OperationContract(Action = "http://roblox.com/CloseExpiredJobs", ReplyAction = "*")]
        int CloseExpiredJobs();

        [OperationContract(Action = "http://roblox.com/CloseAllJobs", ReplyAction = "*")]
        int CloseAllJobs();

        [OperationContract(Action = "http://roblox.com/Diag", ReplyAction = "*")]
        LuaValue[] Diag(int type, string jobID);

        [OperationContract(Action = "http://roblox.com/DiagEx", ReplyAction = "*")]
        LuaValue[] DiagEx(int type, string jobID);
    }
}
