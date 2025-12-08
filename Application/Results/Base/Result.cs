using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Results.Base
{
    public class Result
    {
        public bool IsSucceed { get; private set; }
        public string? ErrorMessage { get; private set; }

        internal Result(bool isSucceed, string errorMessage)
        {
            IsSucceed = isSucceed;

            ErrorMessage = errorMessage;
        }

        public static Result Success()
        {
            return new Result(true, string.Empty);
        }
        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }
}
