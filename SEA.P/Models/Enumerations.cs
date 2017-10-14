using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEA.P.Models.Enum
{
    public enum TableQueryCode : int
    {
        Success = 1,
        InternalError = 2,
        InvalidRequest = 3,
        QueryError = 4,
        ValueExist = 5,
        ValueNotExist = 6
    }

    public enum ErrorCode : int
    {
        InternalError = -101,
        Invalid = -102,
        Exist = -103,
        NotExist = -104
    }
    public enum GameSessionStatus : byte
    {
        Offline = 0,
        Online = 1,
        Standby = 2,
        ProcessNotExist = 3,
        FileNotExist = 4,
        KeyNotExist = 5,
        KeyNotFoundInMemory = 6
    }
    public enum ExecuteCode : byte
    {
        NotInit = 0,
        Timeout = 2,
        Success = 10,
    }

    public static class ErrorCodeExtention
    {
        public static int ToInt(this ErrorCode self) => (int)self;
    }
}
